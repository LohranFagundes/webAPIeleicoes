using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ElectionApi.Net.DTOs;
using ElectionApi.Net.Services;
using System.Security.Claims;

namespace ElectionApi.Net.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CandidateController : ControllerBase
{
    private readonly ICandidateService _candidateService;
    private readonly IAuditService _auditService;

    public CandidateController(ICandidateService candidateService, IAuditService auditService)
    {
        _candidateService = candidateService;
        _auditService = auditService;
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetCandidates([FromQuery] int page = 1, [FromQuery] int limit = 10,
        [FromQuery] int? positionId = null, [FromQuery] bool? isActive = null)
    {
        try
        {
            var result = await _candidateService.GetCandidatesAsync(page, limit, positionId, isActive);
            
            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                await _auditService.LogAsync(userId.Value, "admin", "list", "candidates");
            }

            return Ok(ApiResponse<PagedResult<CandidateResponseDto>>.SuccessResult(result));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to fetch candidates"));
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetCandidate(int id)
    {
        try
        {
            var candidate = await _candidateService.GetCandidateByIdAsync(id);
            
            if (candidate == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult("Candidate not found"));
            }

            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                await _auditService.LogAsync(userId.Value, "admin", "view", "candidates", id);
            }

            return Ok(ApiResponse<CandidateResponseDto>.SuccessResult(candidate));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to fetch candidate"));
        }
    }

    [HttpGet("position/{positionId}")]
    public async Task<IActionResult> GetCandidatesByPosition(int positionId)
    {
        try
        {
            var candidates = await _candidateService.GetCandidatesByPositionAsync(positionId);
            return Ok(ApiResponse<IEnumerable<CandidateResponseDto>>.SuccessResult(candidates));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to fetch candidates for position"));
        }
    }

    [HttpGet("position/{positionId}/with-votes")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetCandidatesWithVotes(int positionId)
    {
        try
        {
            var candidates = await _candidateService.GetCandidatesWithVotesAsync(positionId);
            return Ok(ApiResponse<IEnumerable<CandidateWithVotesDto>>.SuccessResult(candidates));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to fetch candidates with votes"));
        }
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateCandidate([FromBody] CreateCandidateDto createDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResult("Invalid input data", ModelState));
        }

        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult("User authentication required"));
            }

            var candidate = await _candidateService.CreateCandidateAsync(createDto, userId.Value);
            
            return CreatedAtAction(
                nameof(GetCandidate), 
                new { id = candidate.Id }, 
                ApiResponse<CandidateResponseDto>.SuccessResult(candidate, "Candidate created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResult(ex.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to create candidate"));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateCandidate(int id, [FromBody] UpdateCandidateDto updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.ErrorResult("Invalid input data", ModelState));
        }

        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult("User authentication required"));
            }

            var candidate = await _candidateService.UpdateCandidateAsync(id, updateDto, userId.Value);
            
            if (candidate == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult("Candidate not found"));
            }

            return Ok(ApiResponse<CandidateResponseDto>.SuccessResult(candidate, "Candidate updated successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to update candidate"));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteCandidate(int id)
    {
        try
        {
            var result = await _candidateService.DeleteCandidateAsync(id);
            
            if (!result)
            {
                return NotFound(ApiResponse<object>.ErrorResult("Candidate not found"));
            }

            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                await _auditService.LogAsync(userId.Value, "admin", "delete", "candidates", id);
            }

            return Ok(ApiResponse<object>.SuccessResult(null, "Candidate deleted successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to delete candidate"));
        }
    }

    [HttpPost("{id}/upload-photo")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UploadCandidatePhoto(int id, IFormFile photo)
    {
        try
        {
            if (photo == null || photo.Length == 0)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("No photo file provided"));
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Invalid file type. Only JPG, PNG and GIF are allowed"));
            }

            // Validate file size (max 5MB)
            if (photo.Length > 5 * 1024 * 1024)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("File size too large. Maximum size is 5MB"));
            }

            // Check if candidate exists
            var candidate = await _candidateService.GetCandidateByIdAsync(id);
            if (candidate == null)
            {
                return NotFound(ApiResponse<object>.ErrorResult("Candidate not found"));
            }

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "candidates");
            Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileName = $"{id}_{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            // Update candidate with photo URL
            var photoUrl = $"/uploads/candidates/{fileName}";
            var updateDto = new UpdateCandidateDto { PhotoUrl = photoUrl };
            
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResult("User authentication required"));
            }

            var updatedCandidate = await _candidateService.UpdateCandidateAsync(id, updateDto, userId.Value);

            await _auditService.LogAsync(userId.Value, "admin", "upload_photo", "candidates", id, $"Photo uploaded: {fileName}");

            return Ok(ApiResponse<object>.SuccessResult(new { photoUrl }, "Photo uploaded successfully"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult($"Failed to upload photo: {ex.Message}"));
        }
    }

    [HttpPut("position/{positionId}/reorder")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ReorderCandidates(int positionId, [FromBody] Dictionary<int, int> candidateOrders)
    {
        try
        {
            var result = await _candidateService.ReorderCandidatesAsync(positionId, candidateOrders);
            
            if (!result)
            {
                return BadRequest(ApiResponse<object>.ErrorResult("Failed to reorder candidates"));
            }

            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                await _auditService.LogAsync(userId.Value, "admin", "reorder", "candidates", positionId, "Candidates reordered");
            }

            return Ok(ApiResponse<object>.SuccessResult(null, "Candidates reordered successfully"));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse<object>.ErrorResult("Failed to reorder candidates"));
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}