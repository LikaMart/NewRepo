namespace TravelerNotes.Models.Country;

public class CountryRequest
{

    public required string Name { get; set; }

    public required string Capital { get; set; }

    public string? ImageUrl { get; set; }

    public string? Region { get; set; }
}
