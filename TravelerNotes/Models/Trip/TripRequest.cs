namespace TravelerNotes.Models.Trip;

public class TripRequest
{
   public int CountryId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndtDate { get; set; }

    public int Rating { get; set; }

    public string? Notes { get; set; }

    public int UserId { get; set; }
}
