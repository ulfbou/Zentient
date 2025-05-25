namespace Zentient.Results
{
    /// <summary>
    /// Provides a set of common, predefined result statuses.
    /// </summary>
    public static class ResultStatuses
    {
        public static readonly IResultStatus Success = new ResultStatus(Constants.Code.Ok, Constants.Description.Ok);
        public static readonly IResultStatus Created = new ResultStatus(Constants.Code.Created, Constants.Description.Created); // 201
        public static readonly IResultStatus Accepted = new ResultStatus(Constants.Code.Accepted, Constants.Description.Accepted); // 202
        public static readonly IResultStatus NoContent = new ResultStatus(Constants.Code.NoContent, Constants.Description.NoContent); // 204

        public static readonly IResultStatus BadRequest = new ResultStatus(Constants.Code.BadRequest, Constants.Description.BadRequest); // 400
        public static readonly IResultStatus Unauthorized = new ResultStatus(Constants.Code.Unauthorized, Constants.Description.Unauthorized); // 401
        public static readonly IResultStatus PaymentRequired = ResultStatus.Custom(Constants.Code.PaymentRequired, Constants.Description.PaymentRequired); // 402
        public static readonly IResultStatus Forbidden = new ResultStatus(Constants.Code.Forbidden, Constants.Description.BadRequest); // 403
        public static readonly IResultStatus NotFound = new ResultStatus(Constants.Code.NotFound, Constants.Description.NotFound); // 404
        public static readonly IResultStatus MethodNotAllowed = new ResultStatus(Constants.Code.MethodNotAllowed, Constants.Description.MethodNotAllowed); // 405
        public static readonly IResultStatus Conflict = new ResultStatus(Constants.Code.Conflict, Constants.Description.Conflict); // 409
        public static readonly IResultStatus Gone = new ResultStatus(Constants.Code.Gone, Constants.Description.Gone); // 410
        public static readonly IResultStatus PreconditionFailed = new ResultStatus(Constants.Code.PreconditionFailed, Constants.Description.PreconditionFailed); // 412
        public static readonly IResultStatus UnprocessableEntity = new ResultStatus(Constants.Code.UnprocessableEntity, Constants.Description.UnprocessableEntity); // 422
        public static readonly IResultStatus TooManyRequests = new ResultStatus(Constants.Code.TooManyRequests, Constants.Description.TooManyRequests); // 429

        public static readonly IResultStatus Error = new ResultStatus(Constants.Code.InternalServerError, Constants.Description.InternalServerError); // 500
        public static readonly IResultStatus NotImplemented = new ResultStatus(Constants.Code.NotImplemented, Constants.Description.NotImplemented); // 501
        public static readonly IResultStatus ServiceUnavailable = new ResultStatus(Constants.Code.ServiceUnavailable, Constants.Description.ServiceUnavailable); // 503
    }
}
