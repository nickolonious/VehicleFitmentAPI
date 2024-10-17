# VehicleFitmentAPI

Demo
https://vehiclefitmentapi20241016140327.azurewebsites.net/

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

5. Run the Application in either IIS or Docker (if that's setup on your computer)

6. Go to the Admin Page where you can Add / Update / Delete Vehicle and Parts as well as Add Fitment Relationships

7. Once enough are filled out you can view them on the Home Page

Biggest Challenges
- I've never uploaded images into an app before, to further improve I would setup a image bucket on Azure and upload them there
- Deployment, first time deploying to Azure
- There are some newer flavors on the front end with .NET core

Things I don't feel great about
- The UI in general
- I'd probably consolidate add, update, delete onto the same inputs for a simpler UI
- There are a couple of caching issues I would have liked to resolve but I had already gone way over on alotted time

Overall I spent about 12 hours on this project which I know is way over.




