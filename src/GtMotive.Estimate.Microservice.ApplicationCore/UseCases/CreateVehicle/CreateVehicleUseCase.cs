using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.CreateVehicle
{
    /// <summary>
    /// Adds a vehicle to the fleet.
    /// </summary>
    /// <param name="outputPort">Output port.</param>
    /// <param name="vehicleRepository">Vehicle repository.</param>
    /// <param name="vehicleFactory">Vehicle factory.</param>
    /// <param name="unitOfWork">Unit of work.</param>
    /// <param name="timeProvider">Clock.</param>
    public sealed class CreateVehicleUseCase(
        ICreateVehicleOutputPort outputPort,
        IVehicleRepository vehicleRepository,
        IVehicleFactory vehicleFactory,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider) : ICreateVehicleUseCase
    {
        private readonly ICreateVehicleOutputPort _outputPort = outputPort;
        private readonly IVehicleRepository _vehicleRepository = vehicleRepository;
        private readonly IVehicleFactory _vehicleFactory = vehicleFactory;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly TimeProvider _timeProvider = timeProvider;

        /// <inheritdoc />
        public async Task Execute(CreateVehicleInput input)
        {
            ArgumentNullException.ThrowIfNull(input);

            if (await _vehicleRepository.ExistsByLicensePlate(input.LicensePlate))
            {
                _outputPort.ConflictHandle($"The license plate {input.LicensePlate} is already registered.");
                return;
            }

            try
            {
                var vehicle = _vehicleFactory.NewVehicle(
                    input.LicensePlate,
                    input.Brand,
                    input.Model,
                    input.ManufacturingDate,
                    _timeProvider.GetUtcNow().UtcDateTime);

                await _vehicleRepository.Add(vehicle);
                await _unitOfWork.Save();

                _outputPort.StandardHandle(new CreateVehicleOutput(vehicle));
            }
            catch (VehicleTooOldException tooOldEx)
            {
                _outputPort.UnprocessableHandle(tooOldEx.Message);
            }
            catch (LicensePlateAlreadyRegisteredException duplicatedEx)
            {
                _outputPort.ConflictHandle(duplicatedEx.Message);
            }
        }
    }
}
