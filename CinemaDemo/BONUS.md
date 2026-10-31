# ⭐ ბონუსები

## 1. `-Tables Movies -OutputDir OnlyMovies` — რა დაგენერირდა?

საქაღალდე `OnlyMovies/` მხოლოდ ორი ფაილით:

- `Movie.cs` — მხოლოდ `Id`, `Title`, `DurationMinutes`. **`Sessions` ნავიგაცია აღარ აქვს**, რადგან Sessions ცხრილი არ დავასკაფოლდეთ.
- `CinemaContext.cs` — მხოლოდ ერთი `DbSet<Movie> Movies`. `Sessions` და `Halls` აქ არ არის.

ანუ `-Tables` ირჩევს, რომელი ცხრილებისთვის შეიქმნას კლასები; დანარჩენი ცხრილები და მათთან კავშირები უბრალოდ გამოტოვებულია.
(CLI-ში იგივე ოფციაა `--table Movies`.)

## 2. `-DataAnnotations` — Session.cs-ში რა ატრიბუტები გაჩნდა?

შედეგი: `AnnotatedModels/Session.cs`

```csharp
[Key]
public int Id { get; set; }

[Column(TypeName = "decimal(6, 2)")]
public decimal Price { get; set; }

[ForeignKey("HallId")]
[InverseProperty("Sessions")]
public virtual Hall Hall { get; set; } = null!;

[ForeignKey("MovieId")]
[InverseProperty("Sessions")]
public virtual Movie Movie { get; set; } = null!;
```

(Hall.cs-ში კი: `[Index("Name", IsUnique = true)]`, `[StringLength(50)]`.)

**რა დარჩა `OnModelCreating`-ში** — მხოლოდ ის, რაც ატრიბუტით ვერ ჩაიწერება:

- Primary key-ის **სახელი**: `HasKey(...).HasName("PK__Sessions__...")`
- კავშირები `HasOne(...).WithMany(...)` **`OnDelete(DeleteBehavior.ClientSetNull)`**-ით
- Foreign key-ის **სახელი**: `HasConstraintName("FK__Sessions__...")`

ანუ ტიპები, სიგრძეები, გასაღებები და FK სვეტები გადავიდა კლასებში ატრიბუტებად, ხოლო წაშლის ქცევა და constraint-ების სახელები — დარჩა Fluent API-ში.

## 3. დარბაზი უკვე არსებული სახელით — რას იტყვის ბაზა?

`Halls.Name` სვეტზე არის `UNIQUE`. Program.cs ცდილობს დაამატოს `"VIP დარბაზი"` (უკვე არსებობს) და `SaveChanges()` აგდებს `DbUpdateException`-ს:

```
Violation of UNIQUE KEY constraint 'UQ__Halls__737584F60C1311D1'.
Cannot insert duplicate key in object 'dbo.Halls'.
The duplicate key value is (VIP დარბაზი).
```

EF Core თავისით არ ამოწმებს უნიკალურობას — შეცდომას აბრუნებს თვითონ SQL Server, EF კი მას `DbUpdateException`-ში ახვევს (`InnerException` = `SqlException`).
