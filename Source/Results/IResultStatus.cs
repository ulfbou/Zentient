namespace Zentient.Results
{
    /// <summary>
    /// Defines the contract for a result status.
    /// </summary>
    public interface IResultStatus
    {
        /// <summary>Gets the numerical code for the result status.</summary>
        int Code { get; }

        /// <summary>Gets a human-readable description for the result status.</summary>
        string Description { get; }
    }
}