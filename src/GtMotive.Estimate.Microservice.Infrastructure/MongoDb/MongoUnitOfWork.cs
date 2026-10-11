using System;
using System.Threading.Tasks;
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
        private readonly MongoService _mongoService = mongoService;
        private IClientSessionHandle _session;
        private int _pendingWrites;
        private bool _disposed;

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

            await _session.CommitTransactionAsync();

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
