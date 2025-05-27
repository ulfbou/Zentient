namespace Zentient.Results.Tests.Helpers
{
    internal class FakeResultStatus : IResultStatus
    {
        public FakeResultStatus(int code, string desc) { Code = code; Description = desc; }
        public int Code { get; }
        public string Description { get; }
    }
}
