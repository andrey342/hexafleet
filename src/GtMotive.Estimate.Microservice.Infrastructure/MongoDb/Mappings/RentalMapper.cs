using System;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Documents;

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Mappings
{
    public static class RentalMapper
    {
        public static RentalDocument ToDocument(Rental rental)
        {
            ArgumentNullException.ThrowIfNull(rental);

            return new RentalDocument
            {
                Id = rental.Id.ToGuid(),
                VehicleId = rental.VehicleId.ToGuid(),
                CustomerId = rental.CustomerId.ToString(),
                StartedAt = rental.StartedAt,
                ReturnedAt = rental.ReturnedAt,
            };
        }

        public static Rental ToDomain(RentalDocument document)
        {
            ArgumentNullException.ThrowIfNull(document);

            return new Rental(
                RentalId.From(document.Id),
                VehicleId.From(document.VehicleId),
                new CustomerId(document.CustomerId),
                document.StartedAt,
                document.ReturnedAt);
        }
    }
}
