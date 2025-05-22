namespace Zentient.Results
{
    /// <summary>
    /// A default implementation of <see cref="IResultStatus"/> with a code and description.
    /// </summary>
    public readonly struct DefaultResultStatus : IResultStatus
    {
        /// <summary>Gets the numerical code for the result status.</summary>
        public int Code { get; }
        /// <summary>Gets a human-readable description for the result status.</summary>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultResultStatus"/> struct.
        /// </summary>
        /// <param name="code">The numerical code.</param>
        /// <param name="description">The human-readable description.</param>
        public DefaultResultStatus(int code, string description)
        {
            Code = code;
            Description = description;
        }

        /// <summary>
        /// Creates a custom <see cref="DefaultResultStatus"/> instance.
        /// </summary>
        /// <param name="code">The numerical code for the custom status.</param>
        /// <param name="description">The description for the custom status.</param>
        /// <returns>A new <see cref="DefaultResultStatus"/> instance.</returns>
        public static DefaultResultStatus Custom(int code, string description) => new(code, description);

        /// <summary>
        /// Returns a string representation of the result status.
        /// </summary>
        /// <returns>A string in the format "(Code) Description".</returns>
        public override string ToString() => $"({Code}) {Description}";
    }
}
