using System;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Documents;

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Mappings
{
    public static class VehicleMapper
    {
        public static VehicleDocument ToDocument(Vehicle vehicle)
        {
            ArgumentNullException.ThrowIfNull(vehicle);

            return new VehicleDocument
            {
                Id = vehicle.Id.ToGuid(),
                LicensePlate = vehicle.LicensePlate.ToString(),
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                ManufacturingDate = vehicle.ManufacturingDate.ToDateTime(),
            };
        }

        public static Vehicle ToDomain(VehicleDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            return new Vehicle(
                VehicleId.From(document.Id),
                new LicensePlate(document.LicensePlate),
                document.Brand,
                document.Model,
                new ManufacturingDate(document.ManufacturingDate));
        }
    }
}
