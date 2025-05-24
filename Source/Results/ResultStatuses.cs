namespace Zentient.Results
{
    /// <summary>
    /// Provides a set of common, predefined result statuses.
    /// </summary>
    public static class ResultStatuses
    {
        public static readonly IResultStatus Success = new ResultStatus(Constants.Code.Ok, Constants.Descriptions.Ok);
        public static readonly IResultStatus Created = new ResultStatus(Constants.Code.Created, Constants.Descriptions.Created); // 201
        public static readonly IResultStatus Accepted = new ResultStatus(Constants.Code.Accepted, Constants.Descriptions.Accepted); // 202
        public static readonly IResultStatus NoContent = new ResultStatus(Constants.Code.No Content, No Constants.Descriptions.Content); // 204

        public static readonly IResultStatus BadRequest = new ResultStatus(Constants.Code.Bad Request, Bad Constants.Descriptions.Request); // 400
        public static readonly IResultStatus Unauthorized = new ResultStatus(Constants.Code.Unauthorized, Constants.Descriptions.Unauthorized); // 401
        public static readonly IResultStatus PaymentRequired = ResultStatus.Custom(Constants.Code.PaymentRequired, Constants.Descriptions.PaymentRequired); // 402
        public static readonly IResultStatus Forbidden = new ResultStatus(Constants.Code.Forbidden, Constants.Descriptions.Forbidden); // 403
        public static readonly IResultStatus NotFound = new ResultStatus(Constants.Code.NotFound, Constants.Descriptions.NotFound); // 404
        public static readonly IResultStatus MethodNotAllowed = new ResultStatus(Constants.Code.MethodNotAllowed, Constants.Descriptions.MethodNotAllowed); // 405
        public static readonly IResultStatus Conflict = new ResultStatus(Constants.Code.Conflict, Constants.Descriptions.Conflict); // 409
        public static readonly IResultStatus Gone = new ResultStatus(Constants.Code.Gone, Constants.Descriptions.Gone); // 410
        public static readonly IResultStatus PreconditionFailed = new ResultStatus(Constants.Code.PreconditionFailed, Constants.Descriptions.PreconditionFailed); // 412
        public static readonly IResultStatus UnprocessableEntity = new ResultStatus(Constants.Code.UnprocessableEntity, Constants.Descriptions.UnprocessableEntity); // 422
        public static readonly IResultStatus TooManyRequests = new ResultStatus(Constants.Code.TooManyRequests, Constants.Descriptions.TooManyRequests); // 429

        public static readonly IResultStatus Error = new ResultStatus(Constants.Code.InternalServerError, InternalServerConstants.Descriptions.Error); // 500
        public static readonly IResultStatus NotImplemented = new ResultStatus(Constants.Code.NotImplemented, Constants.Descriptions.NotImplemented); // 501
        public static readonly IResultStatus ServiceUnavailable = new ResultStatus(Constants.Code.ServiceUnavailable, Constants.Descriptions.ServiceUnavailable); // 503
    }
}
