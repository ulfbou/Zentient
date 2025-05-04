// (C) 2025 Ulf Bourelius. All rights reserved.
// MIT License. See LICENSE file in the project root for full license information.

namespace Zentient.Definitions.Core
{
    public class DefinitionLoadException : Exception
    {
        public DefinitionLoadException(string message) : base(message) { }
        public DefinitionLoadException(string message, Exception innerException) : base(message, innerException) { }
    }
}
