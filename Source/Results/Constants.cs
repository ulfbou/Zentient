namespace Zentient.Results
{
    /// <summary>Provides constant values for result codes and their descriptions.</summary>
    internal static class Constants
    {
        /// <summary>Contains integer constants representing standard HTTP status codes.</summary>
        internal static class Code
        {
            public const int Ok = 200;
            public const int Created = 201;
            public const int Accepted = 202;
            public const int Content = 204;
            public const int Request = 400;
            public const int Unauthorized = 401;
            public const int PaymentRequired = 402;
            public const int Forbidden = 403;
            public const int NotFound = 404;
            public const int MethodNotAllowed = 405;
            public const int Conflict = 409;
            public const int Gone = 410;
            public const int PreconditionFailed = 412;
            public const int UnprocessableEntity = 422;
            public const int TooManyRequests = 429;
            public const int InternalServerError = 500;
            public const int NotImplemented = 501;
            public const int ServiceUnavailable = 503;
        }

        /// <summary>
        /// Contains string constants representing standard HTTP status descriptions.
        /// </summary>
        internal static class Description
        {
            public const string Created = "Created";
            public const string Accepted = "Accepted";
            public const string Content = "Content";
            public const string Request = "Request";
            public const string Unauthorized = "Unauthorized";
            public const string PaymentRequired = "Payment Required";
            public const string Forbidden = "Forbidden";
            public const string NotFound = "Not Found";
            public const string MethodNotAllowed = "Method Not Allowed";
            public const string Conflict = "Conflict";
            public const string Gone = "Gone";
            public const string PreconditionFailed = "Precondition Failed";
            public const string UnprocessableEntity = "Unprocessable Entity";
            public const string TooManyRequests = "Too Many Requests";
            public const string Error = "Error";
            public const string NotImplemented = "Not Implemented";
            public const string ServiceUnavailable = "Service Unavailable";
        }
    }
}
