using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.GetVehicle
{
    /// <summary>
    /// Input message of the get vehicle use case.
    /// </summary>
    /// <param name="vehicleId">Vehicle identifier.</param>
    public sealed class GetVehicleInput(VehicleId vehicleId) : IUseCaseInput
    {
        /// <summary>
        /// Gets the vehicle identifier.
        /// </summary>
        public VehicleId VehicleId { get; } = vehicleId;
    }
}
