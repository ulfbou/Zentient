// Zentient/Definitions/Core/DefinitionLoadException.cs

namespace Zentient.Definitions.Core
{
    public class DefinitionLoadException : Exception
    {
        public DefinitionLoadException(string message) : base(message) { }
        public DefinitionLoadException(string message, Exception innerException) : base(message, innerException) { }
    }
}
