CREATE DATABASE CinemaDb;
GO
USE CinemaDb;
GO
CREATE TABLE Movies (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    DurationMinutes INT NOT NULL
);
CREATE TABLE Sessions (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MovieId INT NOT NULL FOREIGN KEY REFERENCES Movies(Id),
    StartsAt DATETIME2 NOT NULL,
    Price DECIMAL(6,2) NOT NULL
);
INSERT INTO Movies (Title, DurationMinutes)
VALUES (N'ოპენჰაიმერი', 180),
       (N'ინტერსტელარი', 169),
       (N'დიუნა: მეორე ნაწილი', 166);
INSERT INTO Sessions (MovieId, StartsAt, Price)
VALUES (1, '2026-11-01 19:30', 15.00),
       (2, '2026-11-01 14:00', 12.00),
       (3, '2026-11-01 22:15', 15.00),
       (1, '2026-11-01 11:00', 10.00),
       (3, '2026-11-01 17:45', 14.00);
