using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RentVehicle
{
    /// <summary>
    /// Input message of the rent vehicle use case.
    /// </summary>
    /// <param name="vehicleId">Vehicle to rent.</param>
    /// <param name="customerId">Customer who rents the vehicle.</param>
    public sealed class RentVehicleInput(VehicleId vehicleId, CustomerId customerId) : IUseCaseInput
    {
        /// <summary>
        /// Gets the vehicle to rent.
        /// </summary>
        public VehicleId VehicleId { get; } = vehicleId;

        /// <summary>
        /// Gets the customer who rents the vehicle.
        /// </summary>
        public CustomerId CustomerId { get; } = customerId;
    }
}
