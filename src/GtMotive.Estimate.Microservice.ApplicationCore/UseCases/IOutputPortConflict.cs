namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases
{
    /// <summary>
    /// Interface to define the Conflict Output Port.
    /// </summary>
    public interface IOutputPortConflict
    {
        /// <summary>
        /// Informs the request conflicts with the current state of the resource.
        /// </summary>
        /// <param name="message">Text description.</param>
        void ConflictHandle(string message);
    }
}
