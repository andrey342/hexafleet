using System.Threading;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ListAvailableVehicles;
using MediatR;

namespace GtMotive.Estimate.Microservice.Api.UseCases.ListAvailableVehicles
{
    public sealed class ListAvailableVehiclesRequestHandler(IListAvailableVehiclesUseCase useCase, ListAvailableVehiclesPresenter presenter)
        : IRequestHandler<ListAvailableVehiclesRequest, IWebApiPresenter>
    {
        private readonly IListAvailableVehiclesUseCase _useCase = useCase;
        private readonly ListAvailableVehiclesPresenter _presenter = presenter;

        public async Task<IWebApiPresenter> Handle(ListAvailableVehiclesRequest request, CancellationToken cancellationToken)
        {
            await _useCase.Execute(new ListAvailableVehiclesInput());

            return _presenter;
        }
    }
}
