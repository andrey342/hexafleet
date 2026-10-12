using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.Models;
using GtMotive.Estimate.Microservice.Api.UseCases.RentVehicle;
using GtMotive.Estimate.Microservice.Api.UseCases.ReturnVehicle;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.Controllers
{
    [ApiController]
    [Route("api/rentals")]
    [Produces("application/json")]
    public sealed class RentalsController(IMediator mediator, IAppLogger<RentalsController> appLogger) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly IAppLogger<RentalsController> _appLogger = appLogger;

        /// <summary>
        /// Rents a vehicle to a customer. A customer can have only one active rental.
        /// </summary>
        /// <param name="request">Vehicle and customer.</param>
        /// <returns>The created rental.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Post([FromBody][Required] RentVehicleRequest request)
        {
            _appLogger.LogDebug("Entering Post. Request: {@request}", request);

            var presenter = await _mediator.Send(request);
            return presenter.ActionResult;
        }

        /// <summary>
        /// Returns the rented vehicle and closes the rental.
        /// </summary>
        /// <param name="rentalId">Rental ID.</param>
        /// <returns>The closed rental.</returns>
        [HttpPost("{rentalId:guid}/return")]
        [ProducesResponseType(typeof(RentalResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Return(Guid rentalId)
        {
            _appLogger.LogDebug("Entering Return. RentalId: {rentalId}", rentalId);

            var presenter = await _mediator.Send(new ReturnVehicleRequest(rentalId));
            return presenter.ActionResult;
        }
    }
}
