using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Documents;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Mappings;
using MongoDB.Driver;

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Repositories
{
    public sealed class RentalRepository(MongoService mongoService) : IRentalRepository
    {
        private readonly IMongoCollection<RentalDocument> _rentals = mongoService.Database.GetCollection<RentalDocument>(CollectionNames.Rentals);

        public async Task Add(Rental rental)
        {
            await _rentals.InsertOneAsync(RentalMapper.ToDocument(rental));
        }

        public async Task<Rental> GetById(RentalId rentalId)
        {
            var id = rentalId.ToGuid();

            return await FindOne(r => r.Id == id);
        }

        public async Task<Rental> GetActiveByCustomer(CustomerId customerId)
        {
            var value = customerId.ToString();

            return await FindOne(r => r.CustomerId == value && r.ReturnedAt == null);
        }

        public async Task<Rental> GetActiveByVehicle(VehicleId vehicleId)
        {
            var id = vehicleId.ToGuid();

            return await FindOne(r => r.VehicleId == id && r.ReturnedAt == null);
        }

        public async Task<IReadOnlyCollection<VehicleId>> GetActiveVehicleIds()
        {
            var ids = await _rentals.Find(r => r.ReturnedAt == null)
                .Project(r => r.VehicleId)
                .ToListAsync();

            return ids.ConvertAll(VehicleId.From);
        }

        public async Task Update(Rental rental)
        {
            ArgumentNullException.ThrowIfNull(rental);

            var id = rental.Id.ToGuid();

            // Only an active rental can change, so a concurrent return makes this match nothing.
            var result = await _rentals.ReplaceOneAsync(r => r.Id == id && r.ReturnedAt == null, RentalMapper.ToDocument(rental));

            if (result.MatchedCount == 0)
            {
                throw new RentalAlreadyReturnedException($"The rental {rental.Id} has already been returned.");
            }
        }

        private async Task<Rental> FindOne(Expression<Func<RentalDocument, bool>> filter)
        {
            var document = await _rentals.Find(filter).FirstOrDefaultAsync();

            return document is null ? null : RentalMapper.ToDomain(document);
        }
    }
}
