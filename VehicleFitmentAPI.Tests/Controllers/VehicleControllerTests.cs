using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Results;
using VehicleFitmentAPI.Controllers;
using VehicleFitmentAPI.Interfaces;
using VehicleFitmentAPI.Models;

namespace VehicleFitmentAPI.Tests.Controllers
{
    
    [TestClass]
    public class VehicleControllerTests
    {
        private Mock<IVehicleService> _mockVehicleService;
        private Mock<ICacheService> _mockCacheService;
        private VehicleController _vehicleController;

        [TestInitialize]
        public void Setup()
        {
            _mockVehicleService = new Mock<IVehicleService>();
            _mockCacheService = new Mock<ICacheService>();

            _vehicleController = new VehicleController(
                _mockCacheService.Object,
                _mockVehicleService.Object
            );
        }

        [TestMethod]
        public void Get_ReturnsVehiclesFromCache()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle { VehicleId = 1, Make = "Subaru", Model = "Impreza", ModelYear = 2017 },
                new Vehicle { VehicleId = 2, Make = "Mitsubishi", Model = "Mirage", ModelYear = 2024 }
            };

            _mockCacheService.Setup(mc => mc.TryGetValue("GetAllVehicles", out vehicles)).Returns(true);

            // Act
            IHttpActionResult actionResult = _vehicleController.Get();
            var contentResult = actionResult as OkNegotiatedContentResult<List<Vehicle>>;

            // Assert
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(2, contentResult.Content.Count);
            Assert.AreEqual("Subaru", contentResult.Content[0].Make);
            Assert.AreEqual("Impreza", contentResult.Content[0].Model);
            Assert.AreEqual(2017, contentResult.Content[0].ModelYear);
            Assert.IsInstanceOfType(actionResult, typeof(OkNegotiatedContentResult<List<Vehicle>>));
        }

        [TestMethod]
        public void Get_ReturnsVehiclesFromDatabaseWhenCacheHasZeroVehicles()
        {
            // Arrange
            var vehicles = new List<Vehicle>();

            _mockCacheService.Setup(mc => mc.TryGetValue("GetAllVehicles", out vehicles)).Returns(true);

            var vehiclesFromDb = new List<Vehicle>
            {
                new Vehicle { VehicleId = 1, Make = "Honda", Model = "CR-V", ModelYear = 2013, Trim = "LX" },
                new Vehicle { VehicleId = 2, Make = "Subaru", Model = "Impreza", ModelYear = 2017, Trim = "Sport" }
            };

            _mockVehicleService.Setup(ds => ds.GetVehicles()).Returns(vehiclesFromDb);

            // Act
            IHttpActionResult actionResult = _vehicleController.Get();
            var getResult = actionResult as OkNegotiatedContentResult<List<Vehicle>>;

            // Assert
            Assert.IsNotNull(getResult);
            Assert.IsNotNull(getResult.Content);
            Assert.AreEqual(2, getResult.Content.Count);
            Assert.AreEqual("Honda", getResult.Content[0].Make);
            Assert.AreEqual("CR-V", getResult.Content[0].Model);
            Assert.AreEqual(2013, getResult.Content[0].ModelYear);
            Assert.AreEqual("LX", getResult.Content[0].Trim);
            Assert.AreEqual("Subaru", getResult.Content[1].Make);
            Assert.AreEqual("Impreza", getResult.Content[1].Model);
            Assert.AreEqual(2017, getResult.Content[1].ModelYear);
            Assert.AreEqual("Sport", getResult.Content[1].Trim);
            Assert.IsInstanceOfType(actionResult, typeof(OkNegotiatedContentResult<List<Vehicle>>));
        }

        [TestMethod]
        public void Get_ReturnsVehiclesFromDatabaseWhenCacheIsNull()
        {
            // Arrange
            List<Vehicle> vehicles = null;

            _mockCacheService.Setup(mc => mc.TryGetValue("GetAllVehicles", out vehicles)).Returns(true);

            var vehiclesFromDb = new List<Vehicle>
            {
                new Vehicle { VehicleId = 1, Make = "Honda", Model = "CR-V", ModelYear = 2013, Trim = "LX" },
                new Vehicle { VehicleId = 2, Make = "Subaru", Model = "Impreza", ModelYear = 2017, Trim = "Sport" }
            };

            _mockVehicleService.Setup(ds => ds.GetVehicles()).Returns(vehiclesFromDb);

            // Act
            IHttpActionResult actionResult = _vehicleController.Get();
            var getResult = actionResult as OkNegotiatedContentResult<List<Vehicle>>;

            // Assert
            Assert.IsNotNull(getResult);
            Assert.IsNotNull(getResult.Content);
            Assert.AreEqual(2, getResult.Content.Count);
            Assert.AreEqual("Honda", getResult.Content[0].Make);
            Assert.AreEqual("CR-V", getResult.Content[0].Model);
            Assert.AreEqual(2013, getResult.Content[0].ModelYear);
            Assert.AreEqual("LX", getResult.Content[0].Trim);
            Assert.AreEqual("Subaru", getResult.Content[1].Make);
            Assert.AreEqual("Impreza", getResult.Content[1].Model);
            Assert.AreEqual(2017, getResult.Content[1].ModelYear);
            Assert.AreEqual("Sport", getResult.Content[1].Trim);
            Assert.IsInstanceOfType(actionResult, typeof(OkNegotiatedContentResult<List<Vehicle>>));
        }

        [TestMethod]
        public void Get_ReturnsVehiclesFromDatabase()
        {
            // Arrange
            List<Vehicle> expectedVehicles = null;

            _mockCacheService.Setup(mc => mc.TryGetValue("GetAllVehicles", out expectedVehicles)).Returns(false);

            var vehiclesFromDb = new List<Vehicle>
            {
                new Vehicle { VehicleId = 1, Make = "Subaru", Model = "Impreza", ModelYear = 2017 },
                new Vehicle { VehicleId = 2, Make = "Mitsubishi", Model = "Mirage", ModelYear = 2024 }
            };

            _mockVehicleService.Setup(ds => ds.GetVehicles()).Returns(vehiclesFromDb);

            // Act
            IHttpActionResult actionResult = _vehicleController.Get();
            var contentResult = actionResult as OkNegotiatedContentResult<List<Vehicle>>;

            // Assert
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(2, contentResult.Content.Count);
            Assert.AreEqual("Subaru", contentResult.Content[0].Make);
            Assert.AreEqual("Mitsubishi", contentResult.Content[1].Make);
            Assert.IsInstanceOfType(actionResult, typeof(OkNegotiatedContentResult<List<Vehicle>>));
        }

        [TestMethod]
        public void Get_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockCacheService.Setup(mc => mc.TryGetValue(It.IsAny<string>(), out It.Ref<List<Vehicle>>.IsAny))
                             .Throws(new Exception("Test exception"));

            // Act
            IHttpActionResult actionResult = _vehicleController.Get();

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(ExceptionResult));
        }

        [TestMethod]
        public void Get_GetsCorrectVehicleFromCacheWhenIdIsPassed()
        {
            // Arrange
            Vehicle expectedVehicle = new Vehicle { 
                VehicleId = 1,
                Make = "Subaru",
                Model = "Impreza",
                ModelYear = 2017,
                Trim = "Sport"
            };

            string cacheKey = "GetVehicleId=" + expectedVehicle.VehicleId;

            _mockCacheService.Setup(mc => mc.TryGetValue(cacheKey, out expectedVehicle)).Returns(true);

            
            // Act
            IHttpActionResult actionResult = _vehicleController.Get(expectedVehicle.VehicleId);
            
            // Assert
            var getResult = actionResult as OkNegotiatedContentResult<Vehicle>;
            Assert.IsNotNull(getResult);
            Assert.IsNotNull(getResult.Content);
            Assert.AreEqual(1, getResult.Content.VehicleId);
            Assert.AreEqual("Subaru", getResult.Content.Make);
            Assert.AreEqual("Impreza", getResult.Content.Model);
            Assert.AreEqual(2017, getResult.Content.ModelYear);
            Assert.AreEqual("Sport", getResult.Content.Trim);
            Assert.IsInstanceOfType(actionResult, typeof(OkNegotiatedContentResult<Vehicle>));
        }

        [TestMethod]
        public void Get_GetsCorrectVehicleFromdatabaseWhenIdIsPassed()
        {
            // Arrange
            Vehicle expectedVehicle = new Vehicle
            {
                VehicleId = 1,
                Make = "Subaru",
                Model = "Impreza",
                ModelYear = 2017,
                Trim = "Sport"
            };

            _mockCacheService.Setup(mc => mc.TryGetValue("GetVehicleId=", out expectedVehicle)).Returns(false);

            _mockVehicleService.Setup(ds => ds.GetVehicle(expectedVehicle.VehicleId)).Returns(expectedVehicle);

            // Act
            IHttpActionResult actionResult = _vehicleController.Get(expectedVehicle.VehicleId);

            // Assert
            var getResult = actionResult as OkNegotiatedContentResult<Vehicle>;
            Assert.IsNotNull(getResult);
            Assert.IsNotNull(getResult.Content);
            Assert.AreEqual(1, getResult.Content.VehicleId);
            Assert.AreEqual("Subaru", getResult.Content.Make);
            Assert.AreEqual("Impreza", getResult.Content.Model);
            Assert.AreEqual(2017, getResult.Content.ModelYear);
            Assert.AreEqual("Sport", getResult.Content.Trim);
            Assert.IsInstanceOfType(actionResult, typeof(OkNegotiatedContentResult<Vehicle>));
        }
    }
}