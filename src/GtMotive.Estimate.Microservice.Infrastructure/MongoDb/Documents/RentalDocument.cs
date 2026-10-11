using System;

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Documents
{
    public sealed class RentalDocument
    {
        public Guid Id { get; set; }

        public Guid VehicleId { get; set; }

        public string CustomerId { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? ReturnedAt { get; set; }
    }
}
