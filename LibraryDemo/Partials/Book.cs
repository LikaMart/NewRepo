namespace LibraryDemo.Models;

public partial class Book
{
    public string Info => $"{Title} ({Year})";
}
