using System;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;

namespace GtMotive.Estimate.Microservice.Infrastructure
{
    public sealed class EntityFactory : IVehicleFactory, IRentalFactory
    {
        public Vehicle NewVehicle(
            LicensePlate licensePlate,
            string brand,
            string model,
            ManufacturingDate manufacturingDate,
            DateTime now)
        {
            return Vehicle.Create(VehicleId.New(), licensePlate, brand, model, manufacturingDate, now);
        }

        public Rental NewRental(VehicleId vehicleId, CustomerId customerId, DateTime now)
        {
            return new Rental(RentalId.New(), vehicleId, customerId, now, null);
        }
    }
}
