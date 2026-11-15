using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelerNotes.Entitites;

[Table("Places")]
public class Place
{
    [Key]
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    public string? LocationUrl { get; set; }

    public string? WorkStartTime { get; set; }

    public string? WorkEndTime { get; set; }

    [ForeignKey("Country")]
    public int CountryId { get; set; }

    public Country? Country { get; set; }

    public List<Tag>? Tags { get; set; }
}
