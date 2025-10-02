using ConstructionJournal.Interfaces;
using ConstructionJournal.Models;
using ConstructionJournal.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.LLM;
using WebApp.Models;

namespace ConstructionJournal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MaterialDeliveryController : ControllerBase
    {
        private readonly IMaterialService _materialService;
        private readonly IMaterialInspectionService _materialInspectionService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<MaterialDeliveryController> _logger;

        public MaterialDeliveryController(
            IMaterialService materialService,
            IMaterialInspectionService materialInspectionService,
            IFileStorageService fileStorageService,
            ILogger<MaterialDeliveryController> logger)
        {
            _materialService = materialService;
            _materialInspectionService = materialInspectionService;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // POST: api/materialdelivery
        [HttpPost]
        [Authorize(Roles = "Contractor,Admin")]
        public async Task<ActionResult<ApiResponse<MaterialDelivery>>> Create([FromBody] CreateMaterialDeliveryDto createDto)
        {
            try
            {
                var delivery = new MaterialDelivery
                {
                    ProjectId = createDto.ProjectId,
                    MaterialId = createDto.MaterialId,
                    DeliveryDate = createDto.DeliveryDate,
                    Supplier = createDto.Supplier,
                    BatchNumber = createDto.BatchNumber,
                    Quantity = createDto.Quantity,
                    QualityStatus = "Pending",
                    Status = MaterialDeliveryStatus.Delivered,
                    CreatedAt = DateTime.UtcNow
                };

                var createdDelivery = await _materialService.CreateMaterialDeliveryAsync(delivery);

                _logger.LogInformation("Material delivery created: {DeliveryId}", createdDelivery.Id);

                return CreatedAtAction(nameof(Get), new { id = createdDelivery.Id },
                    ApiResponse<MaterialDelivery>.Success(createdDelivery));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating material delivery");
                return BadRequest(ApiResponse<MaterialDelivery>.Error($"Error creating delivery: {ex.Message}"));
            }
        }

        // GET: api/materialdelivery/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<MaterialDelivery>>> Get(int id)
        {
            try
            {
                var delivery = await _materialService.GetMaterialDeliveryByIdAsync(id);
                if (delivery == null)
                {
                    return NotFound(ApiResponse<MaterialDelivery>.Error("Material delivery not found"));
                }
                return Ok(ApiResponse<MaterialDelivery>.Success(delivery));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving material delivery {DeliveryId}", id);
                return BadRequest(ApiResponse<MaterialDelivery>.Error($"Error retrieving delivery: {ex.Message}"));
            }
        }

        // POST: api/materialdelivery/5/quality-documents
        [HttpPost("{id}/quality-documents")]
        [Authorize(Roles = "Contractor,Admin")]
        public async Task<ActionResult<ApiResponse<MaterialDelivery>>> UploadQualityDocument(int id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<MaterialDelivery>.Error("No file provided"));
                }

                // Validate file type
                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(ApiResponse<MaterialDelivery>.Error("Invalid file type. Allowed: PDF, Word, Images"));
                }

                var delivery = await _materialService.GetMaterialDeliveryByIdAsync(id);
                if (delivery == null)
                {
                    return NotFound(ApiResponse<MaterialDelivery>.Error("Material delivery not found"));
                }

                // Upload file to storage
                var filePath = await _fileStorageService.SaveFileAsync(file, "quality-documents");

                // Update delivery with document path
                var documents = string.IsNullOrEmpty(delivery.QualityDocuments)
                    ? new List<string>()
                    : System.Text.Json.JsonSerializer.Deserialize<List<string>>(delivery.QualityDocuments);

                documents.Add(filePath);
                delivery.QualityDocuments = System.Text.Json.JsonSerializer.Serialize(documents);

                var updatedDelivery = await _materialService.UpdateMaterialDeliveryAsync(delivery);

                // Start LLM analysis in background
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _materialInspectionService.AnalyzeMaterialComplianceAsync(delivery, filePath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in background LLM analysis for delivery {DeliveryId}", id);
                    }
                });

                _logger.LogInformation("Quality document uploaded for delivery {DeliveryId}", id);

                return Ok(ApiResponse<MaterialDelivery>.Success(updatedDelivery, "Quality document uploaded and analysis started"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading quality document for delivery {DeliveryId}", id);
                return BadRequest(ApiResponse<MaterialDelivery>.Error($"Error uploading document: {ex.Message}"));
            }
        }

        // POST: api/materialdelivery/5/analyze
        [HttpPost("{id}/analyze")]
        [Authorize(Roles = "Client,Regulator,Admin")]
        public async Task<ActionResult<ApiResponse<DocumentAnalysisResult>>> AnalyzeWithLLM(int id)
        {
            try
            {
                var delivery = await _materialService.GetMaterialDeliveryByIdAsync(id);
                if (delivery == null)
                {
                    return NotFound(ApiResponse<DocumentAnalysisResult>.Error("Material delivery not found"));
                }

                if (string.IsNullOrEmpty(delivery.QualityDocuments))
                {
                    return BadRequest(ApiResponse<DocumentAnalysisResult>.Error("No quality documents available for analysis"));
                }

                var analysisResult = await _materialInspectionService.AnalyzeMaterialComplianceAsync(delivery);

                _logger.LogInformation("LLM analysis completed for delivery {DeliveryId}", id);

                return Ok(ApiResponse<DocumentAnalysisResult>.Success(analysisResult, "LLM analysis completed"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing LLM analysis for delivery {DeliveryId}", id);
                return BadRequest(ApiResponse<DocumentAnalysisResult>.Error($"Error performing analysis: {ex.Message}"));
            }
        }

        // GET: api/materialdelivery/project/5
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<ApiResponse<List<MaterialDelivery>>>> GetByProject(int projectId)
        {
            try
            {
                var deliveries = await _materialService.GetMaterialDeliveriesByProjectAsync(projectId);
                return Ok(ApiResponse<List<MaterialDelivery>>.Success(deliveries));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving material deliveries for project {ProjectId}", projectId);
                return BadRequest(ApiResponse<List<MaterialDelivery>>.Error($"Error retrieving deliveries: {ex.Message}"));
            }
        }
    }
}