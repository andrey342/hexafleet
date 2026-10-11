using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.CreateVehicle;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using MediatR;

namespace GtMotive.Estimate.Microservice.Api.UseCases.CreateVehicle
{
    public sealed class CreateVehicleRequestHandler(ICreateVehicleUseCase useCase, CreateVehiclePresenter presenter)
        : IRequestHandler<CreateVehicleRequest, IWebApiPresenter>
    {
        private readonly ICreateVehicleUseCase _useCase = useCase;
        private readonly CreateVehiclePresenter _presenter = presenter;

        public async Task<IWebApiPresenter> Handle(CreateVehicleRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var input = new CreateVehicleInput(
                new LicensePlate(request.LicensePlate),
                request.Brand,
                request.Model,
                new ManufacturingDate(request.ManufacturingDate.GetValueOrDefault().ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)));

            await _useCase.Execute(input);

            return _presenter;
        }
    }
}
