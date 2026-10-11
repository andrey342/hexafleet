using System;
using GtMotive.Estimate.Microservice.Api.Models;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.CreateVehicle;
using Microsoft.AspNetCore.Mvc;

namespace GtMotive.Estimate.Microservice.Api.UseCases.CreateVehicle
{
    public sealed class CreateVehiclePresenter : IWebApiPresenter, ICreateVehicleOutputPort
    {
        public IActionResult ActionResult { get; private set; }

        public void StandardHandle(CreateVehicleOutput response)
        {
            ArgumentNullException.ThrowIfNull(response);

            var vehicle = new VehicleResponse(response.Vehicle);

            ActionResult = new CreatedAtRouteResult(
                "GetVehicle",
                new
                {
                    id = vehicle.Id,
                },
                vehicle);
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
