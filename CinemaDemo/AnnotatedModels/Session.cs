using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CinemaDemo.AnnotatedModels;

public partial class Session
{
    [Key]
    public int Id { get; set; }

    public int MovieId { get; set; }

    public DateTime StartsAt { get; set; }

    [Column(TypeName = "decimal(6, 2)")]
    public decimal Price { get; set; }

    public int HallId { get; set; }

    [ForeignKey("HallId")]
    [InverseProperty("Sessions")]
    public virtual Hall Hall { get; set; } = null!;

    [ForeignKey("MovieId")]
    [InverseProperty("Sessions")]
    public virtual Movie Movie { get; set; } = null!;
}
