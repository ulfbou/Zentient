namespace Zentient.Results
{
    /// <summary>
    /// Defines the contract for a non-generic result, indicating success or failure.
    /// </summary>
    public interface IResult
    {
        /// <summary>Gets a value indicating whether the operation was successful.</summary>
        bool IsSuccess { get; }
        /// <summary>Gets a value indicating whether the operation failed.</summary>
        bool IsFailure { get; }
        /// <summary>Gets a read-only list of errors that occurred during the operation.</summary>
        IReadOnlyList<ErrorInfo> Errors { get; }
        /// <summary>Gets a read-only list of informational messages related to the operation.</summary>
        IReadOnlyList<string> Messages { get; }
        /// <summary>Gets the message of the first error, if any.</summary>
        string? Error { get; }
        /// <summary>Gets the status of the result (e.g., Success, BadRequest, NotFound).</summary>
        IResultStatus Status { get; }
    }
}
