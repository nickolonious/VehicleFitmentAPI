using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        private Mock<IVehicleData> _mockVehicleData;
        private Mock<ICacheService> _mockCacheService;
        private VehicleController _vehicleController;

        [TestInitialize]
        public void Setup()
        {
            _mockVehicleData = new Mock<IVehicleData>();
            _mockCacheService = new Mock<ICacheService>();

            _vehicleController = new VehicleController(
                _mockCacheService.Object,
                _mockVehicleData.Object
            );
        }

        #region Get All Vehicles Tests
        [TestMethod]
        public void Get_ReturnsVehiclesFromCache()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle { VehicleId = 1, Make = "Subaru", Model = "Impreza", ModelYear = 2017, Trim="Sport" },
                new Vehicle { VehicleId = 2, Make = "Mitsubishi", Model = "Mirage", ModelYear = 2024, Trim="Sport" }
            };

            _mockCacheService.Setup(mc => mc.TryGetValue("GetAllVehicles", out vehicles)).Returns(true);

            // Act
            IHttpActionResult actionResult = _vehicleController.Get();
            var contentResult = actionResult as OkNegotiatedContentResult<List<Vehicle>>;

            // Assert
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(2, contentResult.Content.Count);
            CollectionAssert.AreEqual(vehicles, contentResult.Content);

            Assert.IsInstanceOfType(actionResult, typeof(OkNegotiatedContentResult<List<Vehicle>>));

            _mockCacheService.Verify(mc => mc.TryGetValue("GetAllVehicles", out vehicles), Times.Once);

            _mockCacheService.Verify(mc => mc.Set("GetAllVehicles", vehicles, null), Times.Never);
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

            _mockVehicleData.Setup(ds => ds.GetVehicles()).Returns(vehiclesFromDb);

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

            _mockCacheService.Verify(mc => mc.TryGetValue("GetAllVehicles", out vehicles), Times.Once);

            _mockCacheService.Verify(mc => mc.Set("GetAllVehicles", vehiclesFromDb, null), Times.Once);

            CollectionAssert.AreEqual(vehiclesFromDb, getResult.Content);

        }

        [TestMethod]
        public void Get_ReturnsVehiclesFromDatabaseWhenCacheIsNull()
        {
            // Arrange
            List<Vehicle> vehicles = null;

            _mockCacheService.Setup(mc => mc.TryGetValue("GetAllVehicles", out vehicles)).Returns(false);

            var vehiclesFromDb = new List<Vehicle>
            {
                new Vehicle { VehicleId = 1, Make = "Honda", Model = "CR-V", ModelYear = 2013, Trim = "LX" },
                new Vehicle { VehicleId = 2, Make = "Subaru", Model = "Impreza", ModelYear = 2017, Trim = "Sport" }
            };

            _mockVehicleData.Setup(ds => ds.GetVehicles()).Returns(vehiclesFromDb);

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

            _mockCacheService.Verify(mc => mc.TryGetValue("GetAllVehicles", out vehicles), Times.Once);

            _mockCacheService.Verify(mc => mc.Set("GetAllVehicles", vehiclesFromDb, null), Times.Once);

            CollectionAssert.AreEqual(vehiclesFromDb, getResult.Content);
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

            _mockVehicleData.Setup(ds => ds.GetVehicles()).Returns(vehiclesFromDb);

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

            _mockCacheService.Verify(mc => mc.TryGetValue("GetAllVehicles", out expectedVehicles), Times.Once);

            _mockCacheService.Verify(mc => mc.Set("GetAllVehicles", vehiclesFromDb, null), Times.Once);

            CollectionAssert.AreEqual(vehiclesFromDb, contentResult.Content);

        }

        [TestMethod]
        public void Get_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var vehicles = new List<Vehicle>
            {
                new Vehicle { VehicleId = 1, Make = "Subaru", Model = "Impreza", ModelYear = 2017 },
                new Vehicle { VehicleId = 2, Make = "Mitsubishi", Model = "Mirage", ModelYear = 2024 }
            };

            _mockCacheService.Setup(mc => mc.TryGetValue(It.IsAny<string>(), out It.Ref<List<Vehicle>>.IsAny))
                             .Throws(new Exception("Test exception"));

            // Act
            IHttpActionResult actionResult = _vehicleController.Get();

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(ExceptionResult));


            _mockCacheService.Verify(mc => mc.TryGetValue("GetAllVehicles", out vehicles), Times.Once);

            _mockCacheService.Verify(mc => mc.Set("GetAllVehicles", vehicles, null), Times.Never);
        }
        #endregion

        #region GetVehicleById Tests
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

            _mockCacheService.Verify(mc => mc.TryGetValue("GetVehicleId=" + expectedVehicle.VehicleId, out expectedVehicle), Times.Once);

            _mockCacheService.Verify(mc => mc.Set("GetVehicleId=" + expectedVehicle.VehicleId, expectedVehicle, null), Times.Never);

            Assert.AreEqual(expectedVehicle, getResult.Content);

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

            _mockVehicleData.Setup(ds => ds.GetVehicle(expectedVehicle.VehicleId)).Returns(expectedVehicle);

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

            _mockCacheService.Verify(mc => mc.TryGetValue("GetVehicleId=" + expectedVehicle.VehicleId, out expectedVehicle), Times.Once);

            _mockCacheService.Verify(mc => mc.Set("GetVehicleId=" + expectedVehicle.VehicleId, expectedVehicle, null), Times.Once);

            Assert.AreEqual(expectedVehicle, getResult.Content);
        }

        [TestMethod]
        public void Get_CacheReturnsError()
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

            _mockCacheService.Setup(mc => mc.TryGetValue(It.IsAny<string>(), out It.Ref<Vehicle>.IsAny))
                             .Throws(new Exception("Test exception"));

            // Act
            IHttpActionResult actionResult = _vehicleController.Get(2);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(ExceptionResult));

            _mockCacheService.Verify(mc => mc.TryGetValue("GetVehicleId=" + expectedVehicle.VehicleId, out expectedVehicle), Times.Never);

            _mockCacheService.Verify(mc => mc.Set("GetVehicleId=" + expectedVehicle.VehicleId, expectedVehicle, null), Times.Never);
        }
        #endregion

        #region Insert Vehicle Tests
        [TestMethod]
        public void Post_ReturnsBadRequestWhenVehicleMakeIsEmpty()
        {
            // Arrange
            Vehicle vehicle = new Vehicle
            {
                Make = "",
                Model = "Impreza",
                ModelYear = 2017,
                Trim = "Sport"
            };

            // Act
            IHttpActionResult actionResult = _vehicleController.Post(vehicle);
            var badRequestResult = actionResult as BadRequestErrorMessageResult;

            // Assert
            Assert.IsNotNull(badRequestResult);
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestErrorMessageResult));
            Assert.AreEqual("Make, Model, and Trim must be filled out, Model Year must be greater than 1930", badRequestResult.Message);
        
        }

        [TestMethod]
        public void Post_ReturnsBadRequestWhenVehicleMakeIsNull()
        {
            // Arrange
            Vehicle vehicle = new Vehicle
            {
                Make = null,
                Model = "Impreza",
                ModelYear = 2017,
                Trim = "Sport"
            };

            // Act
            IHttpActionResult actionResult = _vehicleController.Post(vehicle);
            var badRequestResult = actionResult as BadRequestErrorMessageResult;

            // Assert
            Assert.IsNotNull(badRequestResult);
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestErrorMessageResult));
            Assert.AreEqual("Make, Model, and Trim must be filled out, Model Year must be greater than 1930", badRequestResult.Message);
        }

        [TestMethod]
        public void Post_ReturnsBadRequestWhenVehicleModelIsEmptyString()
        {
            // Arrange
            Vehicle vehicle = new Vehicle
            {
                Make = "Subaru",
                Model = "",
                ModelYear = 2017,
                Trim = "Sport"
            };

            // Act
            IHttpActionResult actionResult = _vehicleController.Post(vehicle);
            var badRequestResult = actionResult as BadRequestErrorMessageResult;

            // Assert
            Assert.IsNotNull(badRequestResult);
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestErrorMessageResult));
            Assert.AreEqual("Make, Model, and Trim must be filled out, Model Year must be greater than 1930", badRequestResult.Message);
        }
        
        [TestMethod]
        public void Post_ReturnsBadRequestWhenVehicleModelIsNull()
        {
            // Arrange
            Vehicle vehicle = new Vehicle
            {
                Make = "Subaru",
                Model = null,
                ModelYear = 2017,
                Trim = "Sport"
            };

            // Act
            IHttpActionResult actionResult = _vehicleController.Post(vehicle);
            var badRequestResult = actionResult as BadRequestErrorMessageResult;

            // Assert
            Assert.IsNotNull(badRequestResult);
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestErrorMessageResult));
            Assert.AreEqual("Make, Model, and Trim must be filled out, Model Year must be greater than 1930", badRequestResult.Message);
        }

        [TestMethod]
        public void Post_ReturnsBadRequestWhenVehicleTrimIsEmptyString()
        {
            // Arrange
            Vehicle vehicle = new Vehicle
            {
                Make = "Subaru",
                Model = "Impreza",
                ModelYear = 2017,
                Trim = ""
            };

            // Act
            IHttpActionResult actionResult = _vehicleController.Post(vehicle);
            var badRequestResult = actionResult as BadRequestErrorMessageResult;

            // Assert
            Assert.IsNotNull(badRequestResult);
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestErrorMessageResult));
            Assert.AreEqual("Make, Model, and Trim must be filled out, Model Year must be greater than 1930", badRequestResult.Message);
        }

        [TestMethod]
        public void Post_ReturnsBadRequestWhenVehicleTrimIsNull()
        {
            // Arrange
            Vehicle vehicle = new Vehicle
            {
                Make = "Subaru",
                Model = "Impreza",
                ModelYear = 2017,
                Trim = null
            };

            // Act
            IHttpActionResult actionResult = _vehicleController.Post(vehicle);
            var badRequestResult = actionResult as BadRequestErrorMessageResult;

            // Assert
            Assert.IsNotNull(badRequestResult);
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestErrorMessageResult));
            Assert.AreEqual("Make, Model, and Trim must be filled out, Model Year must be greater than 1930", badRequestResult.Message);
        }

        [TestMethod]
        public void Post_ReturnsBadRequestWhenVehicleModelYearIs1930()
        {
            // Arrange
            Vehicle vehicle = new Vehicle
            {
                Make = "Subaru",
                Model = "Impreza",
                ModelYear = 1930,
                Trim = "Sport"
            };

            // Act
            IHttpActionResult actionResult = _vehicleController.Post(vehicle);
            var badRequestResult = actionResult as BadRequestErrorMessageResult;

            // Assert
            Assert.IsNotNull(badRequestResult);
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestErrorMessageResult));
            Assert.AreEqual("Make, Model, and Trim must be filled out, Model Year must be greater than 1930", badRequestResult.Message);
        }

        [TestMethod]
        public void Post_ReturnsBadRequestWhenVehicleTrimIsLessThan1930()
        {
            // Arrange
            Vehicle vehicle = new Vehicle
            {
                Make = "Subaru",
                Model = "Impreza",
                ModelYear = 1929,
                Trim = "Sport"
            };

            // Act
            IHttpActionResult actionResult = _vehicleController.Post(vehicle);
            var badRequestResult = actionResult as BadRequestErrorMessageResult;

            // Assert
            Assert.IsNotNull(badRequestResult);
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestErrorMessageResult));
            Assert.AreEqual("Make, Model, and Trim must be filled out, Model Year must be greater than 1930", badRequestResult.Message);
        }

        [TestMethod]
        public void Post_ReturnsOKWhenAllFieldsAreFilledOut()
        {
            // Arrange
            Vehicle vehicle = new Vehicle
            {
                Make = "Subaru",
                Model = "Impreza",
                ModelYear = 2017,
                Trim = "Sport"
            };

            // Act
            IHttpActionResult actionResult = _vehicleController.Post(vehicle);
            var okResult = actionResult as OkNegotiatedContentResult<Vehicle>;

            // Assert
            Assert.IsNotNull(okResult);
            Assert.IsInstanceOfType(actionResult, typeof(OkNegotiatedContentResult<Vehicle>));
        }

        [TestMethod]
        public void Post_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            Vehicle vehicle = new Vehicle
            {
                Make = "Subaru",
                Model = "Impreza",
                ModelYear = 2017,
                Trim = "Sport"
            };

            _mockVehicleData.Setup(vd => vd.InsertVehicle(It.IsAny<Vehicle>())).Throws(new Exception("Test exception"));

            // Act
            IHttpActionResult actionResult = _vehicleController.Post(vehicle);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(ExceptionResult));
        }
        #endregion

        #region Update Vehicle Tests

        [TestMethod]
        public void Put_ReturnsOKWhenAllFieldsAreFilledOut() {

            // Arrange
            Vehicle vehicle = new Vehicle
            {
                VehicleId = 1,
                Make = "Subaru",
                Model = "Impreza",
                ModelYear = 2017,
                Trim = "Sport"
            };
            
            string updateQuery = null;

            _mockVehicleData.Setup(vd => vd.UpdateVehicle(It.IsAny<List<SqlParameter>>(), It.IsAny<string>(), It.IsAny<Vehicle>()))
                            .Callback<List<SqlParameter>, string, Vehicle>((parameters, query, v) => updateQuery = query)
                            .Returns(vehicle);

            // Act
            IHttpActionResult actionResult = _vehicleController.Put(vehicle);

            // Assert
            Assert.IsNotNull(updateQuery);
            Assert.AreEqual("UPDATE Vehicle SET Make = @Make, Model = @Model, Trim = @Trim, ModelYear = @ModelYear WHERE VehicleId = @VehicleId", updateQuery);
            Assert.IsInstanceOfType(actionResult, typeof(OkNegotiatedContentResult<Vehicle>));

            _mockCacheService.Verify(mc => mc.Set("GetVehicleId=" + vehicle.VehicleId, vehicle, null), Times.Once);
            _mockCacheService.Verify(mc => mc.Remove("GetAllVehicles"), Times.Once);

        }

        [TestMethod]
        public void Put_ReturnsOKWhenEverythingButMakeIsFilledOut()
        {
            // Arrange
            Vehicle vehicle = new Vehicle
            {
                VehicleId = 1,
                Model = "Impreza",
                ModelYear = 2017,
                Trim = "Sport"
            };

            string updateQuery = null;

            _mockVehicleData.Setup(vd => vd.UpdateVehicle(It.IsAny<List<SqlParameter>>(), It.IsAny<string>(), It.IsAny<Vehicle>()))
                            .Callback<List<SqlParameter>, string, Vehicle>((parameters, query, v) => updateQuery = query)
                            .Returns(vehicle);

            // Act
            IHttpActionResult actionResult = _vehicleController.Put(vehicle);

            // Assert
            Assert.IsNotNull(updateQuery);
            Assert.AreEqual("UPDATE Vehicle SET Model = @Model, Trim = @Trim, ModelYear = @ModelYear WHERE VehicleId = @VehicleId", updateQuery);
            Assert.IsInstanceOfType(actionResult, typeof(OkNegotiatedContentResult<Vehicle>));

            _mockCacheService.Verify(mc => mc.Set("GetVehicleId=" + vehicle.VehicleId, vehicle, null), Times.Once);
            _mockCacheService.Verify(mc => mc.Remove("GetAllVehicles"), Times.Once);
        }

        [TestMethod]
        public void Put_ReturnsOKWhenEverythingButModelIsFilledOut()
        {
            // Arrange
            Vehicle vehicle = new Vehicle
            {
                VehicleId = 1,
                Make = "Subaru",
                ModelYear = 2017,
                Trim = "Sport"
            };

            string updateQuery = null;

            _mockVehicleData.Setup(vd => vd.UpdateVehicle(It.IsAny<List<SqlParameter>>(), It.IsAny<string>(), It.IsAny<Vehicle>()))
                            .Callback<List<SqlParameter>, string, Vehicle>((parameters, query, v) => updateQuery = query)
                            .Returns(vehicle);

            // Act
            IHttpActionResult actionResult = _vehicleController.Put(vehicle);

            // Assert
            Assert.IsNotNull(updateQuery);
            Assert.AreEqual("UPDATE Vehicle SET Make = @Make, Trim = @Trim, ModelYear = @ModelYear WHERE VehicleId = @VehicleId", updateQuery);
            Assert.IsInstanceOfType(actionResult, typeof(OkNegotiatedContentResult<Vehicle>));

            _mockCacheService.Verify(mc => mc.Set("GetVehicleId=" + vehicle.VehicleId, vehicle, null), Times.Once);
            _mockCacheService.Verify(mc => mc.Remove("GetAllVehicles"), Times.Once);
        }

        [TestMethod]
        public void Put_ReturnsOKWhenEverythingButModelYearIsFilledOut()
        {
            // Arrange
            Vehicle vehicle = new Vehicle
            {
                VehicleId = 1,
                Make = "Subaru",
                Model = "Impreza",
                Trim = "Sport"
            };

            string updateQuery = null;

            _mockVehicleData.Setup(vd => vd.UpdateVehicle(It.IsAny<List<SqlParameter>>(), It.IsAny<string>(), It.IsAny<Vehicle>()))
                            .Callback<List<SqlParameter>, string, Vehicle>((parameters, query, v) => updateQuery = query)
                            .Returns(vehicle);

            // Act
            IHttpActionResult actionResult = _vehicleController.Put(vehicle);

            // Assert
            Assert.IsNotNull(updateQuery);
            Assert.AreEqual("UPDATE Vehicle SET Make = @Make, Model = @Model, Trim = @Trim WHERE VehicleId = @VehicleId", updateQuery);
            Assert.IsInstanceOfType(actionResult, typeof(OkNegotiatedContentResult<Vehicle>));

            _mockCacheService.Verify(mc => mc.Set("GetVehicleId=" + vehicle.VehicleId, vehicle, null), Times.Once);
            _mockCacheService.Verify(mc => mc.Remove("GetAllVehicles"), Times.Once);
        }

        [TestMethod]
        public void Put_ReturnsOKWhenEverythingButTrimIsFilledOut()
        {
            // Arrange
            Vehicle vehicle = new Vehicle
            {
                VehicleId = 1,
                Make = "Subaru",
                Model = "Impreza",
                ModelYear = 2017,
            };

            string updateQuery = null;

            _mockVehicleData.Setup(vd => vd.UpdateVehicle(It.IsAny<List<SqlParameter>>(), It.IsAny<string>(), It.IsAny<Vehicle>()))
                            .Callback<List<SqlParameter>, string, Vehicle>((parameters, query, v) => updateQuery = query)
                            .Returns(vehicle);

            // Act
            IHttpActionResult actionResult = _vehicleController.Put(vehicle);

            // Assert
            Assert.IsNotNull(updateQuery);
            Assert.AreEqual("UPDATE Vehicle SET Make = @Make, Model = @Model, ModelYear = @ModelYear WHERE VehicleId = @VehicleId", updateQuery);
            Assert.IsInstanceOfType(actionResult, typeof(OkNegotiatedContentResult<Vehicle>));

            _mockCacheService.Verify(mc => mc.Set("GetVehicleId=" + vehicle.VehicleId, vehicle, null), Times.Once);
            _mockCacheService.Verify(mc => mc.Remove("GetAllVehicles"), Times.Once);
        }

        [TestMethod]
        public void Put_ThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            Vehicle vehicle = new Vehicle
            {
                VehicleId = 1, // Ensure the VehicleId is set
                Make = "Subaru",
                Model = "Impreza",
                ModelYear = 2017,
                Trim = "Sport"
            };

            _mockVehicleData.Setup(vd => vd.UpdateVehicle(It.IsAny<List<SqlParameter>>(), It.IsAny<string>(), It.IsAny<Vehicle>()))
                            .Throws(new Exception("Test exception"));

            // Act
            IHttpActionResult actionResult = _vehicleController.Put(vehicle);

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(ExceptionResult));

            _mockCacheService.Verify(mc => mc.Set("GetVehicleId=" + vehicle.VehicleId, vehicle, null), Times.Never);
            _mockCacheService.Verify(mc => mc.Remove("GetAllVehicles"), Times.Never);

        }


        [TestMethod]
        public void Put_ReturnsBadRequestWhenVehicleIdIsZero()
        {

            // Arrange
            Vehicle vehicle = new Vehicle
            {
                VehicleId = 0,
                Make = "Subaru",
                Model = "Impreza",
                ModelYear = 2017,
                Trim = "Sport"
            };

            // Act
            IHttpActionResult actionResult = _vehicleController.Put(vehicle);
            var badResult = actionResult as BadRequestErrorMessageResult;

            // Assert
            Assert.IsNotNull(badResult);
            Assert.IsInstanceOfType(badResult, typeof(BadRequestErrorMessageResult));

            _mockCacheService.Verify(mc => mc.Set("GetVehicleId=" + vehicle.VehicleId, vehicle, null), Times.Never);
            _mockCacheService.Verify(mc => mc.Remove("GetAllVehicles"), Times.Never);
        }
        #endregion

        #region Delete Vehicle Tests

        [TestMethod]
        public void Delete_ReturnsBadRequestWhenVehicleIdIsZero()
        {
            // Arrange
            int vehicleId = 0;

            // Act
            IHttpActionResult actionResult = _vehicleController.Delete(vehicleId);
            var badResult = actionResult as BadRequestErrorMessageResult;

            // Assert
            Assert.IsNotNull(badResult);
            Assert.IsInstanceOfType(badResult, typeof(BadRequestErrorMessageResult));

            _mockCacheService.Verify(mc => mc.Remove("GetAllVehicles"), Times.Never);
            _mockCacheService.Verify(mc => mc.Remove("GetVehicleId=" + vehicleId), Times.Never);

        }

        [TestMethod]
        public void Delete_ReturnsOKWhenVehicleIdIsGreaterThanZero()
        {
            // Arrange
            int vehicleId = 1;
            int expectedRowsAffected = 1;
            _mockVehicleData.Setup(ds => ds.DeleteVehicle(vehicleId)).Returns(expectedRowsAffected);


            // Act
            IHttpActionResult actionResult = _vehicleController.Delete(vehicleId);
            var okResult = actionResult as OkNegotiatedContentResult<string>;

            // Assert
            Assert.IsNotNull(okResult);
            Assert.IsInstanceOfType(okResult, typeof(OkNegotiatedContentResult<string>));
            Assert.AreEqual("Vehicle and associated fitments deleted!", okResult.Content);

            // Verify that the DeleteVehicle method was called and returned the expected value
            _mockVehicleData.Verify(ds => ds.DeleteVehicle(vehicleId), Times.Once);
            int actualRowsAffected = _mockVehicleData.Object.DeleteVehicle(vehicleId);
            Assert.AreEqual(expectedRowsAffected, actualRowsAffected);

            // Verify that the cache was updated correctly
            _mockCacheService.Verify(mc => mc.Remove("GetAllVehicles"), Times.Once);
            _mockCacheService.Verify(mc => mc.Remove("GetVehicleId=" + vehicleId), Times.Once);
        }

        [TestMethod]
        public void Delete_ReturnsNotFoundWhenRowsAffectsIsZero()
        {
            // Arrange
            int vehicleId = 1;
            int expectedRowsAffected = 0;
            _mockVehicleData.Setup(ds => ds.DeleteVehicle(vehicleId)).Returns(expectedRowsAffected);


            // Act
            IHttpActionResult actionResult = _vehicleController.Delete(vehicleId);
            var notFoundResult = actionResult as NotFoundResult;

            // Assert
            Assert.IsNotNull(notFoundResult);
            Assert.IsInstanceOfType(notFoundResult, typeof(NotFoundResult));

            _mockVehicleData.Verify(ds => ds.DeleteVehicle(vehicleId), Times.Once);
            int actualRowsAffected = _mockVehicleData.Object.DeleteVehicle(vehicleId);
            Assert.AreEqual(expectedRowsAffected, actualRowsAffected);

            _mockCacheService.Verify(mc => mc.Remove("GetAllVehicles"), Times.Never);
            _mockCacheService.Verify(mc => mc.Remove("GetVehicleId=" + vehicleId), Times.Never);
        }

        [TestMethod]
        public void Delete_ReturnsServerError()
        {
            // Arrange
            int vehicleId = 1;
            _mockVehicleData.Setup(vd => vd.DeleteVehicle(It.IsAny<int>())).Throws(new Exception("Test exception"));


            IHttpActionResult actionResult = _vehicleController.Delete(vehicleId);
            var errorResult = actionResult as ExceptionResult;

            Assert.IsNotNull(errorResult);
            Assert.IsInstanceOfType(errorResult, typeof(ExceptionResult));
            
            _mockVehicleData.Verify(ds => ds.DeleteVehicle(vehicleId), Times.Once);

            _mockCacheService.Verify(mc => mc.Remove("GetAllVehicles"), Times.Never);
            _mockCacheService.Verify(mc => mc.Remove("GetVehicleId=" + vehicleId), Times.Never);

        }
        #endregion
    }
}