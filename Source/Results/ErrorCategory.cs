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
        // Add more common categories as needed (e.g., Network, Database, Timeout)
    }
}