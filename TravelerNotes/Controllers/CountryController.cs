using Microsoft.AspNetCore.Mvc;
using TravelerNotes.Db;
using TravelerNotes.Models.Country;

namespace TravelerNotes.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CountryController : ControllerBase
{
    private readonly TravelerNotesDb _db;

    public CountryController()
    {
        _db = new TravelerNotesDb();
    }


    [HttpGet("GetAllCountries")]
    public List<CountryResponse> GetAllCountries()
    {
        return _db.Countries.Select(i => new CountryResponse
        {
            Capital = i.Capital,
            ImageUrl = i.ImageUrl,
            Id = i.Id,
            Name = i.Name,
            Region = i.Region,
        }).ToList();
    }

    [HttpGet("GetCountryById")]
    public CountryResponse GetCountryById(int id)
    {
        var user = _db.Countries.FirstOrDefault(i => i.Id == id);
        if (user == null) throw new ArgumentException("no user found on this Id");
        return new CountryResponse
        {
            Capital = user.Capital,
            Id = user.Id,
            ImageUrl = user.ImageUrl,
            Name = user.Name,
            Region = user.Region,
        };
    }

    [HttpPost("AddNewCountry")]
    public bool AddNewCountry(CountryRequest request)
    {
        if (request is null) throw new ArgumentException("request is null please check");

        _db.Countries.Add(new Entitites.Country
        {
            Capital = request.Capital,
            ImageUrl = request.ImageUrl,
            Name = request.Name,
            Region = request.Region
        });
        _db.SaveChanges();

        return true;
    }

    [HttpDelete("DeleteCountryById")]
    public bool DeleteCountryById(int id)
    {
        var countryExist = _db.Countries.FirstOrDefault(u => u.Id == id);
        if (countryExist == null) return false;
        _db.Countries.Remove(countryExist);
        _db.SaveChanges();
        return true;
    }
}
