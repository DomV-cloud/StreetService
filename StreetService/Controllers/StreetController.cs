using Application.Interfaces.Repository;
using AutoMapper;
using Azure.Core;
using Contracts.Request.StreetRequestDto;
using Contracts.Responds.StreetResponseDto;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace StreetService.Controllers
{
    [ApiController]
    [Route("street")]
    public class StreetController : Controller
    {
        private readonly IStreetRepository _streetRepository;
        private readonly ILogger<StreetController> _logger;
        private readonly IMapper _mapper;

        public StreetController(IStreetRepository streetRepository, ILogger<StreetController> logger, IMapper mapper)
        {
            _streetRepository = streetRepository ?? throw new ArgumentNullException(nameof(streetRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateStreet([FromBody] CreateStreetRequestDto request)
        {
            if (request is null)
            {
                return BadRequest("Street request is null");
            }

            var street = _mapper.Map<Street>(request);

            var createdStreet = await _streetRepository.CreateStreetAsync(street);

            var response = _mapper.Map<CreateStreetResponseDto>(createdStreet);

            return Ok(response);
        }

        [HttpDelete("delete/{id:int}")]
        public async Task<IActionResult> DeleteStreet(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("Invalid street ID for deletion: {StreetId}", id);
                return BadRequest("Street ID must be greater than zero.");
            }

            await _streetRepository.DeleteStreetAsync(id);
            _logger.LogInformation("Street with ID {StreetId} successfully deleted.", id);

            return NoContent();
        }

        [HttpPost("{streetId:int}/add-point")]
        public async Task<IActionResult> AddPointToStreet(int streetId, [FromBody] AddPointRequestDto request)
        {
            if (request is null)
            {
                _logger.LogWarning("Invalid model state for AddPointToStreet: {@Request}", request);
                return BadRequest("Point request is null");
            }

            var newPoint = new Coordinate(request.Latitude, request.Longitude);

            await _streetRepository.AddPointToStreetAsync(streetId, newPoint, request.AddToEnd);
            _logger.LogInformation("Point added to street with ID {StreetId}.", streetId);

            return NoContent();
        }
    }
}
