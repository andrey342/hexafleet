using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RentVehicle
{
    /// <summary>
    /// Rents a vehicle to a customer.
    /// </summary>
    /// <param name="outputPort">Output port.</param>
    /// <param name="vehicleRepository">Vehicle repository.</param>
    /// <param name="rentalService">Rental domain service.</param>
    /// <param name="unitOfWork">Unit of work.</param>
    /// <param name="timeProvider">Clock.</param>
    public sealed class RentVehicleUseCase(
        IRentVehicleOutputPort outputPort,
        IVehicleRepository vehicleRepository,
        RentalService rentalService,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider) : IRentVehicleUseCase
    {
        private readonly IRentVehicleOutputPort _outputPort = outputPort;
        private readonly IVehicleRepository _vehicleRepository = vehicleRepository;
        private readonly RentalService _rentalService = rentalService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly TimeProvider _timeProvider = timeProvider;

        /// <inheritdoc />
        public async Task Execute(RentVehicleInput input)
        {
            ArgumentNullException.ThrowIfNull(input);

            var vehicle = await _vehicleRepository.GetById(input.VehicleId);

            if (vehicle is null)
            {
                _outputPort.NotFoundHandle($"The vehicle {input.VehicleId} does not exist.");
                return;
            }

            try
            {
                var rental = await _rentalService.Rent(vehicle, input.CustomerId, _timeProvider.GetUtcNow().UtcDateTime);

                await _unitOfWork.Save();

                _outputPort.StandardHandle(new RentVehicleOutput(rental));
            }
            catch (VehicleTooOldException tooOldEx)
            {
                _outputPort.UnprocessableHandle(tooOldEx.Message);
            }
            catch (VehicleNotAvailableException notAvailableEx)
            {
                _outputPort.ConflictHandle(notAvailableEx.Message);
            }
            catch (CustomerAlreadyHasRentalException activeRentalEx)
            {
                _outputPort.ConflictHandle(activeRentalEx.Message);
            }
        }
    }
}
