using System;
using System.Collections.Generic;

namespace CinemaDemo.OnlyMovies;

public partial class Movie
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int DurationMinutes { get; set; }
}
