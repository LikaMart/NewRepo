using System;
using System.Collections.Generic;

namespace CinemaDemo.Models;

public partial class Session
{
    public int Id { get; set; }

    public int MovieId { get; set; }

    public DateTime StartsAt { get; set; }

    public decimal Price { get; set; }

    public int HallId { get; set; }

    public virtual Hall Hall { get; set; } = null!;

    public virtual Movie Movie { get; set; } = null!;
}
