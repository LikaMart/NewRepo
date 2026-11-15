using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelerNotes.Entitites;

[Table("Countries")]
public class Country
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }
    [Required]
    public required string Capital { get; set; }

    public string? ImageUrl { get; set; }

    public string? Region { get; set; }

    public List<Trip>? Trips { get; set; }

    public List<Place>? Places { get; set; }
}
