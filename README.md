# VehicleFitmentAPI

Steps to get started

1. Create Database Locally

2. Run the following commands

CREATE TABLE Vehicle (
    VehicleId INT Identity(1,1) PRIMARY KEY NOT NULL,
    Make VARCHAR(60) NOT NULL,
    Model VARCHAR(60) NOT NULL,
    ModelYear INT NOT NULL,
    Trim VARCHAR(30) NOT NULL
);

Create Part table
CREATE TABLE Part (
    PartId INT Identity(1,1) PRIMARY KEY NOT NULL,
    PartsNumber INT NOT NULL,
    PartsName VARCHAR(60) NOT NULL,
    Description VARCHAR(400),
    ImageUrl VARCHAR(200)
);

Create Fitment table
CREATE TABLE Fitment (
    FitmentID INT IDENTITY(1,1) PRIMARY KEY,
    VehicleID INT NOT NULL,
    PartID INT NOT NULL,
    CONSTRAINT FK_VehicleID FOREIGN KEY (VehicleID) REFERENCES Vehicle(VehicleID),
    CONSTRAINT FK_PartID FOREIGN KEY (PartID) REFERENCES Part(PartID)
);

3. Modify the connection string in "Web.config" to match your db connection

4. Clone Repo from Github




