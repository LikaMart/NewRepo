using System;
using System.Collections.Generic;

namespace CinemaDemo.Models;

public partial class Movie
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int DurationMinutes { get; set; }

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
