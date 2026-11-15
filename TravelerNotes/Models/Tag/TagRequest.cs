namespace TravelerNotes.Models.Tag;

public class TagRequest
{
    public int PlaceId { get; set; }

    public int TripId { get; set; }

    public DateTime TagTime { get; set; }

    public string? Comment { get; set; }
}
