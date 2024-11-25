using Application.DatabaseContext;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Contracts.Request.StreetRequestDto;
using Domain.Entities;
using FluentAssertions;
using FluentAssertions.Common;
using Infrastructure.Implementation.Factory;
using Infrastructure.Implementation.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NetTopologySuite.Geometries;
using StreetService.Controllers;
using Testcontainers.PostgreSql;
using Tests.MockData;

namespace Tests
{
    [TestClass]
    public class StreetControllerTests : IDisposable
    {
        private Mock<IStreetRepository> _mockStreetRepository;
        private Mock<ILogger<StreetController>> _mockLogger;
        private StreetController _controller;

        private PostGISStreetOperationService _postGISService;
        private AlgorithmicStreetOperationService _algorithmicService;
        private StreetContext _dbContext;

        private PostgreSqlContainer _postgresContainer;

        [TestInitialize]
        public void Setup()
        {
            // Start PostgreSQL container
            _postgresContainer = new PostgreSqlBuilder()
                .WithDatabase("testdb")
                .WithUsername("postgres")
                .WithPassword("password")
                .WithImage("postgis/postgis:15-3.3")
                .Build();

            _postgresContainer.StartAsync().Wait();

            // Configure DbContext with the PostgreSQL connection string
            var options = new DbContextOptionsBuilder<StreetContext>()
                .UseNpgsql(_postgresContainer.GetConnectionString(), npgsqlOptions => npgsqlOptions.UseNetTopologySuite())
            .Options;

            _dbContext = new StreetContext(options);

            // Apply migrations or initialize database
            _dbContext.Database.EnsureCreated();

            // Mocking repository and services
            _mockStreetRepository = new Mock<IStreetRepository>();
            _mockLogger = new Mock<ILogger<StreetController>>();

            // Create the actual service instances
            _postGISService = new PostGISStreetOperationService(_dbContext);
            _algorithmicService = new AlgorithmicStreetOperationService(_mockStreetRepository.Object, null);

            // Create the controller with the factory
            var featureFlags = Options.Create(new FeatureFlags { UsePostGIS = true }); // Feature flag enabled for PostGIS
            var factory = new StreetOperationServiceFactory(featureFlags, _postGISService, _algorithmicService);
            _controller = new StreetController(_mockStreetRepository.Object, _mockLogger.Object, null, factory);
        }

        [TestMethod]
        public async Task AddPointToStreet_ValidRequest_ShouldReturnNoContent()
        {
            // Arrange
            int streetId = 1;
            var request = new AddPointRequestDto(50.0, 14.0, true); // Example latitude/longitude
            var newPoint = new Coordinate(request.Latitude, request.Longitude);

            // Act
            var result = await _controller.AddPointToStreet(streetId, request);

            // Assert
            result.Should().BeOfType<NoContentResult>("because a valid request should return NoContent");
        }

        [TestMethod]
        public async Task AddPointToStreet_NullRequest_ShouldReturnBadRequest()
        {
            // Arrange
            int streetId = 1;

            // Act
            var result = await _controller.AddPointToStreet(streetId, null);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>("because a null request should return BadRequest");
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult?.Value.Should().Be("Request cannot be null", "because the error message should indicate null request");
        }

        [TestMethod]
        public async Task AddPointToStreet_ShouldCallPostGIS_WhenFeatureFlagIsEnabled()
        {
            // Arrange
            const int streetId = 1;
            const double latitude = 50.0;
            const double longitude = 14.0;

            var newPoint = new Coordinate(latitude, longitude);

            var featureFlags = Options.Create(new FeatureFlags { UsePostGIS = true });
            var _mockAlgorithmicService = Mock.Of<IStreetOperationService>();

            var factory = new StreetOperationServiceFactory(
                featureFlags,
                _postGISService,
                _mockAlgorithmicService);

            // Act
            await factory.AddPointToStreetAsync(streetId, newPoint, false);

            // Assert
           
        }

        [TestMethod]
        public async Task AddPointToStreet_ShouldCallAlgorithmic_WhenFeatureFlagIsDisabled()
        {
            // Arrange
            const int streetId = 1;
            const double latitude = 50.0;
            const double longitude = 14.0;

            var street = FakeData.GenerateRandomStreet(streetId);
            var featureFlags = Options.Create(new FeatureFlags { UsePostGIS = false }); // Feature flag disabled for PostGIS

            _mockStreetRepository.Setup(r => r.GetStreetByIdAsync(It.Is<int>(id => id == streetId)))
                .ReturnsAsync(street);

            var loggerMock = Mock.Of<ILogger<AlgorithmicStreetOperationService>>();
            var algorithmicService = new AlgorithmicStreetOperationService(_mockStreetRepository.Object, loggerMock);

            var factory = new StreetOperationServiceFactory(
                featureFlags,
                _postGISService, // Mocked PostGIS service
                algorithmicService // Real Algorithmic service
            );

            var newPoint = new Coordinate(latitude, longitude);

            // Act
            await factory.AddPointToStreetAsync(streetId, newPoint, false);

            // Assert
            _mockStreetRepository.Verify(r => r.GetStreetByIdAsync(streetId), Times.Once);
            _mockStreetRepository.Verify(r => r.UpdateStreetAsync(It.IsAny<Street>()), Times.Once);
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            _postgresContainer?.DisposeAsync().AsTask().Wait();
        }
    }
}
