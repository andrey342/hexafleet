namespace GtMotive.Estimate.Microservice.ApplicationCore.UseCases
{
    /// <summary>
    /// Interface to define the Unprocessable Output Port.
    /// </summary>
    public interface IOutputPortUnprocessable
    {
        /// <summary>
        /// Informs the request is valid but breaks a business rule.
        /// </summary>
        /// <param name="message">Text description.</param>
        void UnprocessableHandle(string message);
    }
}
