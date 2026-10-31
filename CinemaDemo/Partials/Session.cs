namespace CinemaDemo.Models;

public partial class Session
{
    public string Time => StartsAt.ToString("HH:mm");
}
