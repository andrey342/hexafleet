using System;
using GtMotive.Estimate.Microservice.Api.Models;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.GetVehicle;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.UseCases.GetVehicle
{
    public sealed class GetVehiclePresenter : IWebApiPresenter, IGetVehicleOutputPort
    {
        public IActionResult ActionResult { get; private set; }

        public void StandardHandle(GetVehicleOutput response)
        {
            ArgumentNullException.ThrowIfNull(response);

            ActionResult = new OkObjectResult(new VehicleResponse(response.Vehicle));
        }

        public void NotFoundHandle(string message)
        {
            ActionResult = ProblemResults.NotFound(message);
        }
    }
}
