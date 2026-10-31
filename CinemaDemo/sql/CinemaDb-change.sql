USE CinemaDb;
GO
CREATE TABLE Halls (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL UNIQUE,
    Seats INT NOT NULL
);
INSERT INTO Halls (Name, Seats)
VALUES (N'დიდი დარბაზი', 200),
       (N'VIP დარბაზი', 40);
GO
ALTER TABLE Sessions ADD HallId INT NULL;
GO
UPDATE Sessions SET HallId = 1 WHERE Id IN (1, 3, 5);
UPDATE Sessions SET HallId = 2 WHERE Id IN (2, 4);
ALTER TABLE Sessions ALTER COLUMN HallId INT NOT NULL;
ALTER TABLE Sessions ADD FOREIGN KEY (HallId) REFERENCES Halls(Id);
