using ConstructionJournal.Interfaces;
using ConstructionJournal.Models;
using ConstructionJournal.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApp.Models;

namespace ConstructionJournal.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkJournalController : ControllerBase
    {
        private readonly IWorkJournalService _workJournalService;
        private readonly IGeoLocationService _geoLocationService;
        private readonly ILogger<WorkJournalController> _logger;

        public WorkJournalController(
            IWorkJournalService workJournalService,
            IGeoLocationService geoLocationService,
            ILogger<WorkJournalController> logger)
        {
            _workJournalService = workJournalService;
            _geoLocationService = geoLocationService;
            _logger = logger;
        }

        // GET: api/workjournal/project/5
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<ApiResponse<List<WorkJournalEntry>>>> GetByProject(int projectId)
        {
            try
            {
                var entries = await _workJournalService.GetEntriesByProjectAsync(projectId);
                return Ok(ApiResponse<List<WorkJournalEntry>>.Success(entries));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving work journal entries for project {ProjectId}", projectId);
                return BadRequest(ApiResponse<List<WorkJournalEntry>>.Error($"Error retrieving entries: {ex.Message}"));
            }
        }

        // GET: api/workjournal/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<WorkJournalEntry>>> Get(int id)
        {
            try
            {
                var entry = await _workJournalService.GetEntryByIdAsync(id);
                if (entry == null)
                {
                    return NotFound(ApiResponse<WorkJournalEntry>.Error("Entry not found"));
                }
                return Ok(ApiResponse<WorkJournalEntry>.Success(entry));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving work journal entry {EntryId}", id);
                return BadRequest(ApiResponse<WorkJournalEntry>.Error($"Error retrieving entry: {ex.Message}"));
            }
        }

        // POST: api/workjournal
        [HttpPost]
        [Authorize(Roles = "Contractor,Admin")]
        public async Task<ActionResult<ApiResponse<WorkJournalEntry>>> Create([FromBody] CreateWorkJournalEntryDto createDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Validate geo coordinates
                if (createDto.GeoLatitude == null || createDto.GeoLongitude == null)
                {
                    return BadRequest(ApiResponse<WorkJournalEntry>.Error("Geo coordinates are required"));
                }

                // Get address from coordinates
                string geoAddress = null;
                try
                {
                    geoAddress = await _geoLocationService.GetAddressFromCoordinatesAsync(
                        createDto.GeoLatitude.Value,
                        createDto.GeoLongitude.Value);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not get address from coordinates, continuing without address");
                }

                var entry = new WorkJournalEntry
                {
                    ProjectId = createDto.ProjectId,
                    WorkTypeId = createDto.WorkTypeId,
                    ContractorUserId = userId,
                    WorkDate = createDto.WorkDate,
                    Description = createDto.Description,
                    CompletedVolume = createDto.CompletedVolume,
                    GeoLatitude = createDto.GeoLatitude,
                    GeoLongitude = createDto.GeoLongitude,
                    GeoAccuracy = createDto.GeoAccuracy,
                    GeoAddress = geoAddress,
                    Status = WorkJournalStatus.Submitted,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdEntry = await _workJournalService.CreateEntryAsync(entry);

                _logger.LogInformation("Work journal entry created: {EntryId} by user {UserId}",
                    createdEntry.Id, userId);

                return CreatedAtAction(nameof(Get), new { id = createdEntry.Id },
                    ApiResponse<WorkJournalEntry>.Success(createdEntry));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work journal entry");
                return BadRequest(ApiResponse<WorkJournalEntry>.Error($"Error creating entry: {ex.Message}"));
            }
        }

        // PUT: api/workjournal/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Contractor,Admin")]
        public async Task<ActionResult<ApiResponse<WorkJournalEntry>>> Update(int id, [FromBody] UpdateWorkJournalEntryDto updateDto)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingEntry = await _workJournalService.GetEntryByIdAsync(id);

                if (existingEntry == null)
                {
                    return NotFound(ApiResponse<WorkJournalEntry>.Error("Entry not found"));
                }

                // Check if user owns the entry or is admin
                if (existingEntry.ContractorUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                existingEntry.Description = updateDto.Description ?? existingEntry.Description;
                existingEntry.CompletedVolume = updateDto.CompletedVolume ?? existingEntry.CompletedVolume;
                existingEntry.Status = updateDto.Status ?? existingEntry.Status;
                existingEntry.UpdatedAt = DateTime.UtcNow;

                var updatedEntry = await _workJournalService.UpdateEntryAsync(existingEntry);

                _logger.LogInformation("Work journal entry updated: {EntryId} by user {UserId}",
                    id, userId);

                return Ok(ApiResponse<WorkJournalEntry>.Success(updatedEntry));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating work journal entry {EntryId}", id);
                return BadRequest(ApiResponse<WorkJournalEntry>.Error($"Error updating entry: {ex.Message}"));
            }
        }

        // DELETE: api/workjournal/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Contractor,Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingEntry = await _workJournalService.GetEntryByIdAsync(id);

                if (existingEntry == null)
                {
                    return NotFound(ApiResponse<bool>.Error("Entry not found"));
                }

                // Check if user owns the entry or is admin
                if (existingEntry.ContractorUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                await _workJournalService.DeleteEntryAsync(id);

                _logger.LogInformation("Work journal entry deleted: {EntryId} by user {UserId}",
                    id, userId);

                return Ok(ApiResponse<bool>.Success(true, "Entry deleted successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting work journal entry {EntryId}", id);
                return BadRequest(ApiResponse<bool>.Error($"Error deleting entry: {ex.Message}"));
            }
        }

        // POST: api/workjournal/5/photos
        [HttpPost("{id}/photos")]
        [Authorize(Roles = "Contractor,Admin")]
        public async Task<ActionResult<ApiResponse<WorkJournalPhoto>>> AddPhoto(int id, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(ApiResponse<WorkJournalPhoto>.Error("No file provided"));
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest(ApiResponse<WorkJournalPhoto>.Error("Invalid file type. Only images are allowed."));
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingEntry = await _workJournalService.GetEntryByIdAsync(id);

                if (existingEntry == null)
                {
                    return NotFound(ApiResponse<WorkJournalPhoto>.Error("Entry not found"));
                }

                // Check if user owns the entry or is admin
                if (existingEntry.ContractorUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var photo = await _workJournalService.AddPhotoToEntryAsync(id, file);

                _logger.LogInformation("Photo added to work journal entry {EntryId} by user {UserId}",
                    id, userId);

                return Ok(ApiResponse<WorkJournalPhoto>.Success(photo, "Photo added successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding photo to work journal entry {EntryId}", id);
                return BadRequest(ApiResponse<WorkJournalPhoto>.Error($"Error adding photo: {ex.Message}"));
            }
        }

        // GET: api/workjournal/5/photos
        [HttpGet("{id}/photos")]
        public async Task<ActionResult<ApiResponse<List<WorkJournalPhoto>>>> GetPhotos(int id)
        {
            try
            {
                var photos = await _workJournalService.GetEntryPhotosAsync(id);
                return Ok(ApiResponse<List<WorkJournalPhoto>>.Success(photos));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving photos for work journal entry {EntryId}", id);
                return BadRequest(ApiResponse<List<WorkJournalPhoto>>.Error($"Error retrieving photos: {ex.Message}"));
            }
        }
    }
}