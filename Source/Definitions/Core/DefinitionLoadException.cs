// (C) 2025 Ulf Bourelius. All rights reserved.
// MIT License. See LICENSE file in the project root for full license information.

namespace Zentient.Definitions.Core
{
    /// <summary>
    /// Represents an exception that occurs when a definition fails to load.
    /// </summary>
    public class DefinitionLoadException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionLoadException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DefinitionLoadException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinitionLoadException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public DefinitionLoadException(string message, Exception innerException) : base(message, innerException) { }
    }
}
