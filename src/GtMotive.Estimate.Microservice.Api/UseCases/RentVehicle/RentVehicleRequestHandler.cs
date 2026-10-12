using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RentVehicle;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using MediatR;

namespace GtMotive.Estimate.Microservice.Api.UseCases.RentVehicle
{
    public sealed class RentVehicleRequestHandler(IRentVehicleUseCase useCase, RentVehiclePresenter presenter)
        : IRequestHandler<RentVehicleRequest, IWebApiPresenter>
    {
        private readonly IRentVehicleUseCase _useCase = useCase;
        private readonly RentVehiclePresenter _presenter = presenter;

        public async Task<IWebApiPresenter> Handle(RentVehicleRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var input = new RentVehicleInput(
                VehicleId.From(request.VehicleId.GetValueOrDefault()),
                new CustomerId(request.CustomerId));

            await _useCase.Execute(input);

            return _presenter;
        }
    }
}
