using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Documents;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Indexes;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Mappings;
using MongoDB.Driver;

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Repositories
{
    public sealed class RentalRepository(MongoService mongoService, MongoUnitOfWork unitOfWork) : IRentalRepository
    {
        private readonly IMongoCollection<RentalDocument> _rentals = mongoService.Database.GetCollection<RentalDocument>(CollectionNames.Rentals);
        private readonly MongoUnitOfWork _unitOfWork = unitOfWork;

        public async Task Add(Rental rental)
        {
            ArgumentNullException.ThrowIfNull(rental);

            var session = await _unitOfWork.GetSessionForWrite();

            try
            {
                await _rentals.InsertOneAsync(session, RentalMapper.ToDocument(rental));
            }
            catch (MongoWriteException ex) when (MongoIndexes.IsDuplicateKey(ex, MongoIndexes.ActiveRentalByCustomer))
            {
                throw new CustomerAlreadyHasRentalException($"The customer {rental.CustomerId} already has an active rental.", ex);
            }
            catch (MongoWriteException ex) when (MongoIndexes.IsDuplicateKey(ex, MongoIndexes.ActiveRentalByVehicle))
            {
                throw new VehicleNotAvailableException($"The vehicle {rental.VehicleId} is already rented.", ex);
            }
            catch (MongoException ex) when (MongoUnitOfWork.IsWriteConflict(ex))
            {
                throw new ConcurrencyConflictException($"The vehicle {rental.VehicleId} or the customer {rental.CustomerId} is being rented by another request. Retry the request.", ex);
            }
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
            var session = await _unitOfWork.GetSessionForWrite();

            // Only an active rental can change: a return committed earlier makes this match nothing,
            // one still in progress makes MongoDB report a write conflict.
            ReplaceOneResult result;

            try
            {
                result = await _rentals.ReplaceOneAsync(session, r => r.Id == id && r.ReturnedAt == null, RentalMapper.ToDocument(rental));
            }
            catch (MongoException ex) when (MongoUnitOfWork.IsWriteConflict(ex))
            {
                throw new ConcurrencyConflictException($"The rental {rental.Id} is being returned by another request. Retry the request.", ex);
            }

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
