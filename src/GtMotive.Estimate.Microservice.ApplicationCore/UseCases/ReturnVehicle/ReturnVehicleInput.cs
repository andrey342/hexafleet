using GtMotive.Estimate.Microservice.Domain.Rentals;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.ReturnVehicle
{
    /// <summary>
    /// Input message of the return vehicle use case.
    /// </summary>
    /// <param name="rentalId">Rental to close.</param>
    public sealed class ReturnVehicleInput(RentalId rentalId) : IUseCaseInput
    {
        /// <summary>
        /// Gets the rental to close.
        /// </summary>
        public RentalId RentalId { get; } = rentalId;
    }
}
