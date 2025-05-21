using System.Runtime.Serialization;
using System.Text.Json.Serialization; // For System.Text.Json compatibility

namespace Zentient.Results
{
    /// <summary>
    /// Defines the contract for a generic result, indicating success or failure with a value.
    /// </summary>
    /// <typeparam name="T">The type of the value held by the result.</typeparam>
    public interface IResult<T> : IResult
    {
        /// <summary>Gets the value if the operation was successful; otherwise, null.</summary>
        T? Value { get; }

        /// <summary>
        /// Gets the value if the result is successful, otherwise throws an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <returns>The encapsulated value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the result is a failure.</exception>
        T GetValueOrThrow();

        /// <summary>
        /// Gets the value if the result is successful, otherwise throws an <see cref="InvalidOperationException"/>
        /// with a custom message.
        /// </summary>
        /// <param name="message">The message for the <see cref="InvalidOperationException"/>.</param>
        /// <returns>The encapsulated value.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the result is a failure.</exception>
        T GetValueOrThrow(string message);

        /// <summary>
        /// Gets the value if the result is successful, otherwise throws an exception created by the provided factory.
        /// </summary>
        /// <param name="exceptionFactory">A function that returns the exception to throw.</param>
        /// <returns>The encapsulated value.</returns>
        /// <exception cref="Exception">The exception returned by the <paramref name="exceptionFactory"/> if the result is a failure.</exception>
        T GetValueOrThrow(Func<Exception> exceptionFactory);

        /// <summary>
        /// Gets the value if the result is successful and the value is not null, otherwise returns the specified fallback value.
        /// </summary>
        /// <param name="fallback">The fallback value to return if the result is a failure or the value is null.</param>
        /// <returns>The encapsulated value or the fallback value.</returns>
        T GetValueOrDefault(T fallback);

        // --- Monadic Operations ---

        /// <summary>
        /// Transforms the value of a successful result using a selector function.
        /// If the result is a failure, the failure is propagated without executing the selector.
        /// </summary>
        /// <typeparam name="U">The type of the new value.</typeparam>
        /// <param name="selector">The function to transform the value.</param>
        /// <returns>A new <see cref="IResult{U}"/> with the transformed value or the original errors.</returns>
        IResult<U> Map<U>(Func<T, U> selector);

        /// <summary>
        /// Chains another operation that returns a new <see cref="IResult{U}"/>.
        /// If the current result is a success, the binder function is executed.
        /// If the current result is a failure, the failure is propagated.
        /// </summary>
        /// <typeparam name="U">The type of the value in the new result.</typeparam>
        /// <param name="binder">The function to chain, which returns a new result.</param>
        /// <returns>A new <see cref="IResult{U}"/> from the binder or the original errors.</returns>
        IResult<U> Bind<U>(Func<T, IResult<U>> binder);

        /// <summary>
        /// Executes an action if the result is successful, then returns the original result.
        /// Useful for side effects without modifying the result.
        /// </summary>
        /// <param name="onSuccess">The action to execute.</param>
        /// <returns>The original <see cref="IResult{T}"/>.</returns>
        IResult<T> Tap(Action<T> onSuccess);

        /// <summary>
        /// Executes an action if the result is successful.
        /// </summary>
        /// <param name="action">The action to execute if the result is successful.</param>
        /// <returns>The original <see cref="IResult{T}"/>.</returns>
        IResult<T> OnSuccess(Action<T> action);

        /// <summary>
        /// Executes an action if the result is a failure.
        /// </summary>
        /// <param name="action">The action to execute if the result is a failure, receiving the list of errors.</param>
        /// <returns>The original <see cref="IResult{T}"/>.</returns>
        IResult<T> OnFailure(Action<IReadOnlyList<ErrorInfo>> action);

        /// <summary>
        /// Executes one of two functions based on whether the result is successful or a failure,
        /// and returns a new type <typeparamref name="U"/>.
        /// </summary>
        /// <typeparam name="U">The type of the value returned by the match operation.</typeparam>
        /// <param name="onSuccess">The function to execute if the result is successful.</param>
        /// <param name="onFailure">The function to execute if the result is a failure.</param>
        /// <returns>The result of either <paramref name="onSuccess"/> or <paramref name="onFailure"/>.</returns>
        U Match<U>(Func<T, U> onSuccess, Func<IReadOnlyList<ErrorInfo>, U> onFailure);
    }
}
