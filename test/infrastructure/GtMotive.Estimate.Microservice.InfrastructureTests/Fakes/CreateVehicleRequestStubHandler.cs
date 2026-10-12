using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.UseCases;
using GtMotive.Estimate.Microservice.Api.UseCases.CreateVehicle;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.CreateVehicle;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using MediatR;

namespace GtMotive.Estimate.Microservice.InfrastructureTests.Fakes
{
    /// <summary>
    /// Replaces the real handler so the host tests only cover routing, model binding and validation.
    /// </summary>
    public sealed class CreateVehicleRequestStubHandler : IRequestHandler<CreateVehicleRequest, IWebApiPresenter>
    {
        public static Guid CreatedVehicleId { get; } = new("6f1c2a1e-6a49-4a4f-9a52-2d4c8f6b1a10");

        public Task<IWebApiPresenter> Handle(CreateVehicleRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var vehicle = new Vehicle(
                VehicleId.From(CreatedVehicleId),
                new LicensePlate(request.LicensePlate),
                request.Brand,
                request.Model,
                new ManufacturingDate(request.ManufacturingDate.GetValueOrDefault().ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)));

            var presenter = new CreateVehiclePresenter();
            presenter.StandardHandle(new CreateVehicleOutput(vehicle));

            return Task.FromResult<IWebApiPresenter>(presenter);
        }
    }
}
