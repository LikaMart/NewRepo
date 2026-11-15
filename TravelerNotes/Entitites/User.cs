using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelerNotes.Entitites;

[Table("Users")]
public class User
{
    [Key]
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Surname { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Password { get; set; }

    public string? BirthDay { get; set; }

    public string? CodeWord { get; set; }

    public List<Trip>? Trips { get; set; }
}
