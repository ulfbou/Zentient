namespace Zentient.Results
{
    /// <summary>
    /// Provides a set of common, predefined result statuses.
    /// </summary>
    public static class ResultStatuses
    {
        public static readonly IResultStatus Success = new DefaultResultStatus(200, "Success");
        public static readonly IResultStatus Created = new DefaultResultStatus(201, "Created");
        public static readonly IResultStatus Accepted = new DefaultResultStatus(202, "Accepted");
        public static readonly IResultStatus NoContent = new DefaultResultStatus(204, "No Content");

        public static readonly IResultStatus BadRequest = new DefaultResultStatus(400, "Bad Request");
        public static readonly IResultStatus Unauthorized = new DefaultResultStatus(401, "Unauthorized");
        public static readonly IResultStatus PaymentRequired = DefaultResultStatus.Custom(402, "Payment Required");
        public static readonly IResultStatus Forbidden = new DefaultResultStatus(403, "Forbidden");
        public static readonly IResultStatus NotFound = new DefaultResultStatus(404, "Not Found");
        public static readonly IResultStatus MethodNotAllowed = new DefaultResultStatus(405, "Method Not Allowed");
        public static readonly IResultStatus Conflict = new DefaultResultStatus(409, "Conflict");
        public static readonly IResultStatus Gone = new DefaultResultStatus(410, "Gone");
        public static readonly IResultStatus PreconditionFailed = new DefaultResultStatus(412, "Precondition Failed");
        public static readonly IResultStatus UnprocessableEntity = new DefaultResultStatus(422, "Unprocessable Entity");
        public static readonly IResultStatus TooManyRequests = new DefaultResultStatus(429, "Too Many Requests");

        public static readonly IResultStatus Error = new DefaultResultStatus(500, "Internal Server Error");
        public static readonly IResultStatus NotImplemented = new DefaultResultStatus(501, "Not Implemented");
        public static readonly IResultStatus ServiceUnavailable = new DefaultResultStatus(503, "Service Unavailable");
    }
}
