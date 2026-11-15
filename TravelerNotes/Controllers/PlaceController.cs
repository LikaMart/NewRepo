using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelerNotes.Db;
using TravelerNotes.Models.Place;

namespace TravelerNotes.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlaceController : ControllerBase
{
    private readonly TravelerNotesDb _db;

    public PlaceController()
    {
        _db = new TravelerNotesDb();
    }

    [HttpGet("GetAllPlaces")]
    public List<PlaceResponse> GetAllPlaces()
    {
        return _db.Places.Select(i => new PlaceResponse
        {
            Id = i.Id,
            Name = i.Name,
            LocationUrl = i.LocationUrl,
            WorkStartTime = i.WorkStartTime,
            WorkEndTime = i.WorkEndTime,
            CountryId = i.CountryId
        }).ToList();
    }

    [HttpGet("GetPlaceById")]
    public PlaceResponse GetPlaceById(int id)
    {
        var place = _db.Places.FirstOrDefault(i => i.Id == id);
        if (place == null) throw new ArgumentException("no place found on this Id");
        return new PlaceResponse
        {
            Id = place.Id,
            Name = place.Name,
            LocationUrl = place.LocationUrl,
            WorkStartTime = place.WorkStartTime,
            WorkEndTime = place.WorkEndTime,
            CountryId = place.CountryId
        };
    }

    // კონკრეტული ქვეყნის ადგილები
    [HttpGet("GetPlacesByCountryId")]
    public List<PlaceResponse> GetPlacesByCountryId(int countryId)
    {
        return _db.Places
            .Where(i => i.CountryId == countryId)
            .Select(i => new PlaceResponse
            {
                Id = i.Id,
                Name = i.Name,
                LocationUrl = i.LocationUrl,
                WorkStartTime = i.WorkStartTime,
                WorkEndTime = i.WorkEndTime,
                CountryId = i.CountryId
            }).ToList();
    }

    [HttpPost("AddNewPlace")]
    public bool AddNewPlace(PlaceRequest request)
    {
        if (request is null) throw new ArgumentException("request is null please check");
        if (!_db.Countries.Any(c => c.Id == request.CountryId)) throw new ArgumentException("no country found on this CountryId");

        _db.Places.Add(new Entitites.Place
        {
            Name = request.Name,
            LocationUrl = request.LocationUrl,
            WorkStartTime = request.WorkStartTime,
            WorkEndTime = request.WorkEndTime,
            CountryId = request.CountryId
        });
        _db.SaveChanges();

        return true;
    }

    [HttpPut("UpdatePlaceById")]
    public bool UpdatePlaceById(int id, PlaceRequest request)
    {
        if (request is null) throw new ArgumentException("request is null please check");

        var place = _db.Places.FirstOrDefault(i => i.Id == id);
        if (place == null) return false;
        if (!_db.Countries.Any(c => c.Id == request.CountryId)) throw new ArgumentException("no country found on this CountryId");

        place.Name = request.Name;
        place.LocationUrl = request.LocationUrl;
        place.WorkStartTime = request.WorkStartTime;
        place.WorkEndTime = request.WorkEndTime;
        place.CountryId = request.CountryId;
        _db.SaveChanges();

        return true;
    }

    [HttpDelete("DeletePlaceById")]
    public bool DeletePlaceById(int id)
    {
        // Tags-იც ვტვირთავ, რომ EF-მა ადგილთან ერთად მისი მონიშვნებიც წაშალოს
        var placeExist = _db.Places.Include(p => p.Tags).FirstOrDefault(u => u.Id == id);
        if (placeExist == null) return false;
        _db.Places.Remove(placeExist);
        _db.SaveChanges();
        return true;
    }
}
