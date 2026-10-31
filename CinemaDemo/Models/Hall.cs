using System;
using System.Collections.Generic;

namespace CinemaDemo.Models;

public partial class Hall
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Seats { get; set; }

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
