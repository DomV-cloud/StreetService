using Application.DatabaseContext;
using Application.Interfaces.Repository;
using Application.Interfaces.Service;
using Contracts.Request.StreetRequestDto;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Implementation.Factory;
using Infrastructure.Implementation.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NetTopologySuite.Geometries;
using Npgsql;
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
        private Mock<IDatabaseExecutor> _mockExecutor;
        private Mock<IServiceProvider> _mockServiceProvider;
        private StreetController _controller;

        private PostGISStreetOperationService _postGISService;
        private AlgorithmicStreetOperationService _algorithmicService;
        private StreetContext _dbContext;

        private PostgreSqlContainer _postgresContainer;

        [TestInitialize]
        public void Setup()
        {
            _postgresContainer = new PostgreSqlBuilder()
                .WithDatabase("testdb")
                .WithUsername("postgres")
                .WithPassword("password")
                .WithImage("postgis/postgis:15-3.3")
                .Build();

            _postgresContainer.StartAsync().Wait();

            var options = new DbContextOptionsBuilder<StreetContext>()
                .UseNpgsql(_postgresContainer.GetConnectionString(), npgsqlOptions => npgsqlOptions.UseNetTopologySuite())
            .Options;

            _dbContext = new StreetContext(options);

            _dbContext.Database.EnsureCreated();

            _mockStreetRepository = new Mock<IStreetRepository>();
            _mockLogger = new Mock<ILogger<StreetController>>();
            _mockExecutor = new Mock<IDatabaseExecutor>();
            _mockServiceProvider = new Mock<IServiceProvider>();

            _postGISService = new PostGISStreetOperationService(_dbContext, _mockExecutor.Object);
            _algorithmicService = new AlgorithmicStreetOperationService(_mockStreetRepository.Object, null);

            var featureFlags = Options.Create(new FeatureFlags { UsePostGIS = true });
            var factory = new StreetOperationServiceFactory(featureFlags, _mockServiceProvider.Object);
            _controller = new StreetController(_mockStreetRepository.Object, _mockLogger.Object, null, factory);
        }

        [TestMethod]
        public async Task AddPointToStreet_ShouldAddPointAtTheEnd_WhenAddToEndIsFalse()
        {
            const int streetId = 1;
            const double latitude = 50.0;
            const double longitude = 14.0;

            var newPoint = new Coordinate(latitude, longitude);

            var featureFlags = Options.Create(new FeatureFlags { UsePostGIS = false });

            var street = FakeData.GenerateRandomStreet(streetId);
            _mockStreetRepository.Setup(r => r.GetStreetByIdAsync(It.Is<int>(id => id == streetId)))
                .ReturnsAsync(street);

            var loggerMock = Mock.Of<ILogger<AlgorithmicStreetOperationService>>();
            var algorithmicService = new AlgorithmicStreetOperationService(_mockStreetRepository.Object, loggerMock);

            var mockServiceProvider = new Mock<IServiceProvider>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(AlgorithmicStreetOperationService)))
                .Returns(algorithmicService);

            var factory = new StreetOperationServiceFactory(featureFlags, mockServiceProvider.Object);

            await factory.AddPointToStreetAsync(streetId, newPoint, false);

            var firstPoint = street.Geometry.Coordinates.First();
            firstPoint.X.Should().Be(latitude);
            firstPoint.Y.Should().Be(longitude);
        }

        [TestMethod]
        public async Task AddPointToStreet_ShouldAddPointAtTheEnd_WhenAddToEndIsTrue()
        {
            const int streetId = 1;
            const double latitude = 50.0;
            const double longitude = 14.0;

            var newPoint = new Coordinate(latitude, longitude);

            var featureFlags = Options.Create(new FeatureFlags { UsePostGIS = false });

            var street = FakeData.GenerateRandomStreet(streetId);
            _mockStreetRepository.Setup(r => r.GetStreetByIdAsync(It.Is<int>(id => id == streetId)))
                .ReturnsAsync(street);

            var loggerMock = Mock.Of<ILogger<AlgorithmicStreetOperationService>>();
            var algorithmicService = new AlgorithmicStreetOperationService(_mockStreetRepository.Object, loggerMock);

            var mockServiceProvider = new Mock<IServiceProvider>();

            mockServiceProvider.Setup(sp => sp.GetService(typeof(AlgorithmicStreetOperationService)))
                .Returns(algorithmicService);

            var factory = new StreetOperationServiceFactory(featureFlags, mockServiceProvider.Object);

            await factory.AddPointToStreetAsync(streetId, newPoint, true);

            var lastPoint = street.Geometry.Coordinates.Last();
            lastPoint.X.Should().Be(latitude);
            lastPoint.Y.Should().Be(longitude);
        }

        [TestMethod]
        public async Task AddPointToStreet_ShouldCallPostGIS_WhenFeatureFlagIsEnabled()
        {
            const int streetId = 1;
            const double latitude = 50.0;
            const double longitude = 14.0;

            var newPoint = new Coordinate(latitude, longitude);

            var featureFlags = Options.Create(new FeatureFlags { UsePostGIS = true });

            var mockExecutor = new Mock<IDatabaseExecutor>();
            mockExecutor.Setup(ex => ex.ExecuteSqlRawAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>()
            ))
            .Returns(Task.CompletedTask);

            var postGISService = new PostGISStreetOperationService(_dbContext,mockExecutor.Object);

            var mockServiceProvider = new Mock<IServiceProvider>();
            mockServiceProvider.Setup(sp => sp.GetService(typeof(PostGISStreetOperationService)))
                .Returns(postGISService);

            var factory = new StreetOperationServiceFactory(featureFlags, mockServiceProvider.Object);

            await factory.AddPointToStreetAsync(streetId, newPoint, false);

            // Assert
            mockExecutor.Verify(
                e => e.ExecuteSqlRawAsync(
                    It.Is<string>(sql => sql.Contains("ST_AddPoint")), // SQL dotaz obsahuje ST_AddPoint
                    It.Is<object[]>(parameters =>
                        parameters.Length == 3 &&
                        parameters.OfType<NpgsqlParameter>().Any(p => p.ParameterName == "@x" && (double)p.Value == latitude) &&
                        parameters.OfType<NpgsqlParameter>().Any(p => p.ParameterName == "@y" && (double)p.Value == longitude) &&
                        parameters.OfType<NpgsqlParameter>().Any(p => p.ParameterName == "@streetId" && (int)p.Value == streetId)
                    )
                ),
                Times.Once, // SQL dotaz by měl být proveden přesně jednou
                "The SQL query for adding a point to the street should be executed exactly once with correct parameters."
            );
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
            _postgresContainer?.DisposeAsync().AsTask().Wait();
        }
    }
}
