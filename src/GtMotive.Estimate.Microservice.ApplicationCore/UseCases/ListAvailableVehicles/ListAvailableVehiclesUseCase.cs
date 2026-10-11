using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Fleet;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ListAvailableVehicles
{
    /// <summary>
    /// Lists the vehicles that are not rented and are within the fleet age limit.
    /// </summary>
    /// <param name="outputPort">Output port.</param>
    /// <param name="vehicleRepository">Vehicle repository.</param>
    /// <param name="rentalRepository">Rental repository.</param>
    /// <param name="timeProvider">Clock.</param>
    public sealed class ListAvailableVehiclesUseCase(
        IListAvailableVehiclesOutputPort outputPort,
        IVehicleRepository vehicleRepository,
        IRentalRepository rentalRepository,
        TimeProvider timeProvider) : IListAvailableVehiclesUseCase
    {
        private readonly IListAvailableVehiclesOutputPort _outputPort = outputPort;
        private readonly IVehicleRepository _vehicleRepository = vehicleRepository;
        private readonly IRentalRepository _rentalRepository = rentalRepository;
        private readonly TimeProvider _timeProvider = timeProvider;

        /// <inheritdoc />
        public async Task Execute(ListAvailableVehiclesInput input)
        {
            ArgumentNullException.ThrowIfNull(input);

            var oldestAllowed = FleetPolicy.OldestAllowedManufacturingDate(_timeProvider.GetUtcNow().UtcDateTime);
            var rentedVehicleIds = await _rentalRepository.GetActiveVehicleIds();
            var vehicles = await _vehicleRepository.FindAvailable(oldestAllowed, rentedVehicleIds);

            _outputPort.StandardHandle(new ListAvailableVehiclesOutput(vehicles));
        }
    }
}
