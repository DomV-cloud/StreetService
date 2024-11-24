using Application.Interfaces.Repository;
using AutoMapper;
using Contracts.Request.StreetRequestDto;
using Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NetTopologySuite.Geometries;
using StreetService.Controllers;
using Tests.MockData;

namespace Tests
{
    [TestClass]
    public class StreetControllerTests
    {
        private Mock<IStreetRepository> _mockRepository;
        private Mock<ILogger<StreetController>> _mockLogger;
        private StreetController _controller;
        private Mock<IMapper> _mockMapper;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IStreetRepository>();
            _mockLogger = new Mock<ILogger<StreetController>>();
            _mockMapper = new Mock<IMapper>();

            // No AutoMapper usage in this specific test
            _controller = new StreetController(_mockRepository.Object, _mockLogger.Object, _mockMapper.Object);
        }

        [TestMethod]
        public async Task AddPointToStreet_AddToEnd_ShouldAddPointAtEnd()
        {
            // Arrange
            var streetId = FakeData.GenerateRandomId();

            var initialCoordinates = FakeData.GenerateRandomCoordinates(2);

            var street = FakeData.GenerateRandomStreet(streetId);

            var newPoint = FakeData.GenerateRandomCoordinate();

            _mockRepository.Setup(r => r.GetStreetByIdAsync(It.Is<int>(i => i == streetId)))
                .ReturnsAsync(street);

            // this should be refactored
            _mockRepository.Setup(r => r.AddPointToStreetAsync(It.Is<int>(i => i == streetId),
                It.Is<Coordinate>(p => p.X == newPoint.X && p.Y == newPoint.Y), true))
                .Callback<int, Coordinate, bool>((_, __, ___) =>
                {
                    var updatedCoordinates = street.Geometry.Coordinates.Concat(new[] { newPoint }).ToArray();
                    street.Geometry = new LineString(updatedCoordinates);
                })
                .Returns(Task.CompletedTask);

            var request = new AddPointRequestDto(newPoint.X, newPoint.Y, true);

            // Act
            var result = await _controller.AddPointToStreet(streetId, request);

            // Assert
            result.Should().BeOfType<NoContentResult>("because a valid request should return NoContentResult");
            street.Geometry.Coordinates.Should().HaveCount(3, "because the street should have 3 coordinates after the addition");
            street.Geometry.Coordinates.Last().Should().Be(newPoint, "because the new point should be added to the end");
        }

        [TestMethod]
        public async Task AddPointToStreet_AddToStart_ShouldAddPointAtStart()
        {
            // Arrange
            var streetId = FakeData.GenerateRandomId();

            var initialCoordinates = FakeData.GenerateRandomCoordinates(2);

            var street = FakeData.GenerateRandomStreet(streetId);

            var newPoint = FakeData.GenerateRandomCoordinate();

            _mockRepository.Setup(r => r.GetStreetByIdAsync(It.Is<int>(i => i == streetId)))
                .ReturnsAsync(street);

            _mockRepository.Setup(r => r.AddPointToStreetAsync(It.Is<int>(i => i == streetId),
                It.Is<Coordinate>(p => p.X == newPoint.X && p.Y == newPoint.Y), false))
                .Callback<int, Coordinate, bool>((_, __, ___) =>
                {
                    var updatedCoordinates = new[] { newPoint }.Concat(street.Geometry.Coordinates).ToArray();
                    street.Geometry = new LineString(updatedCoordinates);
                })
                .Returns(Task.CompletedTask);

            var request = new AddPointRequestDto(newPoint.X, newPoint.Y, false);

            var result = await _controller.AddPointToStreet(streetId, request);

            result.Should().BeOfType<NoContentResult>("because a valid request should return NoContentResult");
            street.Geometry.Coordinates.Should().HaveCount(3, "because the street should have 3 coordinates after the addition");
            street.Geometry.Coordinates.First().Should().Be(newPoint, "because the new point should be added to the start");
        }

        [TestMethod]
        public async Task AddPointToStreet_InvalidRequest_ShouldReturnBadRequest()
        {
            var streetId = 1;

            var result = await _controller.AddPointToStreet(It.Is<int>(i => i == streetId), null);

            result.Should().BeOfType<BadRequestObjectResult>("because a null request should return BadRequestObjectResult");

            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().Be("Point request is null", "because the error message should indicate the request is null");
        }
    }
}
