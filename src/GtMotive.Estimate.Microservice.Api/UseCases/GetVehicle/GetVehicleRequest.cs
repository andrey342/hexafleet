using System;
using MediatR;

namespace GtMotive.Estimate.Microservice.Api.UseCases.GetVehicle
{
    public sealed class GetVehicleRequest(Guid id) : IRequest<IWebApiPresenter>
    {
        public Guid Id { get; } = id;
    }
}
