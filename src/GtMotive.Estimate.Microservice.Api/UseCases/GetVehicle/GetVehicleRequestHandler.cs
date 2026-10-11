using System;
using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.GetVehicle;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using MediatR;

namespace GtMotive.Estimate.Microservice.Api.UseCases.GetVehicle
{
    public sealed class GetVehicleRequestHandler(IGetVehicleUseCase useCase, GetVehiclePresenter presenter)
        : IRequestHandler<GetVehicleRequest, IWebApiPresenter>
    {
        private readonly IGetVehicleUseCase _useCase = useCase;
        private readonly GetVehiclePresenter _presenter = presenter;

        public async Task<IWebApiPresenter> Handle(GetVehicleRequest request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            await _useCase.Execute(new GetVehicleInput(VehicleId.From(request.Id)));

            return _presenter;
        }
    }
}
