using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using MongoDB.Driver;

namespace GtMotive.Estimate.Microservice.Infrastructure.MongoDb
{
    /// <summary>
    /// Groups the writes of a scope in a MongoDB transaction. The transaction starts with the first write,
    /// <see cref="Save"/> commits it and disposing the scope without saving aborts it.
    /// </summary>
    /// <param name="mongoService">Mongo service.</param>
    public sealed class MongoUnitOfWork(MongoService mongoService) : IUnitOfWork, IDisposable
    {
        private const int WriteConflictCode = 112;

        private readonly MongoService _mongoService = mongoService;
        private IClientSessionHandle _session;
        private int _pendingWrites;
        private bool _disposed;

        /// <summary>
        /// Checks whether MongoDB aborted the transaction because another transaction wrote the same key or document.
        /// </summary>
        /// <param name="exception">MongoDB exception.</param>
        /// <returns>True for a write conflict.</returns>
        public static bool IsWriteConflict(MongoException exception)
        {
            return exception switch
            {
                MongoCommandException command => command.Code == WriteConflictCode,
                MongoWriteException write => write.WriteError?.Code == WriteConflictCode,
                _ => false,
            };
        }

        public async Task<IClientSessionHandle> GetSessionForWrite()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            _session ??= await _mongoService.MongoClient.StartSessionAsync();

            if (!_session.IsInTransaction)
            {
                _session.StartTransaction();
            }

            _pendingWrites++;

            return _session;
        }

        public async Task<int> Save()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_session is null || !_session.IsInTransaction)
            {
                return 0;
            }

            try
            {
                await _session.CommitTransactionAsync();
            }
            catch (MongoException ex) when (IsWriteConflict(ex))
            {
                throw new ConcurrencyConflictException("The changes conflict with another request. Retry the request.", ex);
            }

            var writes = _pendingWrites;
            _pendingWrites = 0;

            return writes;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_session is not null)
            {
                if (_session.IsInTransaction)
                {
                    try
                    {
                        _session.AbortTransaction();
                    }
                    catch (MongoException)
                    {
                        // The server may have aborted the transaction already after a failed write.
                    }
                }

                _session.Dispose();
            }

            _disposed = true;
        }
    }
}
