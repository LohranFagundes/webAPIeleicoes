using System.ComponentModel.DataAnnotations;

namespace ElectionApi.Net.Models;

public class Candidate : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Number { get; set; }

    [StringLength(100)]
    public string? Party { get; set; }

    public string? Description { get; set; }

    public string? Biography { get; set; }

    [StringLength(255)]
    public string? PhotoUrl { get; set; }

    public int OrderPosition { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    // Foreign Keys
    [Required]
    public int PositionId { get; set; }

    // Navigation properties
    public Position Position { get; set; } = null!;
    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
}