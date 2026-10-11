using System;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Documents;
using MongoDB.Driver;

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Indexes
{
    public static class MongoIndexes
    {
        public const string VehicleLicensePlate = "ux_vehicles_license_plate";

        public const string ActiveRentalByCustomer = "ux_rentals_active_customer";

        public const string ActiveRentalByVehicle = "ux_rentals_active_vehicle";

        /// <summary>
        /// Creates the indexes. Creating an index that already exists with the same definition does nothing.
        /// </summary>
        /// <param name="database">Database.</param>
        public static void Create(IMongoDatabase database)
        {
            ArgumentNullException.ThrowIfNull(database);

            var vehicles = database.GetCollection<VehicleDocument>(CollectionNames.Vehicles);
            vehicles.Indexes.CreateOne(new CreateIndexModel<VehicleDocument>(
                Builders<VehicleDocument>.IndexKeys.Ascending(v => v.LicensePlate),
                new CreateIndexOptions { Name = VehicleLicensePlate, Unique = true }));

            // One active rental per customer and per vehicle; returned rentals are left out of the index.
            var rentals = database.GetCollection<RentalDocument>(CollectionNames.Rentals);
            var active = Builders<RentalDocument>.Filter.Eq(r => r.ReturnedAt, null);

            rentals.Indexes.CreateOne(new CreateIndexModel<RentalDocument>(
                Builders<RentalDocument>.IndexKeys.Ascending(r => r.CustomerId),
                new CreateIndexOptions<RentalDocument> { Name = ActiveRentalByCustomer, Unique = true, PartialFilterExpression = active }));

            rentals.Indexes.CreateOne(new CreateIndexModel<RentalDocument>(
                Builders<RentalDocument>.IndexKeys.Ascending(r => r.VehicleId),
                new CreateIndexOptions<RentalDocument> { Name = ActiveRentalByVehicle, Unique = true, PartialFilterExpression = active }));
        }

        /// <summary>
        /// Checks whether a write failed because of a duplicate key in the given index.
        /// </summary>
        /// <param name="exception">Write exception.</param>
        /// <param name="indexName">Index name.</param>
        /// <returns>True when the duplicate key belongs to the index.</returns>
        public static bool IsDuplicateKey(MongoWriteException exception, string indexName)
        {
            ArgumentNullException.ThrowIfNull(exception);

            return exception.WriteError?.Category == ServerErrorCategory.DuplicateKey
                && exception.WriteError.Message.Contains(indexName, StringComparison.Ordinal);
        }
    }
}
