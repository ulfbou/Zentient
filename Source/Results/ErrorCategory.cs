namespace Zentient.Results
{
    /// <summary>
    /// Represents categories for errors, providing strong typing for common error types.
    /// </summary>
    public enum ErrorCategory
    {
        /// <summary>A general, uncategorized error.</summary>
        General,

        /// <summary>An error related to input validation.</summary>
        Validation,

        /// <summary>An error during authentication (e.g., invalid credentials).</summary>
        Authentication,

        /// <summary>An error due to insufficient authorization.</summary>
        Authorization,

        /// <summary>A resource was not found.</summary>
        NotFound,

        /// <summary>A conflict occurred (e.g., a duplicate resource).</summary>
        Conflict,

        /// <summary>An unhandled exception occurred.</summary>
        Exception,

        /// <summary>An error related to a network issue.</summary>
        Network,

        /// <summary>An error related to a database operation.</summary>
        Database,

        /// <summary>A timeout error occurred.</summary>
        Timeout,

        /// <summary>An error related to a security issue.</summary>
        Security,

        /// <summary>An error related to a request (e.g., malformed request).</summary>
        Request,

        /// <summary>An error indicating that the user is not authorized (unauthenticated).</summary>
        Unauthorized,

        /// <summary>An error indicating that the user is authenticated but does not have permission to perform the action.</summary>
        Forbidden,

        /// <summary>An error related to concurrent operations, such as concurrency conflicts.</summary>
        Concurrency,

        /// <summary>An error indicating that too many requests have been made in a given amount of time (rate limiting).</summary>
        TooManyRequests,

        /// <summary>An error related to an external service or dependency.</summary>
        ExternalService,
    }
}
