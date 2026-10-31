-- შენიშვნა: სერვერზე უკვე არსებობს სხვა LibraryDb, ამიტომ ვიყენებთ LibraryDemoDb-ს
CREATE DATABASE LibraryDemoDb;
GO
USE LibraryDemoDb;
GO
CREATE TABLE Authors (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Country NVARCHAR(50) NULL
);
CREATE TABLE Books (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Year INT NOT NULL,
    AuthorId INT NOT NULL FOREIGN KEY REFERENCES Authors(Id)
);
INSERT INTO Authors (FullName, Country)
VALUES (N'ილია ჭავჭავაძე', N'საქართველო'),
       (N'ვაჟა-ფშაველა', N'საქართველო');
INSERT INTO Books (Title, Year, AuthorId)
VALUES (N'კაცია-ადამიანი?!', 1863, 1),
       (N'ოთარაანთ ქვრივი', 1887, 1),
       (N'ალუდა ქეთელაური', 1888, 2);
