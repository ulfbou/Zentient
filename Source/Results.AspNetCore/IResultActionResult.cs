using Microsoft.AspNetCore.Mvc;

namespace Zentient.Results.AspNetCore
{
    /// <summary>
    /// An internal interface to help the filter identify Zentient.Results
    /// when they are wrapped in a custom <see cref="IActionResult"/>.
    /// This is an advanced pattern if you want to define specific <see cref="IActionResult"/>
    /// types for <see cref="Zentient.Results.IResult"/>.
    /// </summary>
    internal interface IResultActionResult : IActionResult
    {
        /// <summary>
        /// Gets the underlying <see cref="Zentient.Results.IResult"/> instance
        /// that this <see cref="IActionResult"/> wraps.
        /// </summary>
        Zentient.Results.IResult Result { get; }
    }
}