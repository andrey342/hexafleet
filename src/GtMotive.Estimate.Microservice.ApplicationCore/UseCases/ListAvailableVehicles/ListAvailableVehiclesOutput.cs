using System.Collections.Generic;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ListAvailableVehicles
{
    /// <summary>
    /// Output message of the list available vehicles use case.
    /// </summary>
    /// <param name="vehicles">Available vehicles.</param>
    public sealed class ListAvailableVehiclesOutput(IReadOnlyCollection<Vehicle> vehicles) : IUseCaseOutput
    {
        /// <summary>
        /// Gets the vehicles that are not rented and are within the fleet age limit.
        /// </summary>
        public IReadOnlyCollection<Vehicle> Vehicles { get; } = vehicles;
    }
}
