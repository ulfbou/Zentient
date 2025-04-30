using System.Globalization;

namespace Zentient.Runtime.Manifest
{
    /// <summary>
    /// Provides scoped access to localized templates based on an enumeration.
    /// </summary>
    /// <typeparam name="TEnumScope">The enumeration type representing the scope.</typeparam>
    public abstract class ScopedTemplateAccessor<TEnumScope> where TEnumScope : Enum
    {
        private readonly ILocalizedValueProvider _localizedValueProvider;
        private readonly string _scopeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopedTemplateAccessor{TEnumScope}"/> class.
        /// </summary>
        /// <param name="localizedValueProvider">The localized value provider.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="localizedValueProvider"/> is null.</exception>
        protected ScopedTemplateAccessor(ILocalizedValueProvider localizedValueProvider)
        {
            _localizedValueProvider = localizedValueProvider ?? throw new ArgumentNullException(nameof(localizedValueProvider));
            _scopeName = typeof(TEnumScope).Name;
        }

        /// <summary>
        /// Gets a localized value for the specified key and arguments.
        /// </summary>
        /// <param name="key">The key of the template.</param>
        /// <param name="args">Optional arguments to format the template.</param>
        /// <returns>The resolved localized value.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is not a valid value of the enumeration.</exception>
        protected string GetValue(TEnumScope key, params object[] args)
        {
            if (!Enum.IsDefined(typeof(TEnumScope), key))
            {
                throw new ArgumentException($"The key '{key}' is not a valid value of the enumeration '{typeof(TEnumScope).Name}'.", nameof(key));
            }

            args ??= Array.Empty<object>();
            return _localizedValueProvider.Resolve(_scopeName, key.ToString(), args);
        }

        /// <summary>
        /// Gets a localized value for the specified key, culture, and arguments.
        /// </summary>
        /// <param name="key">The key of the template.</param>
        /// <param name="culture">The culture for localization.</param>
        /// <param name="args">Optional arguments to format the template.</param>
        /// <returns>The resolved localized value.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="culture"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is not a valid value of the enumeration.</exception>
        protected string GetValue(TEnumScope key, CultureInfo culture, params object[] args)
        {
            if (!Enum.IsDefined(typeof(TEnumScope), key))
            {
                throw new ArgumentException($"The key '{key}' is not a valid value of the enumeration '{typeof(TEnumScope).Name}'.", nameof(key));
            }

            ArgumentNullException.ThrowIfNull(culture);
            args ??= Array.Empty<object>();
            return _localizedValueProvider.Resolve(_scopeName, key.ToString(), culture, args);
        }

        /// <summary>
        /// Attempts to get a localized value for the specified key, culture, and arguments.
        /// </summary>
        /// <param name="key">The key of the template.</param>
        /// <param name="value">The resolved value, if found.</param>
        /// <param name="culture">The culture for localization. Defaults to the current culture.</param>
        /// <param name="args">Optional arguments to format the template.</param>
        /// <returns><c>true</c> if the value was resolved; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is not a valid value of the enumeration.</exception>
        protected bool TryGetValue(TEnumScope key, out string value, CultureInfo? culture = null, params object[] args)
        {
            if (!Enum.IsDefined(typeof(TEnumScope), key))
            {
                throw new ArgumentException($"The key '{key}' is not a valid value of the enumeration '{typeof(TEnumScope).Name}'.", nameof(key));
            }

            args ??= Array.Empty<object>();
            return _localizedValueProvider.TryResolve(_scopeName, key.ToString(), out value, culture, args);
        }
    }
}
