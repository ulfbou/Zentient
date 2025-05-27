namespace Zentient.Results.Tests.Helpers
{
    internal class FakeFailureResult : IResult
    {
        public bool IsSuccess => false;
        public bool IsFailure => true;
        public IReadOnlyList<ErrorInfo> Errors => new[] { new ErrorInfo(ErrorCategory.General, "ERR", "Failure") };
        public IReadOnlyList<string> Messages => new[] { "Failure" };
        public string? Error => "Failure";
        public IResultStatus Status { get; } = new FakeResultStatus(500, "Internal Server Error");
    }
}
