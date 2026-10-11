using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.CreateVehicle
{
    /// <summary>
    /// Output message of the create vehicle use case.
    /// </summary>
    /// <param name="vehicle">Created vehicle.</param>
    public sealed class CreateVehicleOutput(Vehicle vehicle) : IUseCaseOutput
    {
        /// <summary>
        /// Gets the created vehicle.
        /// </summary>
        public Vehicle Vehicle { get; } = vehicle;
    }
}
