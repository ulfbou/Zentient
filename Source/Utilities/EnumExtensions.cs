namespace Zentient.Utilities
{
    /// <summary>Provides extension methods for working with enums.</summary>
    public static class EnumExtensions
    {
        /// <summary>Converts an enum value to its string representation.</summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The enum value to convert.</param>
        /// <returns>The string representation of the enum value.</returns>
        public static string ToString<T>(this T value) where T : Enum
        {
            return value.ToString();
        }
    }
}
