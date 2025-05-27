using System.Net;

namespace Zentient.Results.Tests.Helpers
{
    internal class FakeSuccessResultWithObject : IResult<object>
    {
        public FakeSuccessResultWithObject(object? value) => Value = value;
        public object? Value { get; }
        public object GetValueOrThrow() => Value!;
        public object GetValueOrThrow(string message) => Value!;
        public object GetValueOrThrow(Func<Exception> exceptionFactory) => Value!;
        public object GetValueOrDefault(object fallback) => Value ?? fallback;
        public IResult<U> Map<U>(Func<object, U> selector) => throw new NotImplementedException();
        public IResult<U> Bind<U>(Func<object, IResult<U>> binder) => throw new NotImplementedException();
        public IResult<object> Tap(Action<object> onSuccess) => this;
        public IResult<object> OnSuccess(Action<object> action) => this;
        public IResult<object> OnFailure(Action<IReadOnlyList<ErrorInfo>> action) => this;
        public U Match<U>(Func<object, U> onSuccess, Func<IReadOnlyList<ErrorInfo>, U> onFailure) => onSuccess(Value!);
        public bool IsSuccess => true;
        public bool IsFailure => false;
        public IReadOnlyList<ErrorInfo> Errors => Array.Empty<ErrorInfo>();
        public IReadOnlyList<string> Messages => Array.Empty<string>();
        public string? Error => null;
        public IResultStatus Status { get; } = new FakeResultStatus((int)HttpStatusCode.OK, "OK");
    }
}
