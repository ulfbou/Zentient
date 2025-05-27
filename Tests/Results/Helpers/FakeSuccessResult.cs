using System.Net;

namespace Zentient.Results.Tests.Helpers
{
    internal class FakeSuccessResult : IResult
    {
        public bool IsSuccess => true;
        public bool IsFailure => false;
        public IReadOnlyList<ErrorInfo> Errors => Array.Empty<ErrorInfo>();
        public IReadOnlyList<string> Messages => Array.Empty<string>();
        public string? Error => null;
        public IResultStatus Status { get; } = new FakeResultStatus((int)HttpStatusCode.NoContent, "No Content");
    }
}
