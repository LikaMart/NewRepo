using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CinemaDemo.AnnotatedModels;

[Index("Name", Name = "UQ__Halls__737584F60C1311D1", IsUnique = true)]
public partial class Hall
{
    [Key]
    public int Id { get; set; }

    [StringLength(50)]
    public string Name { get; set; } = null!;

    public int Seats { get; set; }

    [InverseProperty("Hall")]
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
