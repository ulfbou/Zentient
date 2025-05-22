namespace Zentient.Results.AspNetCore
{
    /// <summary>Provides extension methods for <see cref="Dictionary{TKey, TValue}"/>.</summary>
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// Adds a model error to the dictionary. If the key already exists, the new error message is appended to the existing array of error messages.
        /// </summary>
        /// <param name="dictionary">The <see cref="Dictionary{TKey, TValue}"/> to add the error to, where the key is a string and the value is a string array.</param>
        /// <param name="key">The key representing the error (e.g., a property name).</param>
        /// <param name="value">The error message to add.</param>
        public static void AddModelError(this Dictionary<string, string[]> dictionary, string key, string value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = dictionary[key].Concat(new[] { value }).ToArray();
            }
            else
            {
                dictionary.Add(key, new[] { value });
            }
        }
    }
}
