using System;
using System.Threading.Tasks;
using GtMotive.Estimate.Microservice.ApplicationCore.UseCases.RentVehicle;
using GtMotive.Estimate.Microservice.Domain;
using GtMotive.Estimate.Microservice.Domain.Interfaces;
using GtMotive.Estimate.Microservice.Domain.Rentals;
using GtMotive.Estimate.Microservice.Domain.Vehicles;
using GtMotive.Estimate.Microservice.UnitTests.Support;
using Moq;
using Xunit;

namespace GtMotive.Estimate.Microservice.UnitTests.ApplicationCore.RentVehicle
{
    public sealed class RentVehicleUseCaseTests
    {
        private static readonly DateTime Now = new(2026, 10, 10, 9, 30, 0, DateTimeKind.Utc);
        private static readonly CustomerId Customer = new("12345678A");

        private readonly Mock<IRentVehicleOutputPort> _outputPort = new();
        private readonly Mock<IVehicleRepository> _vehicleRepository = new();
        private readonly Mock<IRentalRepository> _rentalRepository = new();
        private readonly Mock<IRentalFactory> _rentalFactory = new();
        private readonly Mock<IUnitOfWork> _unitOfWork = new();
        private readonly RentVehicleUseCase _useCase;

        public RentVehicleUseCaseTests()
        {
            var rentalService = new RentalService(_rentalFactory.Object, _rentalRepository.Object);

            _useCase = new RentVehicleUseCase(
                _outputPort.Object,
                _vehicleRepository.Object,
                rentalService,
                _unitOfWork.Object,
                new FixedTimeProvider(new DateTimeOffset(Now)));
        }

        [Fact]
        public async Task Execute_reports_not_found_and_does_not_save_when_the_vehicle_does_not_exist()
        {
            var vehicleId = VehicleId.New();
            _vehicleRepository.Setup(r => r.GetById(vehicleId)).ReturnsAsync((Vehicle)null);

            await _useCase.Execute(new RentVehicleInput(vehicleId, Customer));

            _outputPort.Verify(p => p.NotFoundHandle(It.IsAny<string>()), Times.Once);
            _outputPort.VerifyNoOtherCalls();
            _unitOfWork.Verify(u => u.Save(), Times.Never);
        }

        [Fact]
        public async Task Execute_reports_the_rental_and_saves_once_when_vehicle_and_customer_are_free()
        {
            var vehicle = AVehicleInTheRepository(new DateTime(2023, 5, 1, 0, 0, 0, DateTimeKind.Utc));
            var rental = new Rental(RentalId.New(), vehicle.Id, Customer, Now, null);
            _rentalFactory.Setup(f => f.NewRental(vehicle.Id, Customer, Now)).Returns(rental);

            await _useCase.Execute(new RentVehicleInput(vehicle.Id, Customer));

            _rentalRepository.Verify(r => r.Add(rental), Times.Once);
            _unitOfWork.Verify(u => u.Save(), Times.Once);
            _outputPort.Verify(p => p.StandardHandle(It.Is<RentVehicleOutput>(o => o.Rental == rental)), Times.Once);
            _outputPort.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Execute_reports_a_conflict_and_does_not_save_when_the_customer_has_an_active_rental()
        {
            var vehicle = AVehicleInTheRepository(new DateTime(2023, 5, 1, 0, 0, 0, DateTimeKind.Utc));
            var activeRental = new Rental(RentalId.New(), VehicleId.New(), Customer, Now.AddDays(-2), null);
            _rentalRepository.Setup(r => r.GetActiveByCustomer(Customer)).ReturnsAsync(activeRental);

            await _useCase.Execute(new RentVehicleInput(vehicle.Id, Customer));

            _outputPort.Verify(p => p.ConflictHandle(It.IsAny<string>()), Times.Once);
            _outputPort.VerifyNoOtherCalls();
            _unitOfWork.Verify(u => u.Save(), Times.Never);
        }

        [Fact]
        public async Task Execute_reports_a_conflict_and_does_not_save_when_the_vehicle_is_already_rented()
        {
            var vehicle = AVehicleInTheRepository(new DateTime(2023, 5, 1, 0, 0, 0, DateTimeKind.Utc));
            var activeRental = new Rental(RentalId.New(), vehicle.Id, new CustomerId("87654321B"), Now.AddDays(-2), null);
            _rentalRepository.Setup(r => r.GetActiveByVehicle(vehicle.Id)).ReturnsAsync(activeRental);

            await _useCase.Execute(new RentVehicleInput(vehicle.Id, Customer));

            _outputPort.Verify(p => p.ConflictHandle(It.IsAny<string>()), Times.Once);
            _outputPort.VerifyNoOtherCalls();
            _unitOfWork.Verify(u => u.Save(), Times.Never);
        }

        [Fact]
        public async Task Execute_reports_unprocessable_and_does_not_save_when_the_vehicle_is_too_old()
        {
            var vehicle = AVehicleInTheRepository(new DateTime(2021, 10, 9, 0, 0, 0, DateTimeKind.Utc));

            await _useCase.Execute(new RentVehicleInput(vehicle.Id, Customer));

            _outputPort.Verify(p => p.UnprocessableHandle(It.IsAny<string>()), Times.Once);
            _outputPort.VerifyNoOtherCalls();
            _unitOfWork.Verify(u => u.Save(), Times.Never);
        }

        [Fact]
        public async Task Execute_reports_a_conflict_and_does_not_save_when_another_request_writes_at_the_same_time()
        {
            var vehicle = AVehicleInTheRepository(new DateTime(2023, 5, 1, 0, 0, 0, DateTimeKind.Utc));
            var rental = new Rental(RentalId.New(), vehicle.Id, Customer, Now, null);
            _rentalFactory.Setup(f => f.NewRental(vehicle.Id, Customer, Now)).Returns(rental);
            _rentalRepository.Setup(r => r.Add(rental)).ThrowsAsync(new ConcurrencyConflictException("Retry the request."));

            await _useCase.Execute(new RentVehicleInput(vehicle.Id, Customer));

            _outputPort.Verify(p => p.ConflictHandle("Retry the request."), Times.Once);
            _outputPort.VerifyNoOtherCalls();
            _unitOfWork.Verify(u => u.Save(), Times.Never);
        }

        private Vehicle AVehicleInTheRepository(DateTime manufacturingDate)
        {
            var vehicle = new Vehicle(VehicleId.New(), new LicensePlate("1234ABC"), "Seat", "Leon", new ManufacturingDate(manufacturingDate));
            _vehicleRepository.Setup(r => r.GetById(vehicle.Id)).ReturnsAsync(vehicle);

            return vehicle;
        }
    }
}
