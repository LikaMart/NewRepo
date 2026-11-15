using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelerNotes.Entitites;

[Table("Trips")]
public class Trip
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Country")]
    public int CountryId { get; set; }

    public Country? Country { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndtDate { get; set; }

    [Range(0,6)]
    public int Rating { get; set; }

    public string? Notes  { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }

    public User? User { get; set; }

    public List<Tag>? Tags { get; set; }
}
