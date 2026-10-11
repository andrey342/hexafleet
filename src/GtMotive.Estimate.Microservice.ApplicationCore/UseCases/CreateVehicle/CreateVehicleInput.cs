using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.CreateVehicle
{
    /// <summary>
    /// Input message of the create vehicle use case.
    /// </summary>
    /// <param name="licensePlate">License plate.</param>
    /// <param name="brand">Brand.</param>
    /// <param name="model">Model.</param>
    /// <param name="manufacturingDate">Manufacturing date.</param>
    public sealed class CreateVehicleInput(LicensePlate licensePlate, string brand, string model, ManufacturingDate manufacturingDate) : IUseCaseInput
    {
        /// <summary>
        /// Gets the license plate.
        /// </summary>
        public LicensePlate LicensePlate { get; } = licensePlate;

        /// <summary>
        /// Gets the brand.
        /// </summary>
        public string Brand { get; } = brand;

        /// <summary>
        /// Gets the model.
        /// </summary>
        public string Model { get; } = model;

        /// <summary>
        /// Gets the manufacturing date.
        /// </summary>
        public ManufacturingDate ManufacturingDate { get; } = manufacturingDate;
    }
}
