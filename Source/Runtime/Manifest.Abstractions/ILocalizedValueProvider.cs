using System.Globalization;

namespace Zentient.Runtime.Manifest
{
    /// <summary>
    /// Provides methods to resolve localized values for a given scope and key.
    /// </summary>
    public interface ILocalizedValueProvider
    {
        /// <summary>
        /// Resolves a localized value for the specified scope, key, culture, and arguments.
        /// </summary>
        /// <param name="scope">The scope of the localization, typically representing a logical grouping.</param>
        /// <param name="key">The key identifying the localized value within the scope.</param>
        /// <param name="culture">The culture to use for localization.</param>
        /// <param name="args">Optional arguments to format the localized value.</param>
        /// <returns>The resolved localized value.</returns>
        string Resolve(string scope, string key, CultureInfo culture, params object[] args);

        /// <summary>
        /// Resolves a localized value for the specified scope, key, and arguments using the default culture.
        /// </summary>
        /// <param name="scope">The scope of the localization, typically representing a logical grouping.</param>
        /// <param name="key">The key identifying the localized value within the scope.</param>
        /// <param name="args">Optional arguments to format the localized value.</param>
        /// <returns>The resolved localized value.</returns>
        string Resolve(string scope, string key, params object[] args);

        /// <summary>
        /// Attempts to resolve a localized value for the specified scope, key, culture, and arguments.
        /// </summary>
        /// <param name="scope">The scope of the localization, typically representing a logical grouping.</param>
        /// <param name="key">The key identifying the localized value within the scope.</param>
        /// <param name="value">When this method returns, contains the resolved localized value if the resolution was successful; otherwise, <c>null</c>.</param>
        /// <param name="culture">The culture to use for localization. Defaults to the current culture if not specified.</param>
        /// <param name="args">Optional arguments to format the localized value.</param>
        /// <returns><c>true</c> if the localized value was successfully resolved; otherwise, <c>false</c>.</returns>
        bool TryResolve(string scope, string key, out string value, CultureInfo? culture = null, params object[] args);
    }
}
