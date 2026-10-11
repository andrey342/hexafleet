using GtMotive.Estimate.Microservice.Domain.Rentals;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ReturnVehicle
{
    /// <summary>
    /// Output message of the return vehicle use case.
    /// </summary>
    /// <param name="rental">Closed rental.</param>
    public sealed class ReturnVehicleOutput(Rental rental) : IUseCaseOutput
    {
        /// <summary>
        /// Gets the closed rental.
        /// </summary>
        public Rental Rental { get; } = rental;
    }
}
