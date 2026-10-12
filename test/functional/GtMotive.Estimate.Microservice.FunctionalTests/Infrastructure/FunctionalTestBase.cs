using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api.UseCases;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using Xunit;

namespace GtMotive.Estimate.Microservice.FunctionalTests.Infrastructure
{
    [Collection(TestCollections.Functional)]
    public abstract class FunctionalTestBase(CompositionRootTestFixture fixture) : IAsyncLifetime
    {
        public const int QueueWaitingTimeInMilliseconds = 1000;

        protected CompositionRootTestFixture Fixture { get; } = fixture;

        public async Task InitializeAsync()
        {
            await Fixture.UsingRepository<MongoService>(async mongoService =>
            {
                foreach (var collection in new[] { CollectionNames.Vehicles, CollectionNames.Rentals })
                {
                    await mongoService.Database
                        .GetCollection<BsonDocument>(collection)
                        .DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
                }
            });
        }

        public async Task DisposeAsync()
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// Sends the request to its handler in a new scope, as the API does for each HTTP request.
        /// </summary>
        /// <typeparam name="TRequest">Request type.</typeparam>
        /// <param name="request">Request.</param>
        /// <returns>The action result built by the presenter.</returns>
        protected async Task<IActionResult> Send<TRequest>(TRequest request)
            where TRequest : IRequest<IWebApiPresenter>
        {
            IActionResult result = null;

            await Fixture.UsingHandlerForRequestResponse<TRequest, IWebApiPresenter>(async handler =>
            {
                var presenter = await handler.Handle(request, default);
                result = presenter.ActionResult;
            });

            return result;
        }
    }
}
