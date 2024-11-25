using Application.Interfaces.Repository;
using Application.Interfaces.Service;
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
        private readonly IStreetOperationService _streetOperationService;

        public StreetController(IStreetRepository streetRepository, ILogger<StreetController> logger, IMapper mapper, IStreetOperationService streetOperationService)
        {
            _streetRepository = streetRepository;
            _logger = logger;
            _mapper = mapper;
            _streetOperationService = streetOperationService;
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
                return BadRequest("Request cannot be null");
            }

            var newPoint = new Coordinate(request.Latitude, request.Longitude);

            await _streetOperationService.AddPointToStreetAsync(streetId, newPoint, request.AddToEnd);
            _logger.LogInformation("Point added to street with ID {StreetId}.", streetId);

            return NoContent();
        }
    }
}
