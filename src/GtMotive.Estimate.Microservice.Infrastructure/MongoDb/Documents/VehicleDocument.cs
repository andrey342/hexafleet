using System;

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Documents
{
    public sealed class VehicleDocument
    {
        public Guid Id { get; set; }

        public string LicensePlate { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public DateTime ManufacturingDate { get; set; }
    }
}
