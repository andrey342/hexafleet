# Vehicle fleet microservice

REST service for the fleet of a vehicle renting company: register vehicles, list the ones available, rent a vehicle and return it. It is built on the hexagonal architecture template documented below, with .NET 9 and MongoDB.

## Quick start

Requires Docker Desktop.

```bash
docker compose up --build
```

- Swagger: http://localhost:8080/swagger
- Health: http://localhost:8080/health

The first build takes a few minutes, most of it downloading images and packages. Later starts take about 15 seconds: the API waits until MongoDB has elected a primary.

If port 8080 is taken, publish the API on another port:

```bash
API_PORT=18080 docker compose up --build
```

Stop and remove the containers and the data volume:

```bash
docker compose down -v
```

### Running from the SDK (optional)

Requires the .NET SDK 9.0.200 or later (`global.json` rolls forward within .NET 9). Start only MongoDB from the compose file and run the host:

```bash
docker compose up -d --wait mongo
dotnet run --project src/GtMotive.Estimate.Microservice.Host
```

The API listens on http://localhost:5000 and uses `mongodb://localhost:27017/?directConnection=true` from `appsettings.json`.

## API

| Method | Route | Body | Responses |
|---|---|---|---|
| `POST` | `/api/vehicles` | `{ "licensePlate": "1234 ABC", "brand": "Seat", "model": "Leon", "manufacturingDate": "2024-03-15" }` | 201 vehicle + `Location`; 400 invalid model; 409 license plate already registered; 422 older than five years |
| `GET` | `/api/vehicles/available` | | 200 vehicles that are not rented and within the age limit |
| `GET` | `/api/vehicles/{id}` | | 200 vehicle; 404 |
| `POST` | `/api/rentals` | `{ "vehicleId": "{id}", "customerId": "12345678A" }` | 201 rental; 400 invalid model; 404 vehicle not found; 409 vehicle rented or customer with an active rental; 422 vehicle older than five years |
| `POST` | `/api/rentals/{rentalId}/return` | | 200 rental with `returnedAt`; 404; 409 already returned |
| `GET` | `/health` | | 200 Healthy; 503 when MongoDB does not answer |

License plates keep 4 to 10 letters or digits and customer ids (identity document) 5 to 20; both are stored in upper case without spaces or hyphens. Errors are returned as `ProblemDetails`.

### Walkthrough

Replace `{vehicleId}` and `{rentalId}` with the ids returned by the previous calls.

```bash
# 201 Created, Location: /api/vehicles/{vehicleId}
curl -i -X POST http://localhost:8080/api/vehicles -H "Content-Type: application/json" -d '{"licensePlate":"1234 ABC","brand":"Seat","model":"Leon","manufacturingDate":"2024-03-15"}'

# 201 Created, another vehicle
curl -i -X POST http://localhost:8080/api/vehicles -H "Content-Type: application/json" -d '{"licensePlate":"5678-DEF","brand":"Kia","model":"Ceed","manufacturingDate":"2025-01-20"}'

# 200 OK
curl -i http://localhost:8080/api/vehicles/{vehicleId}

# 200 OK, both vehicles
curl -i http://localhost:8080/api/vehicles/available

# 201 Created, returns the rental id
curl -i -X POST http://localhost:8080/api/rentals -H "Content-Type: application/json" -d '{"vehicleId":"{vehicleId}","customerId":"12345678A"}'

# 409 Conflict, the customer already has an active rental (use the id of the second vehicle)
curl -i -X POST http://localhost:8080/api/rentals -H "Content-Type: application/json" -d '{"vehicleId":"{secondVehicleId}","customerId":"12345678A"}'

# 200 OK, only the second vehicle
curl -i http://localhost:8080/api/vehicles/available

# 200 OK, returnedAt is set
curl -i -X POST http://localhost:8080/api/rentals/{rentalId}/return

# 409 Conflict, already returned
curl -i -X POST http://localhost:8080/api/rentals/{rentalId}/return

# 200 OK, both vehicles again
curl -i http://localhost:8080/api/vehicles/available

# 422 Unprocessable Entity, older than five years
curl -i -X POST http://localhost:8080/api/vehicles -H "Content-Type: application/json" -d '{"licensePlate":"9999ZZZ","brand":"Seat","model":"Toledo","manufacturingDate":"2019-06-01"}'
```

## Business rules

| Rule | Where it is enforced |
|---|---|
| The fleet does not contain vehicles manufactured more than five years ago | `FleetPolicy` (Domain) is the single place for the limit. `Vehicle.Create` rejects the vehicle on registration (422), `RentalService` rejects the rental (422) and the available list filters by the oldest allowed manufacturing date. A manufacturing date in the future is rejected with 400. |
| A customer cannot have more than one vehicle rented at the same time | `RentalService` checks the active rental of the customer and of the vehicle (409). Two unique partial indexes on `rentals` (`customerId` and `vehicleId` where `returnedAt` is null) keep the rule in the database when two requests arrive at the same time. |
| A rental is returned once | `Rental.Return` rejects a second return (409). The repository replaces the document filtering by `returnedAt: null`, so a concurrent second return matches nothing and gets the same 409. |

## Architecture

| Layer | Project | Contents |
|---|---|---|
| Domain | `GtMotive.Estimate.Microservice.Domain` | `Vehicle` and `Rental` aggregates, value objects, `FleetPolicy`, `RentalService`, repository and factory interfaces, domain exceptions. No dependencies. |
| Application | `GtMotive.Estimate.Microservice.ApplicationCore` | One folder per use case with its input, output, output port and implementation. Depends on Domain. |
| Infrastructure | `GtMotive.Estimate.Microservice.Infrastructure` | MongoDB documents, mappers, repositories, unit of work, indexes and the entity factory. |
| User interface | `GtMotive.Estimate.Microservice.Api` | Controllers, MediatR requests and handlers, presenters and response models. |
| Host | `GtMotive.Estimate.Microservice.Host` | Composition root: configuration, Serilog, Swagger, health check and the Dockerfile. |

| Pattern | Files |
|---|---|
| Use Case + Output Port | `ApplicationCore/UseCases/RentVehicle/RentVehicleUseCase.cs`, `IRentVehicleOutputPort.cs`; ports `IOutputPortStandard`, `IOutputPortNotFound`, `IOutputPortConflict`, `IOutputPortUnprocessable` |
| Presenter | `Api/UseCases/RentVehicle/RentVehiclePresenter.cs` builds the `IActionResult`; errors through `Api/UseCases/ProblemResults.cs` |
| Controller + Command (MediatR) | `Api/Controllers/RentalsController.cs` sends `RentVehicleRequest`; `RentVehicleRequestHandler` maps it to the input and returns the presenter |
| ViewModel | `Api/Models/VehicleResponse.cs`, `Api/Models/RentalResponse.cs` |
| Value Object | `Domain/Vehicles/VehicleId.cs`, `LicensePlate.cs`, `ManufacturingDate.cs`, `Domain/Rentals/RentalId.cs`, `CustomerId.cs` |
| Entity / Aggregate Root | `Domain/Vehicles/Vehicle.cs`, `Domain/Rentals/Rental.cs` |
| Repository | `Domain/Vehicles/IVehicleRepository.cs`, `Infrastructure/MongoDb/Repositories/VehicleRepository.cs` and the rental pair |
| Domain Service | `Domain/Rentals/RentalService.cs` |
| Entity Factory | `Domain/Vehicles/IVehicleFactory.cs`, `Domain/Rentals/IRentalFactory.cs`, `Infrastructure/EntityFactory.cs` |
| Unit of Work | `Domain/Interfaces/IUnitOfWork.cs`, `Infrastructure/MongoDb/MongoUnitOfWork.cs` |

A rental request goes `RentalsController` → MediatR → `RentVehicleRequestHandler` → `RentVehicleUseCase` → `RentalService` + repositories → `IUnitOfWork.Save()` → `RentVehiclePresenter` → HTTP response.

## Design decisions

- **MongoDB.** The template already wires the driver and settings, and a container in the compose file keeps the service runnable without external dependencies.
- **Two aggregates, no "rented" flag on the vehicle.** Availability comes from the existence of an active `Rental`. Each operation writes a single document and the unique partial indexes protect the rules.
- **Unit of work with MongoDB transactions.** `MongoUnitOfWork` opens a session and a transaction with the first write of the request; `Save()` commits it and disposing the scope without saving aborts it. Use cases call `Save()`, domain services do not, as the template describes. Transactions require a replica set, so MongoDB runs as a single-node replica set in the compose file and in the tests.
- **Concurrency.** Duplicate keys on the partial indexes are translated to the domain exceptions and end in 409. If two transactions for the same customer are open at the same time, MongoDB reports a write conflict instead of a duplicate key and the second request gets a 500 that the client can retry; retrying the transaction automatically is left as an improvement.
- **Entity factory for new entities only.** `EntityFactory` creates ids and applies `Vehicle.Create`; the repositories rebuild entities from documents through the constructors without the age check.
- **Clock.** Use cases receive `TimeProvider` and pass `DateTime` values to the domain, which never reads the system clock. Unit tests use a fixed provider. `Rental` truncates its dates to milliseconds, the precision MongoDB stores, so the response of a rental matches what is read back later.
- **HTTP errors.** 400 for model validation and invalid value objects (`BusinessExceptionFilter`), 404 for missing resources, 409 for conflicts with the current state, 422 for vehicles outside the age policy. All of them return `ProblemDetails`; `ProblemDetailsResultFilter` adds `instance` and `traceId` to every one.
- **Authentication.** Out of scope. Authorization is still registered in the pipeline so endpoints can be protected later without touching the template wiring.

## Tests

| Type | Project | What the example covers | Command |
|---|---|---|---|
| Unit | `test/unit/GtMotive.Estimate.Microservice.UnitTests` | Five-year boundary, future dates and normalization in `Vehicle`; `Rental` returns; `RentalService` and `RentVehicleUseCase` with Moq (output port called, `Save()` only on success) | `dotnet test test/unit/GtMotive.Estimate.Microservice.UnitTests` |
| Infrastructure | `test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests` | Host level with `TestServer` and the template `Startup`: model binding and validation of `POST /api/vehicles`, with the MediatR handler replaced by a stub | `dotnet test test/infrastructure/GtMotive.Estimate.Microservice.InfrastructureTests` |
| Functional | `test/functional/GtMotive.Estimate.Microservice.FunctionalTests` | Handlers, use cases and MongoDB repositories through the composition root, without the host: creation, rental, single active rental (also at index level) and returns | `dotnet test test/functional/GtMotive.Estimate.Microservice.FunctionalTests` |

Run everything with `dotnet test src/microservice.sln`. The functional tests start a MongoDB replica set with Testcontainers, so Docker must be running. To use the MongoDB of the compose file instead:

```bash
docker compose up -d --wait mongo
MongoDb__ConnectionString="mongodb://localhost:27017/?directConnection=true" dotnet test test/functional/GtMotive.Estimate.Microservice.FunctionalTests
```

## Changes to the template

- `global.json` rolls forward to any .NET 9 feature band; the template pinned SDK 9.0.200 with roll forward disabled.
- Package pins updated to 9.0.x for the hosting and test packages, and MongoDB.Driver to 3.12.0, which removes vulnerable transitive dependencies.
- Key Vault, IdentityServer and Application Insights removed from the host so it runs locally without Azure or an identity server. This deletes `Host/Configuration/AppSettings.cs`, which only held the JWT authority, and `Host/Infrastructure/Swagger/IdentityServerApiSecurityOperationFilter.cs`, which added the OAuth2 requirement and the 401/403 responses to endpoints with `[Authorize]`. `SwaggerExtensions.cs` no longer defines the OAuth2 scheme against the identity server, and the two `SA1010` suppressions for the filter are gone from the host `GlobalSuppressions.cs`. Authorization is still registered in the pipeline.
- AutoMapper removed: it was unused and version 14.0.0 fails the restore with NU1903 (known vulnerability) because warnings are errors.
- Dockerfile rewritten as a multi-stage build that only uses nuget.org; the original one needed a private Azure DevOps feed.
- `test/.editorconfig` turns off CA1515 (xUnit needs public test classes) and CA1707 (test names use underscores) for all the test projects. It replaces `GlobalSuppressions.cs` in `FunctionalTests` and `InfrastructureTests`, which only suppressed CA1515 for one collection fixture each.
- The infrastructure tests reference `Microsoft.AspNetCore.TestHost` 9.0.3 directly: `Acheve.TestHost` brings version 8, which fails to write JSON responses on ASP.NET Core 9.

---

The original documentation of the template follows.

# Sample Implementation of Hexagonal Architecture in a Microservice
## Index
### [Introduction](#introduction)
### [Clean Architecture](#clean-architecture)
### [Hexagonal Architecture Style](#hexagonal-architecture-style)
- [The Left side](#the-left-side)
- [The Right Side](#the-right-side)
- [Ports](#ports)
- [Adapters](#adapters)
### [Use Cases](#use-cases)
### [Design Patterns](#design-patterns)
- [Controller and Command Pattern](#controller)
- [ViewModel](#view-model)
- [Presenter](#presenter)
- [Unit of Work](#unit-of-work)
- [First-Class Collections](#first-class-collections)
- [Factory](@factory)
### [DDD Patterns](#ddd-patterns)
- [Value Object](#value-object)
- [Entity](#entity)
- [Aggregate Root](#aggregate-root)
- [Repository](#repository)
- [Use Case](#use-case)
- [Bounded Context](#bounded-context)
- [Entity Factory](#entity-factory)
- [Domain Service](#domain-service)
### [Separation of Concerns](#separation-of-concerns)
- [Domain](#domain)
- [Application](#application)
- [Infrastructure](#infrastructure)
- [User interface](#user-interface)
### [.NET Core Web API](#net-core-web-api)
- [Swagger](#swagger)
- [Microsoft Extensions](#microsoft-extensions)
- [Logging](#logging)
- [Authentication](#authentication)
- [Authorization](#authorization)

## Introduction
Sample implementation of the Clean Architecture Principles with .NET Core. Use cases as central organizing structure, decoupled from frameworks and technology details. Built with small components that are developed and tested in isolation.

This template is a Virtual Wallet applicationn which a customer can register an account then manage the balance with Deposits, Withdraws and Transfers.

## Clean Architecture
The Clean Architecture style focus on a loosely coupled implementation of use cases and it is summarized as:
- It is an architecture style that the Use Cases are the central organizing structure.
- Follows the Ports and Adapters pattern.
- The implementation is guided by tests (TDD Outside-In).
- Decoupled from technology details.

![clean-architecture.jpg](/.attachments/clean-architecture.jpg)

## Hexagonal Architecture Style
Hexagonal Architecture (aka Ports and Adapters) is one strategy to decouple the use cases from the external details. 
It is an architectural style that allows an application to equally be driven by users, programs, automated test or batch scripts, and to be developed and tested in isolation from its eventual run-time devices and databases. It has another beneficial effect: it allows to isolate the core business of an application and automatically test its behaviour independently of everything else.

This architectural style uses a notion of port and adapter. Thus, it is also referred to as **Ports and Adapters**. Port is an abstract concept, whereas the adapter is the concrete implementation of a port.

![hexagonal-architecture.png](/.attachments/hexagonal-architecture.png)

The hexagon is the application itself. Saying “hexagon” and saying “application” is the same thing. Inside the hexagon we just have the things that are important for the business problem that the application is trying to solve. The hexagon contains the business logic, with no references to any technology, framework or real world device. So the application is technology agnostic.

Outside the hexagon we have any real world thing that the application interacts with. These things include humans, other applications, or any hardware/software device. They are the actors. Actors are arranged around the hexagon depending on who triggers the interaction between the application and the actor.

### The Left Side
Actors on the left/top side are Drivers, or Primary Actors. The interaction is triggered by the actor. A driver is an actor that interacts with the application to achieve a goal. Primary Actors are usually the user interface or the Test Suit.

### The Right Side
Actors on the right/bottom side are Driven Actors, or Secondary Actors. The interaction is triggered by the application. A driven actor provides some functionality needed by the application for implementing the business logic. The Secondary Actors are usually Databases, Cloud Services or other systems.

### Ports
Ports are the application boundary, in the picture a port is an edge of the hexagon. From the outside world, actors can only interact with the hexagon ports, they shouldn’t be able to access the inside of the hexagon. Ports are interfaces that the application offers to the outside world for allowing actors interact with the application.

### Adapters
Actors interact with hexagon ports through adapters using a specific technology. An adapter is a software component that allows a technology to interact with a port of the hexagon. Given a port, there may be an adapter for each desired technology that we want to use. Adapters are outside the application. A driver adapter uses a driver port interface, converting a specific technology request into a technology agnostic request to a driver port.

![hexagonal-architecture2.png](/.attachments/hexagonal-architecture2.png)

## Use Cases
Use Cases are algorithms that interpret the input to generate the output data, their implementation should be closer as possible to the business vocabulary.

When talking about a use case, it does not matter if it a Mobile or a Desktop application, use cases are delivery independent. The most important about use cases is how they interact with the actors.

- Primary actors initiate a use case. They can be the End User, another system or a clock.
- Secondary actors are affected by use cases.

On this template the Customer can Register an account then manage the balance by Deposits, Withdrawls and Transfers. A set of use cases is used to describe software. Following the Customer primary actor on the left side, in the middle the Virtual Wallet system and the secondary actors on the right side

## Design Patterns
The following design patterns will help us continue to implement use cases consistently. 

### Controller and Command Pattern
In a Command-based architecture, Controllers receive Requests and send commands to perform some operationnd we a have separate handler of command that makes the separation of concern and improves the single responsibility as well. 

To implement this architecture, we can use a third-party library, like **MediatR** (Mediator Pattern) which does a lot of groundwork for us. The mediator pattern defines an object that encapsulates how a set of objects interact.

```
    public sealed class CustomersController : ControllerBase
    {

        // code omitted to simplify

        public async Task<IActionResult> Post([FromBody][Required] RegisterRequest request)
        {
            _appLogger.LogDebug(
                "Entering Post. Request: {@request}",
                request);

            var presenter = await _mediator.Send(request);
            return presenter.ActionResult;
        }
    }
```
#### MediatR
- The mediator pattern defines an object that encapsulates how a set of objects interact.
- It promotes loose coupling by keeping the objects from referring to each other explicitly.
- It promotes the Single Responsibility Principle by allowing the communication to be offloaded to a class that handles just that.

MediatR allows us to decouple our controllers from our business logic by having our controller actions send a request message to a handler. The MediatR library supports two type of operations.
- Request/response messages, dispatched to a single handler.
- Notification messages, dispatched to multiple handlers.

The request/response interface handles both command and query scenarios. First, create a message:
```
    public class RegisterRequest : IRequest<IWebApiPresenter>
    {
        public RegisterRequest(string ssn, string name, decimal initialAmount)
        {
            SSN = ssn;
            Name = name;
            InitialAmount = initialAmount;
        }

        // code omitted to simplify
    }
```

Next, the handler. Build the Input message then call the Use Case, you should notice that the handler do not build the Response, instead this responsibility is delegated to the presenter object.:
```
    public class RegisterRequestHandler : IRequestHandler<RegisterRequest, IWebApiPresenter>
    {
        private readonly IRegisterUseCase _useCase;
        private readonly RegisterPresenter _presenter;

        // code omitted to simplify

        public async Task<IWebApiPresenter> Handle(RegisterRequest request, CancellationToken cancellationToken)
        {
            var input = new RegisterInput(new SSN(request.SSN), new Name(request.Name), new PositiveMoney(request.InitialAmount));
            await _useCase.Execute(input);
            return _presenter;
        }
    }
```

### View Model
ViewModels are data transfer objects, they will be rendered by the framework so we need to follow the framework guidelines. You have to add comments describing each property and the [Required] attribute so swagger generators could know the properties that are not nullable.

```
    public sealed class RegisterResponse
    {
        public RegisterResponse(CustomerId customerId, SSN ssn, Name name, List<AccountDetailsModel> accounts)
        {
            CustomerId = customerId.ToGuid();
            SSN = ssn.ToString();
            Name = name.ToString();
            Accounts = accounts;
        }

        /// <summary>
        /// Gets customer ID.
        /// </summary>
        [Required]
        public Guid CustomerId { get; }

        /// <summary>
        /// Gets sSN.
        /// </summary>
        [Required]
        public string SSN { get; }

        /// <summary>
        /// Gets name.
        /// </summary>
        [Required]
        public string Name { get; }

        /// <summary>
        /// Gets accounts.
        /// </summary>
        [Required]
        public List<AccountDetailsModel> Accounts { get; }
    }
```
### Presenter
Presenters are called by te application Use Cases and build the Response objects.
```
    public sealed class RegisterPresenter : IWebApiPresenter, IRegisterOutputPort
    {
        public IActionResult ActionResult { get; private set; }

        public void CustomerAlreadyRegistered(string message)
        {
            ActionResult = new BadRequestObjectResult(message);
        }

        public void StandardHandle(RegisterOutput output)
        {
            var transactions = new List<TransactionModel>();

            foreach (var item in output.Account.Transactions)
            {
                var transaction = new TransactionModel(item.Amount, item.Description, item.TransactionDate);
                transactions.Add(transaction);
            }

            var account = new AccountDetailsModel(output.Account.AccountId, output.Account.CurrentBalance, transactions);

            var accounts = new List<AccountDetailsModel>();
            accounts.Add(account);

            var registerResponse = new RegisterResponse(output.Customer.CustomerId, output.Customer.SSN, output.Customer.Name, accounts);

            ActionResult = new CreatedAtRouteResult(
                "GetCustomer",
                new
                {
                    CustomerId = registerResponse.CustomerId,
                },
                registerResponse);
        }
    }
```
It is important to understand that from the Application perspective the use cases see an OutputPort with custom methods to call dependent on the message, and from the Web Api perspective the Controller only see the ViewModel property.

**Standard Output**

The output port for the use case regular behavior.

**Error Output**

Called when an blocking errors happens.

**Alternative Output**

Called when an blocking errors happens.

### Unit Of Work
Unit of work is the pattern that allows handling transactions during data manipulation using the repository pattern. It maintains a list of the objects affected by a business transaction and coordinates the writing of the changes and the resolution of concurrency problems.

A basic example of the implementation of the unit of work pattern can be seen in the following code of the template:

```
    public interface IUnitOfWork
    {
        Task<int> Save();
    }
```

```
    public sealed class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ExampleContext _context;
        private bool _disposed = false;

        public UnitOfWork(ExampleContext context)
        {
            _context = context;
        }

        public async Task<int> Save()
        {
            int affectedRows = await _context.SaveChangesAsync();
            return affectedRows;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }

            _disposed = true;
        }
    }
```

### First-Class Collections
Application of this rule is simple: any class that contains a collection should contain no other member variables. Each collection gets wrapped in its own class, so now behaviors related to the collection have a home. You may find that filters become a part of this new class. Also, your new class can handle activities like joining two groups together or applying a rule to each element of the group.

```
    public sealed class CreditsCollection
    {
        private readonly IList<ICredit> _credits;

        public CreditsCollection()
        {
            this._credits = new List<ICredit>();
        }

        public void Add<T>(IEnumerable<T> credits)
            where T : ICredit
        {
            foreach (var credit in credits)
            {
                this.Add(credit);
            }
        }

        public void Add(ICredit credit) => this._credits.Add(credit);

        public IReadOnlyCollection<ICredit> GetTransactions()
        {
            var transactions = new ReadOnlyCollection<ICredit>(this._credits);
            return transactions;
        }

        public PositiveMoney GetTotal()
        {
            PositiveMoney total = new PositiveMoney(0);

            foreach (ICredit credit in this._credits)
            {
                total = credit.Sum(total);
            }

            return total;
        }
    }
```

### Factory
In Factory pattern, we create object without exposing the creation logic to the client and refer to newly created object using a common interface.

```
    public interface IAccountFactory
    {
        IAccount NewAccount(CustomerId customerId);

        ICredit NewCredit(IAccount account, PositiveMoney amountToDeposit, DateTime transactionDate);

        IDebit NewDebit(IAccount account, PositiveMoney amountToWithdraw, DateTime transactionDate);
    }
```
```
    public interface ICustomerFactory
    {
        ICustomer NewCustomer(SSN ssn, Name name);
    }
```    
```
    public sealed class EntityFactory : ICustomerFactory, IAccountFactory
    {
        public IAccount NewAccount(CustomerId customerId) => new AccountEntity(customerId);

        public ICredit NewCredit(
            IAccount account,
            PositiveMoney amountToDeposit,
            DateTime transactionDate) => new CreditEntity(account, amountToDeposit, transactionDate);

        public ICustomer NewCustomer(
            SSN ssn,
            Name name) => new CustomerEntity(ssn, name);

        public IDebit NewDebit(
            IAccount account,
            PositiveMoney amountToWithdraw,
            DateTime transactionDate) => new DebitEntity(account, amountToWithdraw, transactionDate);
    }
```

## DDD Patterns
Since, as we have seen, the hexagonal architecture is still a domain-oriented architecture (DDD), the following DDD patterns will also be used (all of them are implemented in the template):

### Value Object
Encapsulate the tiny domain business rules. Structures that are unique by their properties and the whole object is immutable, once it is created its state can not change.

As seen in the example, the VOs are where we have the field validations.

```
    public readonly struct Name
    {
        private readonly string _text;

        public Name(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new NameShouldNotBeEmptyException($"The {nameof(text)} field is required.");
            }

            this._text = text;
        }

        public override string ToString()
        {
            return this._text;
        }
    }
```

Rules of thumb:

- The developer should make the Value Object serializable and deserializable.
- A Value Object can not reference an Entity or another mutable object.

### Entity
Highly abstract and mutable objects unique identified by its IDs

```
    public abstract class Customer : ICustomer
    {
        public Customer()
        {
            this.Accounts = new AccountCollection();
        }

        public CustomerId Id { get; protected set; }

        public Name Name { get; protected set; }

        public SSN SSN { get; protected set; }

        public AccountCollection Accounts { get; protected set; }

        public void Register(AccountId accountId)
        {
            this.Accounts ??= new AccountCollection();
            this.Accounts.Add(accountId);
        }
    }
```

Rules of Thumb:

- Entities are mutable.
- Entities are highly abstract.
- Entities do not need to be serializable.
- The entity state should be encapsulated to external access.

### Aggregate Root
Similar to Entities with the addition that Aggregate Root are responsible to keep the graph of objects consistent.

- Owns entities object graph.
- Ensure the child entities state are always consistent.
- Define the transaction scope.

```
    public abstract class Account : IAccount
    {
        protected Account()
        {
            this.Credits = new CreditsCollection();
            this.Debits = new DebitsCollection();
        }

        public AccountId Id { get; protected set; }

        public CreditsCollection Credits { get; protected set; }

        public DebitsCollection Debits { get; protected set; }

        public ICredit Deposit(IAccountFactory entityFactory, PositiveMoney amountToDeposit)
        {
            var credit = entityFactory.NewCredit(this, amountToDeposit, DateTime.UtcNow);
            this.Credits.Add(credit);
            return credit;
        }

        public IDebit Withdraw(IAccountFactory entityFactory, PositiveMoney amountToWithdraw)
        {
            if (this.GetCurrentBalance().LessThan(amountToWithdraw))
            {
                throw new MoneyShouldBePositiveException("Account has not enough funds.");
            }

            var debit = entityFactory.NewDebit(this, amountToWithdraw, DateTime.UtcNow);
            this.Debits.Add(debit);
            return debit;
        }

        public bool IsClosingAllowed()
        {
            return this.GetCurrentBalance().IsZero();
        }

        public Money GetCurrentBalance()
        {
            var totalCredits = this.Credits
                .GetTotal();

            var totalDebits = this.Debits
                .GetTotal();

            var totalAmount = totalCredits
                .Subtract(totalDebits);

            return totalAmount;
        }
    }
```

Rules of thumb:

- Protect business invariants inside Aggregate boundaries.
- Design small Aggregates.
- Reference other Aggregates by identity only.
- Update other Aggregates using eventual consistency.

### Repository
Provides persistence capabilities to Aggregate Roots or Entities.

```
    public sealed class CustomerRepository : ICustomerRepository
    {
        private readonly ExampleContext _context;

        public CustomerRepository(ExampleContext context)
        {
            _context = context ??
                throw new ArgumentNullException(nameof(context));
        }

        public async Task Add(ICustomer customer)
        {
            await _context.Customers.AddAsync((CustomerEntity)customer);
            await _context.SaveChangesAsync();
        }

        public async Task<ICustomer> GetBy(CustomerId customerId)
        {
            var customer = await _context.Customers
                .Where(c => c.Id.Equals(customerId))
                .SingleOrDefaultAsync();

            if (customer is null)
            {
                throw new CustomerNotFoundException($"The customer {customerId} does not exist or is not processed yet.");
            }

            var accounts = _context.Accounts
                .Where(e => e.CustomerId.Equals(customer.Id))
                .Select(e => e.Id)
                .ToList();

            customer.LoadAccounts(accounts);

            return customer;
        }

        public async Task Update(ICustomer customer)
        {
            _context.Customers.Update((CustomerEntity)customer);
            await _context.SaveChangesAsync();
        }
    }
```    
Rules of thumb:

- The repository is designed around the aggregate root or entity.
- A repository for every entity is a code smell.

### Use Case
It is the application entry point for an user interaction. It accepts an input message, executes the algorithm then it should give the output message to the Output port.

They are also called **Application Service** in DDD architectures.

```
    public sealed class WithdrawUseCase : IWithdrawUseCase
    {
        private readonly AccountService _accountService;
        private readonly IWithdrawOutputPort _outputPort;
        private readonly IAccountRepository _accountRepository;
        private readonly IUnitOfWork _unitOfWork;

        public WithdrawUseCase(
            AccountService accountService,
            IWithdrawOutputPort outputPort,
            IAccountRepository accountRepository,
            IUnitOfWork unitOfWork)
        {
            this._accountService = accountService;
            this._outputPort = outputPort;
            this._accountRepository = accountRepository;
            this._unitOfWork = unitOfWork;
        }

        public async Task Execute(WithdrawInput input)
        {
            try
            {
                var account = await this._accountRepository.Get(input.AccountId);
                var debit = await this._accountService.Withdraw(account, input.Amount);

                await this._unitOfWork.Save();

                this.BuildOutput(debit, account);
            }
            catch (AccountNotFoundException notFoundEx)
            {
                this._outputPort.NotFoundHandle(notFoundEx.Message);
                return;
            }
            catch (MoneyShouldBePositiveException outOfBalanceEx)
            {
                this._outputPort.OutOfBalanceHandle(outOfBalanceEx.Message);
                return;
            }
        }

        private void BuildOutput(IDebit debit, IAccount account)
        {
            var output = new WithdrawOutput(
                debit,
                account.GetCurrentBalance());

            this._outputPort.StandardHandle(output);
        }
    }
```

Rules of thumb:

- The use case implementation are close to a human readable language.
- Ideally a class has a single use case.
- Invokes transaction operations (eg. Unit Of Work).

### Bounnded Context
It is a logical boundary, similar to a module in a system. In the Manga project the single Domain project is the single bounded context we designed.

As a rule out, in most cases a microservice will have only a bounded context.

### Entity Factory
Creates new instances of Entities and Aggregate Roots. Should be implemented by the Infrastructure layer. See [Factory](#factory)

### Domain Service
Useful functions cross Entities.

```
    public class AccountService
    {
        private readonly IAccountFactory _accountFactory;
        private readonly IAccountRepository _accountRepository;

        public AccountService(
            IAccountFactory accountFactory,
            IAccountRepository accountRepository)
        {
            this._accountFactory = accountFactory;
            this._accountRepository = accountRepository;
        }

        public async Task<IAccount> OpenCheckingAccount(CustomerId customerId, PositiveMoney amount)
        {
            var account = this._accountFactory.NewAccount(customerId);
            var credit = account.Deposit(this._accountFactory, amount);
            await this._accountRepository.Add(account, credit);

            return account;
        }

        public async Task<IDebit> Withdraw(IAccount account, PositiveMoney amount)
        {
            var debit = account.Withdraw(this._accountFactory, amount);
            await this._accountRepository.Update(account, debit);

            return debit;
        }

        public async Task<ICredit> Deposit(IAccount account, PositiveMoney amount)
        {
            var credit = account.Deposit(this._accountFactory, amount);
            await this._accountRepository.Update(account, credit);

            return credit;
        }
    }
```

Rules of thumb:

- It should not call transaction methods, should not call a Unit Of Work method.

## Separation of concerns
Applications can be logically built to follow this principle by separating core business behavior from infrastructure and user interface logic. Ideally, business rules and logic should reside in a separate project, which should not depend on other projects in the application. This helps ensure that the business model is easy to test and can evolve without being tightly coupled to low-level implementation details. Separation of concerns is a key consideration behind the use of layers in application architectures.

### Domain
The package that contains the High Level Modules which describe the Domain via **Aggregate Roots**, **Entities** and **Value Objects**. By design this project is Highly Abstract and Stable, in other terms this package contains a considerable amount of interfaces and should not depend on external libraries and frameworks. 

### Application
A project that contains the Application **Use Cases** which orchestrate the high level business rules. By design the orchestration will depend on abstractions of external services (eg. Repositories). The package exposes **Boundaries Interfaces** (in other terms Contracts or **Ports**) which are used by the user interface. Should not depend on external libraries and frameworks.

### Infrastructure
The infrastructure layer is responsible to implement the **Adapters** to the **Secondary Actors**(Right Side). For instance an SQL Server Database is a secondary actor which is affected by the application use cases, all the implementation and dependencies required to consume the SQL Server is created on infrastructure. By design the infrastructure depends on application layer.

### User Interface
Responsible for rendering the Graphical User Interface (GUI) to interact with the User or other systems. Made of **Controllers** which receive HTTP Requests and **Presenters** which converts the application outputs into **ViewModels** that are rendered as HTTP Responses. 

