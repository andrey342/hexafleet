using System;
using GtMotive.Estimate.Microservice.Api.Models;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ReturnVehicle;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.UseCases.ReturnVehicle
{
    public sealed class ReturnVehiclePresenter : IWebApiPresenter, IReturnVehicleOutputPort
    {
        public IActionResult ActionResult { get; private set; }

        public void StandardHandle(ReturnVehicleOutput response)
        {
            ArgumentNullException.ThrowIfNull(response);

            ActionResult = new OkObjectResult(new RentalResponse(response.Rental));
        }

        public void NotFoundHandle(string message)
        {
            ActionResult = ProblemResults.NotFound(message);
        }

        public void ConflictHandle(string message)
        {
            ActionResult = ProblemResults.Conflict(message);
        }
    }
}
