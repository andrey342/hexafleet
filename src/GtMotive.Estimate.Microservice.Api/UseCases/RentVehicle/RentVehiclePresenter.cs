using System;
using GtMotive.Estimate.Microservice.Api.Models;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RentVehicle;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.UseCases.RentVehicle
{
    public sealed class RentVehiclePresenter : IWebApiPresenter, IRentVehicleOutputPort
    {
        public IActionResult ActionResult { get; private set; }

        public void StandardHandle(RentVehicleOutput response)
        {
            ArgumentNullException.ThrowIfNull(response);

            ActionResult = new ObjectResult(new RentalResponse(response.Rental))
            {
                StatusCode = StatusCodes.Status201Created,
            };
        }

        public void NotFoundHandle(string message)
        {
            ActionResult = ProblemResults.NotFound(message);
        }

        public void ConflictHandle(string message)
        {
            ActionResult = ProblemResults.Conflict(message);
        }

        public void UnprocessableHandle(string message)
        {
            ActionResult = ProblemResults.UnprocessableEntity(message);
        }
    }
}
