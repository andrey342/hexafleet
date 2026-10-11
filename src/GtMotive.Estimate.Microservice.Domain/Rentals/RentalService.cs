using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Fleet;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.Domain.Rentals
{
    /// <summary>
    /// Rental rules that involve more than one aggregate.
    /// </summary>
    /// <param name="rentalFactory">Rental factory.</param>
    /// <param name="rentalRepository">Rental repository.</param>
    public sealed class RentalService(IRentalFactory rentalFactory, IRentalRepository rentalRepository)
    {
        private readonly IRentalFactory _rentalFactory = rentalFactory;
        private readonly IRentalRepository _rentalRepository = rentalRepository;

        /// <summary>
        /// Rents a vehicle to a customer. The change is not saved; the caller commits the unit of work.
        /// </summary>
        /// <param name="vehicle">Vehicle to rent.</param>
        /// <param name="customerId">Customer who rents the vehicle.</param>
        /// <param name="now">Current UTC date and time.</param>
        /// <returns>The new rental.</returns>
        /// <exception cref="VehicleTooOldException">The vehicle is older than the fleet allows.</exception>
        /// <exception cref="VehicleNotAvailableException">The vehicle already has an active rental.</exception>
        /// <exception cref="CustomerAlreadyHasRentalException">The customer already has an active rental.</exception>
        public async Task<Rental> Rent(Vehicle vehicle, CustomerId customerId, DateTime now)
        {
            ArgumentNullException.ThrowIfNull(vehicle);

            FleetPolicy.EnsureAllowed(vehicle.ManufacturingDate, now);

            if (await _rentalRepository.GetActiveByVehicle(vehicle.Id) is not null)
            {
                throw new VehicleNotAvailableException($"The vehicle {vehicle.Id} is already rented.");
            }

            if (await _rentalRepository.GetActiveByCustomer(customerId) is not null)
            {
                throw new CustomerAlreadyHasRentalException($"The customer {customerId} already has an active rental.");
            }

            var rental = _rentalFactory.NewRental(vehicle.Id, customerId, now);
            await _rentalRepository.Add(rental);

            return rental;
        }
    }
}
