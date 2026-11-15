using Microsoft.AspNetCore.Mvc;
using TravelerNotes.Db;
using TravelerNotes.Models.Tag;

namespace TravelerNotes.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TagController : ControllerBase
{
    private readonly TravelerNotesDb _db;

    public TagController()
    {
        _db = new TravelerNotesDb();
    }

    [HttpGet("GetAllTags")]
    public List<TagResponse> GetAllTags()
    {
        return _db.Tags.Select(i => new TagResponse
        {
            Id = i.Id,
            PlaceId = i.PlaceId,
            TripId = i.TripId,
            TagTime = i.TagTime,
            Comment = i.Comment
        }).ToList();
    }

    [HttpGet("GetTagById")]
    public TagResponse GetTagById(int id)
    {
        var tag = _db.Tags.FirstOrDefault(i => i.Id == id);
        if (tag == null) throw new ArgumentException("no tag found on this Id");
        return new TagResponse
        {
            Id = tag.Id,
            PlaceId = tag.PlaceId,
            TripId = tag.TripId,
            TagTime = tag.TagTime,
            Comment = tag.Comment
        };
    }

    // ერთი მოგზაურობის ყველა მონიშვნა, დროის მიხედვით
    [HttpGet("GetTagsByTripId")]
    public List<TagResponse> GetTagsByTripId(int tripId)
    {
        return _db.Tags
            .Where(i => i.TripId == tripId)
            .OrderBy(i => i.TagTime)
            .Select(i => new TagResponse
            {
                Id = i.Id,
                PlaceId = i.PlaceId,
                TripId = i.TripId,
                TagTime = i.TagTime,
                Comment = i.Comment
            }).ToList();
    }

    [HttpPost("AddNewTag")]
    public bool AddNewTag(TagRequest request)
    {
        if (request is null) throw new ArgumentException("request is null please check");
        CheckPlaceAndTrip(request);

        _db.Tags.Add(new Entitites.Tag
        {
            PlaceId = request.PlaceId,
            TripId = request.TripId,
            TagTime = request.TagTime,
            Comment = request.Comment
        });
        _db.SaveChanges();

        return true;
    }

    [HttpPut("UpdateTagById")]
    public bool UpdateTagById(int id, TagRequest request)
    {
        if (request is null) throw new ArgumentException("request is null please check");

        var tag = _db.Tags.FirstOrDefault(i => i.Id == id);
        if (tag == null) return false;
        CheckPlaceAndTrip(request);

        tag.PlaceId = request.PlaceId;
        tag.TripId = request.TripId;
        tag.TagTime = request.TagTime;
        tag.Comment = request.Comment;
        _db.SaveChanges();

        return true;
    }

    [HttpDelete("DeleteTagById")]
    public bool DeleteTagById(int id)
    {
        var tagExist = _db.Tags.FirstOrDefault(u => u.Id == id);
        if (tagExist == null) return false;
        _db.Tags.Remove(tagExist);
        _db.SaveChanges();
        return true;
    }

    // ადგილი და მოგზაურობა უნდა არსებობდეს და ერთსა და იმავე ქვეყანაში უნდა იყოს
    private void CheckPlaceAndTrip(TagRequest request)
    {
        var place = _db.Places.FirstOrDefault(p => p.Id == request.PlaceId);
        if (place == null) throw new ArgumentException("no place found on this PlaceId");

        var trip = _db.Trips.FirstOrDefault(t => t.Id == request.TripId);
        if (trip == null) throw new ArgumentException("no trip found on this TripId");

        if (place.CountryId != trip.CountryId) throw new ArgumentException("place and trip are in different countries");
    }
}
