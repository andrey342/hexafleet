using System;
using MediatR;

namespace GtMotive.Estimate.Microservice.Api.UseCases.ReturnVehicle
{
    public sealed class ReturnVehicleRequest(Guid rentalId) : IRequest<IWebApiPresenter>
    {
        public Guid RentalId { get; } = rentalId;
    }
}
