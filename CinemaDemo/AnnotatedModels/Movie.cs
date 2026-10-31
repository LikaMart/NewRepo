using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CinemaDemo.AnnotatedModels;

public partial class Movie
{
    [Key]
    public int Id { get; set; }

    [StringLength(200)]
    public string Title { get; set; } = null!;

    public int DurationMinutes { get; set; }

    [InverseProperty("Movie")]
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
