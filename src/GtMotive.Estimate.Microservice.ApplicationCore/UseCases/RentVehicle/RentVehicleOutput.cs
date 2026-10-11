using GtMotive.Estimate.Microservice.Domain.Rentals;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RentVehicle
{
    /// <summary>
    /// Output message of the rent vehicle use case.
    /// </summary>
    /// <param name="rental">Created rental.</param>
    public sealed class RentVehicleOutput(Rental rental) : IUseCaseOutput
    {
        /// <summary>
        /// Gets the created rental.
        /// </summary>
        public Rental Rental { get; } = rental;
    }
}
