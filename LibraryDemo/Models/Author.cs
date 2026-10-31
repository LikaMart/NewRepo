using System;
using System.Collections.Generic;

namespace LibraryDemo.Models;

public partial class Author
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;

    public string? Country { get; set; }

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
