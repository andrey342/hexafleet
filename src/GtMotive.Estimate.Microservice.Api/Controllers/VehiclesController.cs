using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Models;
using GtMotive.Estimate.Microservice.Api.UseCases.CreateVehicle;
using GtMotive.Estimate.Microservice.Api.UseCases.GetVehicle;
using GtMotive.Estimate.Microservice.Api.UseCases.ListAvailableVehicles;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.Controllers
{
    [ApiController]
    [Route("api/vehicles")]
    [Produces("application/json")]
    public sealed class VehiclesController(IMediator mediator, IAppLogger<VehiclesController> appLogger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly IAppLogger<VehiclesController> _appLogger = appLogger;

        /// <summary>
        /// Adds a vehicle to the fleet.
        /// </summary>
        /// <param name="request">Vehicle data.</param>
        /// <returns>The created vehicle.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Post([FromBody][Required] CreateVehicleRequest request)
        {
            _appLogger.LogDebug("Entering Post. Request: {@request}", request);

            var presenter = await _mediator.Send(request);
            return presenter.ActionResult;
        }

        /// <summary>
        /// Lists the vehicles that can be rented: not rented and within the fleet age limit.
        /// </summary>
        /// <returns>The available vehicles.</returns>
        [HttpGet("available")]
        [ProducesResponseType(typeof(IReadOnlyCollection<VehicleResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailable()
        {
            _appLogger.LogDebug("Entering GetAvailable.");

            var presenter = await _mediator.Send(new ListAvailableVehiclesRequest());
            return presenter.ActionResult;
        }

        /// <summary>
        /// Gets a vehicle of the fleet.
        /// </summary>
        /// <param name="id">Vehicle ID.</param>
        /// <returns>The vehicle.</returns>
        [HttpGet("{id:guid}", Name = "GetVehicle")]
        [ProducesResponseType(typeof(VehicleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(Guid id)
        {
            _appLogger.LogDebug("Entering Get. Id: {id}", id);

            var presenter = await _mediator.Send(new GetVehicleRequest(id));
            return presenter.ActionResult;
        }
    }
}
