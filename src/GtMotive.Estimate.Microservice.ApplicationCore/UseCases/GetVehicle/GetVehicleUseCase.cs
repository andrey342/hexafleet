using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.GetVehicle
{
    /// <summary>
    /// Gets a vehicle of the fleet.
    /// </summary>
    /// <param name="outputPort">Output port.</param>
    /// <param name="vehicleRepository">Vehicle repository.</param>
    public sealed class GetVehicleUseCase(
        IGetVehicleOutputPort outputPort,
        IVehicleRepository vehicleRepository) : IGetVehicleUseCase
    {
        private readonly IGetVehicleOutputPort _outputPort = outputPort;
        private readonly IVehicleRepository _vehicleRepository = vehicleRepository;

        /// <inheritdoc />
        public async Task Execute(GetVehicleInput input)
        {
            ArgumentNullException.ThrowIfNull(input);

            var vehicle = await _vehicleRepository.GetById(input.VehicleId);

            if (vehicle is null)
            {
                _outputPort.NotFoundHandle($"The vehicle {input.VehicleId} does not exist.");
                return;
            }

            _outputPort.StandardHandle(new GetVehicleOutput(vehicle));
        }
    }
}
