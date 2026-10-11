namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases.GetVehicle
{
    /// <summary>
    /// Output port of the get vehicle use case.
    /// </summary>
    public interface IGetVehicleOutputPort : IOutputPortStandard<GetVehicleOutput>, IOutputPortNotFound
    {
    }
}
