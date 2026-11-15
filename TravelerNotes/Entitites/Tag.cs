using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelerNotes.Entitites;

[Table("Tags")]
public class Tag
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Place")]
    public int PlaceId { get; set; }

    [ForeignKey("Trip")]
    public int TripId { get; set; }

    public DateTime TagTime { get; set; }

    public string? Comment { get; set; }

    public Place? Place { get; set; }

    public Trip? Trip { get; set; }

}
