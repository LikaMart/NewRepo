using Microsoft.AspNetCore.Mvc;
using TravelerNotes.Db;
using TravelerNotes.Models.Country;
using TravelerNotes.Models.Trip;

namespace TravelerNotes.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripController : ControllerBase
{
    private readonly TravelerNotesDb _db;

    public TripController()
    {
        _db = new TravelerNotesDb();
    }

    [HttpGet("GetAllTrips")]
    public List<TripResponse> GetAllTrips()
    {
        return _db.Trips.Select(i => new TripResponse
        {
          EndtDate = i.EndtDate,
          StartDate = i.StartDate,
          CountryId = i.CountryId,
          Notes = i.Notes,
          Rating = i.Rating,
          UserId = i.UserId,
          Id = i.Id
        }).ToList();
    }

    [HttpGet("GetTripById")]
    public TripResponse GetTripById(int id)
    {
        var trip = _db.Trips.FirstOrDefault(i => i.Id == id);
        if (trip == null) throw new ArgumentException("no user found on this Id");
        return new TripResponse
        {
           EndtDate = trip.EndtDate,
           StartDate = trip.StartDate,
           CountryId = trip.CountryId,
           Notes = trip.Notes,
           Id = trip.Id,
           Rating = trip.Rating,
           UserId = trip.UserId
        };
    }

    [HttpPost("AddNewTrip")]
    public bool AddNewTrip(TripRequest request)
    {
        if (request is null) throw new ArgumentException("request is null please check");

        _db.Trips.Add(new Entitites.Trip
        {
           Rating = request.Rating,
           EndtDate = request.EndtDate,
           StartDate = request.StartDate,
           CountryId =request.CountryId,
           Notes =request.Notes,
           UserId =request.UserId
        });
        _db.SaveChanges();

        return true;
    }

    [HttpDelete("DeleteTripById")]
    public bool DeleteTripById(int id)
    {
        var tripExist = _db.Trips.FirstOrDefault(u => u.Id == id);
        if (tripExist == null) return false;
        _db.Trips.Remove(tripExist);
        _db.SaveChanges();
        return true;
    }
}
