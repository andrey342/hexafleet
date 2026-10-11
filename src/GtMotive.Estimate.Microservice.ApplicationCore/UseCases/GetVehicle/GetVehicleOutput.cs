using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.GetVehicle
{
    /// <summary>
    /// Output message of the get vehicle use case.
    /// </summary>
    /// <param name="vehicle">Requested vehicle.</param>
    public sealed class GetVehicleOutput(Vehicle vehicle) : IUseCaseOutput
    {
        /// <summary>
        /// Gets the requested vehicle.
        /// </summary>
        public Vehicle Vehicle { get; } = vehicle;
    }
}
