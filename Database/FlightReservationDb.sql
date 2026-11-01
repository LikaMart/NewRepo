-- საბოლოო პროექტი: ფრენების დაჯავშნის სისტემა (SQL ნაწილი)
-- ბაზა: FlightReservationDb, SQL Server
-- სკრიპტი ეშვება ნულიდან ერთბაშად (F5). თუ ბაზა უკვე არსებობს, ჯერ იშლება და თავიდან იქმნება.
--
-- GO არ არის SQL ბრძანება, ის SSMS-ს ეუბნება სად მთავრდება ერთი "ბლოკი" (batch).
-- სულ 4 GO მაქვს და ოთხივე აუცილებელია:
--   1) CREATE DATABASE-ის შემდეგ, რომ USE-მა ახალი ბაზა უკვე დაინახოს
--   2) CREATE VIEW-ს წინ, რადგან VIEW ბლოკში პირველი ბრძანება უნდა იყოს
--   3) CREATE PROCEDURE-ს წინ, იგივე მიზეზით
--   4) პროცედურის შემდეგ, თორემ ქვემოთ დაწერილი ტესტებიც პროცედურის ნაწილი გახდება

-- ფილტრიან ინდექსს (UX_Reservations_Flight_Seat) ეს ორი პარამეტრი ჭირდება.
-- SSMS-ში ისედაც ჩართულია, მაგრამ sqlcmd-ში არა, ამიტომ ცალკე ვრთავ.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

USE master;

-- თუ ბაზა უკვე არსებობს, ვშლი. SINGLE_USER სხვა კავშირებს თიშავს,
-- თორემ DROP DATABASE შეცდომას ამოაგდებს, თუ ბაზა სადმე ღიაა.
IF DB_ID(N'FlightReservationDb') IS NOT NULL
BEGIN
    ALTER DATABASE FlightReservationDb SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE FlightReservationDb;
END

CREATE DATABASE FlightReservationDb;
GO

USE FlightReservationDb;


-- 1. ბაზის დიზაინი

-- მგზავრების ცხრილი
-- Id ავტომატურად იზრდება (IDENTITY), Email ორ მგზავრს ერთი ვერ ექნება (UNIQUE).
-- CHECK-ით ვამოწმებ, რომ Email-ში @ და წერტილი იყოს და დაბადების თარიღი მომავალში არ იყოს.
CREATE TABLE Passengers (
    Id          INT IDENTITY(1,1) NOT NULL,
    FirstName   NVARCHAR(50)  NOT NULL,
    LastName    NVARCHAR(50)  NOT NULL,
    Email       NVARCHAR(100) NOT NULL,
    BirthDate   DATE          NOT NULL,
    CONSTRAINT PK_Passengers PRIMARY KEY (Id),
    CONSTRAINT UQ_Passengers_Email UNIQUE (Email),
    CONSTRAINT CK_Passengers_Email CHECK (Email LIKE N'%_@_%._%'),
    CONSTRAINT CK_Passengers_BirthDate CHECK (BirthDate > '1900-01-01' AND BirthDate < GETDATE())
);

-- აეროპორტების ცხრილი
-- Code არის IATA კოდი (მაგ. TBS), ზუსტად 3 დიდი ლათინური ასო და უნიკალური.
CREATE TABLE Airports (
    Id       INT IDENTITY(1,1) NOT NULL,
    Code     CHAR(3)       NOT NULL,
    Name     NVARCHAR(100) NOT NULL,
    City     NVARCHAR(50)  NOT NULL,
    Country  NVARCHAR(50)  NOT NULL,
    CONSTRAINT PK_Airports PRIMARY KEY (Id),
    CONSTRAINT UQ_Airports_Code UNIQUE (Code),
    CONSTRAINT CK_Airports_Code CHECK (Code LIKE '[A-Z][A-Z][A-Z]')
);

-- რეისების ცხრილი
-- აქ ორი FK მიდის ერთსა და იმავე Airports ცხრილზე: ერთი გამოსვლის აეროპორტია, მეორე ჩასვლის.
-- TotalSeats-ს DEFAULT აქვს 180 (თუ INSERT-ში არ მივუთითებ, 180 ჩაიწერება).
CREATE TABLE Flights (
    Id                  INT IDENTITY(1,1) NOT NULL,
    FlightNumber        NVARCHAR(10)  NOT NULL,
    DepartureAirportId  INT           NOT NULL,
    ArrivalAirportId    INT           NOT NULL,
    DepartureTime       DATETIME2(0)  NOT NULL,
    ArrivalTime         DATETIME2(0)  NOT NULL,
    Price               DECIMAL(10,2) NOT NULL,
    TotalSeats          INT           NOT NULL CONSTRAINT DF_Flights_TotalSeats DEFAULT (180),
    CONSTRAINT PK_Flights PRIMARY KEY (Id),
    CONSTRAINT UQ_Flights_FlightNumber UNIQUE (FlightNumber),
    CONSTRAINT FK_Flights_DepartureAirport FOREIGN KEY (DepartureAirportId) REFERENCES Airports(Id),
    CONSTRAINT FK_Flights_ArrivalAirport   FOREIGN KEY (ArrivalAirportId)   REFERENCES Airports(Id),
    -- ფასი და ადგილების რაოდენობა დადებითი უნდა იყოს
    CONSTRAINT CK_Flights_Price      CHECK (Price > 0),
    CONSTRAINT CK_Flights_TotalSeats CHECK (TotalSeats > 0),
    -- ჩასვლის დრო გამოსვლის დროზე გვიან უნდა იყოს
    CONSTRAINT CK_Flights_ArrivalAfterDeparture CHECK (ArrivalTime > DepartureTime),
    -- თვითმფრინავი იმავე აეროპორტში ვერ "ჩაფრინდება" საიდანაც გაფრინდა
    CONSTRAINT CK_Flights_DifferentAirports CHECK (DepartureAirportId <> ArrivalAirportId)
);

-- ჯავშნების ცხრილი
-- ერთ მგზავრს ბევრი ჯავშანი შეიძლება ჰქონდეს, ერთ რეისზეც ბევრი ჯავშანია (ორივე FK).
-- Status-ის DEFAULT არის 'Confirmed', CreatedAt-ის DEFAULT კი მიმდინარე დრო.
-- Status მხოლოდ სამიდან ერთ-ერთი შეიძლება იყოს (CHECK ... IN).
-- SeatNumber-ის ფორმატი: ციფრი(ები) და ბოლოში ასო, მაგ. 1A, 12C.
CREATE TABLE Reservations (
    Id           INT IDENTITY(1,1) NOT NULL,
    PassengerId  INT          NOT NULL,
    FlightId     INT          NOT NULL,
    SeatNumber   NVARCHAR(5)  NOT NULL,
    Status       NVARCHAR(20) NOT NULL CONSTRAINT DF_Reservations_Status DEFAULT (N'Confirmed'),
    CreatedAt    DATETIME2(0) NOT NULL CONSTRAINT DF_Reservations_CreatedAt DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_Reservations PRIMARY KEY (Id),
    CONSTRAINT FK_Reservations_Passengers FOREIGN KEY (PassengerId) REFERENCES Passengers(Id),
    CONSTRAINT FK_Reservations_Flights    FOREIGN KEY (FlightId)    REFERENCES Flights(Id),
    CONSTRAINT CK_Reservations_Status CHECK (Status IN (N'Pending', N'Confirmed', N'Cancelled')),
    CONSTRAINT CK_Reservations_SeatNumber CHECK (SeatNumber LIKE N'[1-9]%[A-K]')
);

-- ერთ რეისზე ერთი და იგივე ადგილი ორჯერ რომ ვერ დაიჯავშნოს.
-- ჩვეულებრივი UNIQUE (FlightId, SeatNumber) არ გამოვიყენე, რადგან მაშინ გაუქმებული ჯავშანიც
-- სამუდამოდ დაიკავებდა ადგილს. ამიტომ ავიღე UNIQUE ინდექსი WHERE პირობით (filtered index):
-- უნიკალურობა მოწმდება მხოლოდ იმ ჯავშნებში, რომლებიც გაუქმებული არ არის.
CREATE UNIQUE NONCLUSTERED INDEX UX_Reservations_Flight_Seat
    ON Reservations (FlightId, SeatNumber)
    WHERE Status <> N'Cancelled';

-- დამატებითი ინდექსები იმ სვეტებზე, რომლებითაც ხშირად ვაკეთებ JOIN-ს
CREATE NONCLUSTERED INDEX IX_Reservations_PassengerId ON Reservations (PassengerId);
CREATE NONCLUSTERED INDEX IX_Flights_Route ON Flights (DepartureAirportId, ArrivalAirportId);


-- 2. მონაცემების შევსება

-- 7 აეროპორტი: 3 საქართველოში (შიდა რეისებისთვის) და 4 საზღვარგარეთ (საერთაშორისოსთვის)
INSERT INTO Airports (Code, Name, City, Country) VALUES
    ('TBS', N'Tbilisi International Airport',                  N'Tbilisi',   N'Georgia'),
    ('KUT', N'David the Builder Kutaisi International Airport', N'Kutaisi',   N'Georgia'),
    ('BUS', N'Batumi International Airport',                   N'Batumi',    N'Georgia'),
    ('IST', N'Istanbul Airport',                               N'Istanbul',  N'Turkey'),
    ('FRA', N'Frankfurt Airport',                              N'Frankfurt', N'Germany'),
    ('CDG', N'Paris Charles de Gaulle Airport',                N'Paris',     N'France'),
    ('DXB', N'Dubai International Airport',                    N'Dubai',     N'United Arab Emirates');

-- 11 რეისი. ბოლოში მიწერილია რომელი Id გამოუვა და საიდან სად მიდის.
-- 9 და 10 ნომერ რეისებს განზრახ ცოტა ადგილი აქვთ, რომ ამოცანა 11 (80%-ზე მეტი) შედეგს აბრუნებდეს.
-- 11 ნომერი უკვე შემდგარი (წარსული) რეისია, ის VIEW-ში არ უნდა გამოჩნდეს.
INSERT INTO Flights (FlightNumber, DepartureAirportId, ArrivalAirportId, DepartureTime, ArrivalTime, Price, TotalSeats) VALUES
    (N'GE101',  1, 3, '2026-12-05T08:00:00', '2026-12-05T09:00:00', 120.00,  50),  -- 1  TBS -> BUS
    (N'TK381',  1, 4, '2026-12-07T06:30:00', '2026-12-07T08:45:00', 260.00, 180),  -- 2  TBS -> IST
    (N'TK383',  1, 4, '2026-12-20T18:00:00', '2026-12-20T20:15:00', 310.00, 180),  -- 3  TBS -> IST
    (N'LH2557', 1, 5, '2026-12-10T04:30:00', '2026-12-10T07:40:00', 450.00, 150),  -- 4  TBS -> FRA
    (N'W64301', 2, 6, '2027-01-18T13:00:00', '2027-01-18T16:30:00', 180.00, 230),  -- 5  KUT -> CDG
    (N'FZ712',  1, 7, '2027-01-22T02:00:00', '2027-01-22T05:30:00', 380.00, 190),  -- 6  TBS -> DXB
    (N'FZ711',  7, 1, '2027-01-29T22:00:00', '2027-01-30T01:30:00', 360.00, 190),  -- 7  DXB -> TBS
    (N'TK380',  4, 1, '2027-02-03T01:00:00', '2027-02-03T04:30:00', 240.00, 180),  -- 8  IST -> TBS
    (N'GE205',  2, 3, '2027-01-10T10:00:00', '2027-01-10T10:40:00',  95.00,   5),  -- 9  KUT -> BUS, 5 ადგილი
    (N'GE207',  3, 1, '2027-01-15T19:00:00', '2027-01-15T20:00:00', 130.00,   6),  -- 10 BUS -> TBS, 6 ადგილი
    (N'GE099',  1, 3, '2026-10-15T08:00:00', '2026-10-15T09:00:00', 110.00,  50);  -- 11 TBS -> BUS, წარსული

-- 12 მგზავრი. 11 და 12 ნომრებს ჯავშანს არ ვაძლევ, ამოცანა 8-ისთვის.
-- სამ მგზავრს გვარი B-ზე ეწყება, ამოცანა 2-ისთვის.
INSERT INTO Passengers (FirstName, LastName, Email, BirthDate) VALUES
    (N'Giorgi', N'Beridze',     N'giorgi.beridze@mail.ge',     '1990-04-12'),  -- 1
    (N'Nino',   N'Kapanadze',   N'nino.kapanadze@mail.ge',     '1995-11-03'),  -- 2
    (N'Luka',   N'Bakradze',    N'luka.bakradze@mail.ge',      '2001-07-25'),  -- 3
    (N'Mariam', N'Lomidze',     N'mariam.lomidze@mail.ge',     '1988-02-14'),  -- 4
    (N'Davit',  N'Gelashvili',  N'davit.gelashvili@mail.ge',   '1979-09-30'),  -- 5
    (N'Ana',    N'Bolkvadze',   N'ana.bolkvadze@mail.ge',      '1999-12-19'),  -- 6
    (N'Saba',   N'Tsiklauri',   N'saba.tsiklauri@mail.ge',     '2004-05-08'),  -- 7
    (N'Tamar',  N'Kiknadze',    N'tamar.kiknadze@mail.ge',     '1985-06-21'),  -- 8
    (N'Irakli', N'Maisuradze',  N'irakli.maisuradze@mail.ge',  '1993-03-02'),  -- 9
    (N'Elene',  N'Japaridze',   N'elene.japaridze@mail.ge',    '2000-10-11'),  -- 10
    (N'Nika',   N'Abashidze',   N'nika.abashidze@mail.ge',     '1997-01-27'),  -- 11
    (N'Salome', N'Chkheidze',   N'salome.chkheidze@mail.ge',   '2002-08-16');  -- 12

-- 17 ჯავშანი, ორ ნაწილად.
-- პირველ ნაწილში Status და CreatedAt არ მიმითითებია, ამიტომ DEFAULT-ით ივსება ('Confirmed' და ახლანდელი დრო).
INSERT INTO Reservations (PassengerId, FlightId, SeatNumber) VALUES
    -- რეისი 9 (5 ადგილი): 5-ივე დაკავებულია, ანუ 100%
    (1, 9, N'1A'), (2, 9, N'1B'), (3, 9, N'2A'), (4, 9, N'2B'), (5, 9, N'3A'),
    -- რეისი 10 (6 ადგილი): 5 დაკავებულია, ანუ 83%
    (6, 10, N'1A'), (7, 10, N'1B'), (8, 10, N'2A'), (9, 10, N'2B'), (10, 10, N'3A'),
    -- სხვა რეისები
    (1, 2, N'12C'), (3, 2, N'14A'), (2, 4, N'3C');

-- მეორე ნაწილში სტატუსს და თარიღს ხელით ვწერ, რომ Pending და Cancelled სტატუსებიც მქონდეს
INSERT INTO Reservations (PassengerId, FlightId, SeatNumber, Status, CreatedAt) VALUES
    (6, 6, N'7F',  N'Pending',   '2026-10-28T14:20:00'),
    (4, 11, N'5A', N'Confirmed', '2026-09-30T10:00:00'),
    (7, 11, N'5B', N'Confirmed', '2026-10-01T12:30:00'),
    (8, 3, N'20A', N'Cancelled', '2026-10-20T09:15:00');


-- 3. SELECT და ფილტრაცია

-- ამოცანა 1
-- რეისები, რომელთა ფასი 200-ზე მეტია. DESC ნიშნავს კლებადობით (ძვირიდან იაფისკენ).
SELECT Id, FlightNumber, DepartureTime, ArrivalTime, Price
FROM Flights
WHERE Price > 200
ORDER BY Price DESC;

-- ამოცანა 2
-- მგზავრები, რომელთა გვარი B-ზე იწყება. % ნიშნავს "ნებისმიერი სიმბოლოები შემდეგ".
SELECT Id, FirstName, LastName, Email
FROM Passengers
WHERE LastName LIKE N'B%'
ORDER BY LastName;

-- ამოცანა 3
-- 3 უახლოესი რეისი. ჯერ ვფილტრავ მხოლოდ მომავალ რეისებს (წარსული "უახლოესი" აღარაა),
-- მერე ვალაგებ დროის ზრდით და TOP (3)-ით ვიღებ პირველ სამს.
SELECT TOP (3) Id, FlightNumber, DepartureTime, Price
FROM Flights
WHERE DepartureTime > SYSDATETIME()
ORDER BY DepartureTime ASC;

-- ამოცანა 4
-- რეისები ორ თარიღს შორის, აქ 2026 წლის დეკემბერი.
-- ბოლო თარიღში 23:59:59 იმიტომ წერია, რომ 31 დეკემბრის რეისებიც მოხვდეს
-- (მარტო '2026-12-31' ნიშნავს 31 დეკემბრის 00:00-ს).
SELECT Id, FlightNumber, DepartureTime, Price
FROM Flights
WHERE DepartureTime BETWEEN '2026-12-01T00:00:00' AND '2026-12-31T23:59:59'
ORDER BY DepartureTime;

-- ამოცანა 5
-- მგზავრები ასაკით.
-- DATEDIFF(YEAR, ...) მხოლოდ წლებს აკლებს ერთმანეთს და დაბადების დღეს არ ითვალისწინებს.
-- მაგ. 1990-12-30-ზე დაბადებულს 2026-01-01-ზე 36-ს დაუთვლის, სინამდვილეში კი 35-ისაა.
-- ამიტომ ვამოწმებ: თუ წელს დაბადების დღე ჯერ არ დამდგარა, 1-ს ვაკლებ.
SELECT Id,
       FirstName,
       LastName,
       BirthDate,
       DATEDIFF(YEAR, BirthDate, GETDATE())
         - CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, BirthDate, GETDATE()), BirthDate) > CAST(GETDATE() AS DATE)
                THEN 1 ELSE 0 END AS Age
FROM Passengers
ORDER BY Age DESC;


-- 4. JOIN

-- ამოცანა 6
-- რეისები აეროპორტების სახელებით. Airports-ს ორჯერ ვაჯოინებ სხვადასხვა ფსევდონიმით:
-- dep არის გამოსვლის აეროპორტი, arr ჩასვლის. ერთი JOIN-ით ორივეს ვერ მივიღებდი.
SELECT f.FlightNumber,
       dep.Code + ' (' + dep.City + N') -> ' + arr.Code + ' (' + arr.City + ')' AS Route,
       dep.Name AS DepartureAirport,
       arr.Name AS ArrivalAirport,
       f.DepartureTime,
       f.ArrivalTime,
       f.Price
FROM Flights f
JOIN Airports dep ON dep.Id = f.DepartureAirportId
JOIN Airports arr ON arr.Id = f.ArrivalAirportId
ORDER BY f.DepartureTime;

-- ამოცანა 7
-- ჯავშნები: მგზავრის სახელს ვიღებ Passengers-დან, რეისის ნომერს Flights-დან.
SELECT r.Id AS ReservationId,
       p.FirstName + N' ' + p.LastName AS PassengerName,
       f.FlightNumber,
       r.SeatNumber,
       r.Status,
       r.CreatedAt
FROM Reservations r
JOIN Passengers p ON p.Id = r.PassengerId
JOIN Flights f    ON f.Id = r.FlightId
ORDER BY f.FlightNumber, r.SeatNumber;

-- ამოცანა 8
-- მგზავრები, რომლებსაც ჯავშანი არ აქვთ.
-- LEFT JOIN ყველა მგზავრს ტოვებს. ვისაც ჯავშანი არ აქვს, იმის r სვეტები NULL იქნება,
-- ამიტომ WHERE r.Id IS NULL სწორედ მათ გამოიტანს.
SELECT p.Id, p.FirstName, p.LastName, p.Email
FROM Passengers p
LEFT JOIN Reservations r ON r.PassengerId = p.Id
WHERE r.Id IS NULL;

-- ამოცანა 9
-- რეისები, რომლებზეც არცერთი ადგილი არ არის დაჯავშნილი. იგივე ლოგიკაა, რაც ამოცანა 8-ში.
-- გაუქმებული ჯავშანი ადგილს არ იკავებს, ამიტომ Status-ის პირობა ON-ში წერია და არა WHERE-ში.
-- WHERE-ში რომ დამეწერა, LEFT JOIN ჩვეულებრივ JOIN-ად იქცეოდა.
SELECT f.Id, f.FlightNumber, f.DepartureTime, f.TotalSeats
FROM Flights f
LEFT JOIN Reservations r
       ON r.FlightId = f.Id
      AND r.Status <> N'Cancelled'
WHERE r.Id IS NULL
ORDER BY f.DepartureTime;


-- 5. GROUP BY და აგრეგაცია

-- ამოცანა 10
-- ჯავშნების რაოდენობა თითო რეისზე.
-- LEFT JOIN იმიტომ, რომ 0 ჯავშნიანი რეისებიც გამოჩნდეს. COUNT(r.Id) NULL-ებს არ ითვლის, ამიტომ მათთან 0 ჩანს.
-- ცალკე ვითვლი აქტიურ (გაუუქმებელ) ჯავშნებსაც.
SELECT f.Id,
       f.FlightNumber,
       COUNT(r.Id) AS TotalReservations,
       SUM(CASE WHEN r.Status <> N'Cancelled' THEN 1 ELSE 0 END) AS ActiveReservations
FROM Flights f
LEFT JOIN Reservations r ON r.FlightId = f.Id
GROUP BY f.Id, f.FlightNumber
ORDER BY TotalReservations DESC, f.FlightNumber;

-- ამოცანა 11
-- რეისები, სადაც ადგილების 80%-ზე მეტია დაკავებული.
-- HAVING გამოიყენება, რადგან ფილტრი აგრეგატზეა (COUNT), WHERE ამას ვერ შეძლებს.
-- 1.0-ზე ვამრავლებ, რომ გაყოფა ათწილადი იყოს. 5/6 მთელ რიცხვებში 0 გამოვა.
SELECT f.Id,
       f.FlightNumber,
       f.TotalSeats,
       COUNT(r.Id) AS BookedSeats,
       CAST(COUNT(r.Id) * 100.0 / f.TotalSeats AS DECIMAL(5,2)) AS OccupancyPercent
FROM Flights f
JOIN Reservations r
  ON r.FlightId = f.Id
 AND r.Status <> N'Cancelled'
GROUP BY f.Id, f.FlightNumber, f.TotalSeats
HAVING COUNT(r.Id) * 1.0 / f.TotalSeats > 0.8
ORDER BY OccupancyPercent DESC;

-- ამოცანა 12
-- საშუალო ფასი თითო მიმართულებაზე (საიდან -> სად).
-- ვაჯგუფებ გამოსვლისა და ჩასვლის ქალაქით. TBS -> IST და TBS -> BUS ორ-ორი რეისია, ამიტომ იქ ნამდვილი საშუალო ჩანს.
SELECT dep.City + N' -> ' + arr.City AS Direction,
       COUNT(*) AS FlightsCount,
       CAST(AVG(f.Price) AS DECIMAL(10,2)) AS AveragePrice,
       MIN(f.Price) AS MinPrice,
       MAX(f.Price) AS MaxPrice
FROM Flights f
JOIN Airports dep ON dep.Id = f.DepartureAirportId
JOIN Airports arr ON arr.Id = f.ArrivalAirportId
GROUP BY dep.City, arr.City
ORDER BY AveragePrice DESC;


-- 6. UPDATE და DELETE

-- ამოცანა 13
-- კონკრეტული მგზავრის ყველა ჯავშნის გაუქმება. მგზავრს Email-ით ვპოულობ, რადგან ის უნიკალურია.
-- UPDATE-ში JOIN იმიტომ მჭირდება, რომ Email Passengers ცხრილშია და არა Reservations-ში.
UPDATE r
SET r.Status = N'Cancelled'
FROM Reservations r
JOIN Passengers p ON p.Id = r.PassengerId
WHERE p.Email = N'giorgi.beridze@mail.ge'
  AND r.Status <> N'Cancelled';

-- შემოწმება: ამ მგზავრის ჯავშნები ახლა Cancelled უნდა იყოს
SELECT r.Id, r.FlightId, r.SeatNumber, r.Status
FROM Reservations r
JOIN Passengers p ON p.Id = r.PassengerId
WHERE p.Email = N'giorgi.beridze@mail.ge';

-- ამოცანა 14
-- საერთაშორისო რეისების ფასის გაზრდა 10%-ით.
-- საერთაშორისოდ ვთვლი რეისს, რომლის გამოსვლის და ჩასვლის ქვეყანა განსხვავდება.
-- ქვეყანა Airports-შია, ამიტომ აქაც ორჯერ ვაჯოინებ. ROUND 2 ციფრამდე ამრგვალებს.
UPDATE f
SET f.Price = ROUND(f.Price * 1.10, 2)
FROM Flights f
JOIN Airports dep ON dep.Id = f.DepartureAirportId
JOIN Airports arr ON arr.Id = f.ArrivalAirportId
WHERE dep.Country <> arr.Country;

-- შემოწმება: შიდა რეისების (Georgia -> Georgia) ფასი არ უნდა შეცვლილიყო
SELECT f.FlightNumber, dep.Country AS FromCountry, arr.Country AS ToCountry, f.Price
FROM Flights f
JOIN Airports dep ON dep.Id = f.DepartureAirportId
JOIN Airports arr ON arr.Id = f.ArrivalAirportId
ORDER BY f.Id;

-- ამოცანა 15
-- გაუქმებული ჯავშნების წაშლა. ისინი ადგილს აღარ იკავებენ და ბაზაში აღარ მჭირდება.
DELETE FROM Reservations
WHERE Status = N'Cancelled';

-- შემოწმება: რამდენი ჯავშანი დარჩა
SELECT COUNT(*) AS RemainingReservations FROM Reservations;
GO


-- 7. მოწინავე ამოცანები

-- ამოცანა 18
-- VIEW: მომავალი რეისები თავისუფალი ადგილების რაოდენობით.
-- VIEW არის შენახული SELECT, მერე ჩვეულებრივ ცხრილივით შემიძლია მივმართო.
-- თავისუფალი ადგილი = სულ ადგილები - აქტიური ჯავშნები.
-- LEFT JOIN იმიტომ, რომ ჯავშნის გარეშე რეისებიც გამოჩნდეს (მათ ყველა ადგილი თავისუფალი აქვთ).
CREATE VIEW vw_UpcomingFlights
AS
SELECT f.Id,
       f.FlightNumber,
       dep.Code AS DepartureCode,
       dep.City AS DepartureCity,
       arr.Code AS ArrivalCode,
       arr.City AS ArrivalCity,
       f.DepartureTime,
       f.ArrivalTime,
       f.Price,
       f.TotalSeats,
       COUNT(r.Id) AS BookedSeats,
       f.TotalSeats - COUNT(r.Id) AS FreeSeats
FROM Flights f
JOIN Airports dep ON dep.Id = f.DepartureAirportId
JOIN Airports arr ON arr.Id = f.ArrivalAirportId
LEFT JOIN Reservations r
       ON r.FlightId = f.Id
      AND r.Status <> N'Cancelled'
WHERE f.DepartureTime > SYSDATETIME()
GROUP BY f.Id, f.FlightNumber, dep.Code, dep.City, arr.Code, arr.City,
         f.DepartureTime, f.ArrivalTime, f.Price, f.TotalSeats;
GO

-- ამოცანა 19
-- პროცედურა BookSeat: ამოწმებს ადგილი თავისუფალია თუ არა და თუ ყველაფერი რიგზეა, ამატებს ჯავშანს.
-- ყველაფერი ერთ ტრანზაქციაშია: ან მთლიანად სრულდება, ან არაფერი იცვლება.
CREATE PROCEDURE BookSeat
    @PassengerId INT,
    @FlightId    INT,
    @SeatNumber  NVARCHAR(5)
AS
BEGIN
    -- NOCOUNT: "(1 rows affected)" შეტყობინებებს აღარ გამოიტანს
    -- XACT_ABORT: ნებისმიერი შეცდომისას ტრანზაქცია ავტომატურად უქმდება
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @TotalSeats INT,
            @DepartureTime DATETIME2(0),
            @BookedSeats INT,
            @NewReservationId INT;

    BEGIN TRY
        -- SERIALIZABLE და UPDLOCK, HOLDLOCK იმისთვისაა, რომ ორმა ადამიანმა ერთსა და იმავე წამს
        -- ერთი და იგივე ადგილი ვერ დაიჯავშნოს: სანამ ეს ტრანზაქცია არ დასრულდება, მეორე ელოდება.
        SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
        BEGIN TRANSACTION;

        -- 1) არსებობს თუ არა მგზავრი
        IF NOT EXISTS (SELECT 1 FROM Passengers WHERE Id = @PassengerId)
            THROW 50001, N'მგზავრი ვერ მოიძებნა.', 1;

        -- 2) ვიღებ რეისის მონაცემებს. თუ რეისი არ არსებობს, @TotalSeats NULL დარჩება.
        SELECT @TotalSeats = TotalSeats,
               @DepartureTime = DepartureTime
        FROM Flights WITH (UPDLOCK, HOLDLOCK)
        WHERE Id = @FlightId;

        IF @TotalSeats IS NULL
            THROW 50002, N'რეისი ვერ მოიძებნა.', 1;

        -- 3) უკვე გაფრენილ რეისზე დაჯავშნა არ შეიძლება
        IF @DepartureTime <= SYSDATETIME()
            THROW 50003, N'რეისი უკვე შედგა, დაჯავშნა შეუძლებელია.', 1;

        -- 4) მთავარი შემოწმება: ეს ადგილი ხომ არ არის უკვე დაკავებული
        IF EXISTS (SELECT 1
                   FROM Reservations WITH (UPDLOCK, HOLDLOCK)
                   WHERE FlightId = @FlightId
                     AND SeatNumber = @SeatNumber
                     AND Status <> N'Cancelled')
            THROW 50004, N'ეს ადგილი უკვე დაკავებულია.', 1;

        -- 5) ერთ მგზავრს ერთ რეისზე ორი ადგილი არ უნდა ჰქონდეს
        IF EXISTS (SELECT 1
                   FROM Reservations
                   WHERE FlightId = @FlightId
                     AND PassengerId = @PassengerId
                     AND Status <> N'Cancelled')
            THROW 50005, N'მგზავრს ამ რეისზე უკვე აქვს ჯავშანი.', 1;

        -- 6) რეისზე თავისუფალი ადგილი თუ დარჩა
        SELECT @BookedSeats = COUNT(*)
        FROM Reservations WITH (UPDLOCK, HOLDLOCK)
        WHERE FlightId = @FlightId
          AND Status <> N'Cancelled';

        IF @BookedSeats >= @TotalSeats
            THROW 50006, N'რეისზე თავისუფალი ადგილი აღარ არის.', 1;

        -- ყველა შემოწმება გაიარა, ვამატებ ჯავშანს
        INSERT INTO Reservations (PassengerId, FlightId, SeatNumber, Status)
        VALUES (@PassengerId, @FlightId, @SeatNumber, N'Confirmed');

        -- SCOPE_IDENTITY() აბრუნებს ახლახან ჩამატებული ჩანაწერის Id-ს
        SET @NewReservationId = SCOPE_IDENTITY();

        COMMIT TRANSACTION;

        SELECT @NewReservationId AS NewReservationId;
    END TRY
    BEGIN CATCH
        -- რამე თუ ჩავარდა, ტრანზაქციას ვაბრუნებ უკან და შეცდომას ზემოთ ვაგდებ (THROW)
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- VIEW-ს შემოწმება: მომავალი რეისები თავისუფალი ადგილებით
SELECT * FROM vw_UpcomingFlights ORDER BY DepartureTime;

-- BookSeat ტესტი 1: წარმატებული დაჯავშნა.
-- რეისი 9-ზე ადგილი 1A Giorgi Beridze-ს ეკავა, ამოცანა 13-ში გაუქმდა და გათავისუფლდა.
-- ახლა მას 11 ნომერი მგზავრი (Nika Abashidze) იკავებს.
EXEC BookSeat @PassengerId = 11, @FlightId = 9, @SeatNumber = N'1A';

-- BookSeat ტესტი 2: დაკავებული ადგილის დაჯავშნის მცდელობა (2A უკვე Luka Bakradze-ს უჭირავს).
-- პროცედურა შეცდომას აგდებს. TRY/CATCH-ში იმიტომ ჩავსვი, რომ შეცდომა ცხრილად გამოჩნდეს
-- და სკრიპტი არ გაჩერდეს.
BEGIN TRY
    EXEC BookSeat @PassengerId = 12, @FlightId = 9, @SeatNumber = N'2A';
END TRY
BEGIN CATCH
    SELECT ERROR_NUMBER() AS ErrorNumber, ERROR_MESSAGE() AS ErrorMessage;
END CATCH

-- შემოწმება: რეისი 9 ისევ სრულად უნდა იყოს დაკავებული (FreeSeats = 0)
SELECT * FROM vw_UpcomingFlights WHERE Id = 9;

-- ამოცანა 20
-- ინდექსი Flights(DepartureTime)-ზე. ამ სვეტით ხშირად ვფილტრავ და ვალაგებ
-- (ამოცანა 3, 4, VIEW), ინდექსით SQL Server მთელ ცხრილს აღარ გადაუყვება.
CREATE NONCLUSTERED INDEX IX_Flights_DepartureTime
    ON Flights (DepartureTime);
