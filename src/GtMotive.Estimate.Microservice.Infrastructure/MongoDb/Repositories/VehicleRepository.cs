using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Documents;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Mappings;
using MongoDB.Driver;

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Repositories
{
    public sealed class VehicleRepository(MongoService mongoService) : IVehicleRepository
    {
        private readonly IMongoCollection<VehicleDocument> _vehicles = mongoService.Database.GetCollection<VehicleDocument>(CollectionNames.Vehicles);

        public async Task Add(Vehicle vehicle)
        {
            await _vehicles.InsertOneAsync(VehicleMapper.ToDocument(vehicle));
        }

        public async Task<Vehicle> GetById(VehicleId vehicleId)
        {
            var id = vehicleId.ToGuid();
            var document = await _vehicles.Find(v => v.Id == id).FirstOrDefaultAsync();

            return document is null ? null : VehicleMapper.ToDomain(document);
        }

        public async Task<bool> ExistsByLicensePlate(LicensePlate licensePlate)
        {
            var value = licensePlate.ToString();

            return await _vehicles.Find(v => v.LicensePlate == value).AnyAsync();
        }

        public async Task<IReadOnlyCollection<Vehicle>> FindAvailable(
            DateTime oldestAllowedManufacturingDate,
            IReadOnlyCollection<VehicleId> rentedVehicleIds)
        {
            ArgumentNullException.ThrowIfNull(rentedVehicleIds);

            var rentedIds = rentedVehicleIds.Select(id => id.ToGuid()).ToList();
            var filter = Builders<VehicleDocument>.Filter.Gte(v => v.ManufacturingDate, oldestAllowedManufacturingDate)
                & Builders<VehicleDocument>.Filter.Nin(v => v.Id, rentedIds);

            var documents = await _vehicles.Find(filter).SortBy(v => v.LicensePlate).ToListAsync();

            return documents.ConvertAll(VehicleMapper.ToDomain);
        }
    }
}
