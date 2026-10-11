using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.Domain.Rentals;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ReturnVehicle
{
    /// <summary>
    /// Closes the rental of a vehicle.
    /// </summary>
    /// <param name="outputPort">Output port.</param>
    /// <param name="rentalRepository">Rental repository.</param>
    /// <param name="unitOfWork">Unit of work.</param>
    /// <param name="timeProvider">Clock.</param>
    public sealed class ReturnVehicleUseCase(
        IReturnVehicleOutputPort outputPort,
        IRentalRepository rentalRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider) : IReturnVehicleUseCase
    {
        private readonly IReturnVehicleOutputPort _outputPort = outputPort;
        private readonly IRentalRepository _rentalRepository = rentalRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly TimeProvider _timeProvider = timeProvider;

        /// <inheritdoc />
        public async Task Execute(ReturnVehicleInput input)
        {
            ArgumentNullException.ThrowIfNull(input);

            var rental = await _rentalRepository.GetById(input.RentalId);

            if (rental is null)
            {
                _outputPort.NotFoundHandle($"The rental {input.RentalId} does not exist.");
                return;
            }

            try
            {
                rental.Return(_timeProvider.GetUtcNow().UtcDateTime);

                await _rentalRepository.Update(rental);
                await _unitOfWork.Save();

                _outputPort.StandardHandle(new ReturnVehicleOutput(rental));
            }
            catch (RentalAlreadyReturnedException returnedEx)
            {
                _outputPort.ConflictHandle(returnedEx.Message);
            }
        }
    }
}
