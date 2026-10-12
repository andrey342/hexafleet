using System;
using System.Diagnostics;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.Api;
using GtMotive.Estimate.Microservice.Infrastructure;
using GtMotive.Estimate.Microservice.Infrastructure.MongoDb.Settings;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MongoDb;
using Xunit;

[assembly: CLSCompliant(false)]

namespace GtMotive.Estimate.Microservice.FunctionalTests.Infrastructure
{
    public sealed class CompositionRootTestFixture : IDisposable, IAsyncLifetime
    {
        private MongoDbContainer _mongoDbContainer;
        private ServiceProvider _serviceProvider;

        public CompositionRootTestFixture()
        {
            Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        public IConfiguration Configuration { get; }

        public async Task InitializeAsync()
        {
            // MongoDb__ConnectionString points the tests to an existing server, for example the one in docker-compose.
            var connectionString = Configuration.GetValue<string>("MongoDb:ConnectionString");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                _mongoDbContainer = new MongoDbBuilder("mongo:7")
                    .WithReplicaSet()
                    .Build();

                await _mongoDbContainer.StartAsync();
                connectionString = _mongoDbContainer.GetConnectionString();
            }

            var services = new ServiceCollection();
            ConfigureServices(services, connectionString, Configuration.GetValue<string>("MongoDb:MongoDbDatabaseName"));
            services.AddSingleton(Configuration);
            _serviceProvider = services.BuildServiceProvider();
        }

        public async Task DisposeAsync()
        {
            if (_serviceProvider is not null)
            {
                await _serviceProvider.DisposeAsync();
            }

            if (_mongoDbContainer is not null)
            {
                await _mongoDbContainer.DisposeAsync();
            }
        }

        public async Task UsingHandlerForRequest<TRequest>(Func<IRequestHandler<TRequest, Unit>, Task> handlerAction)
            where TRequest : IRequest<Unit>
        {
            ArgumentNullException.ThrowIfNull(handlerAction);

            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TRequest, Unit>>();

            await handlerAction.Invoke(handler);
        }

        public async Task UsingHandlerForRequestResponse<TRequest, TResponse>(Func<IRequestHandler<TRequest, TResponse>, Task> handlerAction)
            where TRequest : IRequest<TResponse>
        {
            ArgumentNullException.ThrowIfNull(handlerAction);

            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

            if (handler == null)
            {
                Debug.Fail("The requested handler has not been registered");
            }

            await handlerAction.Invoke(handler);
        }

        public async Task UsingRepository<TRepository>(Func<TRepository, Task> handlerAction)
        {
            ArgumentNullException.ThrowIfNull(handlerAction);

            using var scope = _serviceProvider.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<TRepository>();

            if (handler == null)
            {
                Debug.Fail("The requested handler has not been registered");
            }

            await handlerAction.Invoke(handler);
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }

        private static void ConfigureServices(IServiceCollection services, string connectionString, string databaseName)
        {
            services.Configure<MongoDbSettings>(options =>
            {
                options.ConnectionString = connectionString;
                options.MongoDbDatabaseName = databaseName;
            });

            services.AddApiDependencies();
            services.AddLogging();
            services.AddBaseInfrastructure(true).AddMongoDb();
        }
    }
}
