using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zentient.Configuration
{
    public interface IAutoConfiguredOption<TItem, TKey>
        where TItem : IConfigurable<TKey>
        where TKey : IEquatable<TKey>
    {
        static abstract string ConfigKey { get; }
    }
    public interface IOptionSettings<TItem, TKey>
        where TItem : IConfigurable<TKey>
        where TKey : IEquatable<TKey>
    {
        string DisplayName { get; init; }
        Dictionary<string, object> Metadata { get; init; }
    }
    public interface IConfigurable<TKey>
        where TKey : IEquatable<TKey>
    {
        TKey Id { get; }
    }
    public static class OptionSettingsRegistrationExtensions
    {
        // ...

        static void Register<TItem, TKey>(IServiceCollection services, IConfiguration config)
            where TItem : IConfigurable<TKey>, IAutoConfiguredOption<TItem, TKey>
            where TKey : IEquatable<TKey>
        {
            var sectionPath = $"OptionSettings:{TItem.ConfigKey}";
            var section = config.GetSection(sectionPath);
            if (!section.Exists())
            {
                throw new InvalidOperationException($"Missing configuration for {typeof(TItem).Name} at '{sectionPath}'");
            }

            // Use OptionsBuilder to register options with named support
            //services.Configure<IOptionSettings<TItem, TKey>>(TItem.ConfigKey, section);
            //services.PostConfigure<IOptionSettings<TItem, TKey>>(TItem.ConfigKey, options =>
            //{
            //    // Optionally validate data annotations here if needed
            //    var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(options);
            //    System.ComponentModel.DataAnnotations.Validator.ValidateObject(options, validationContext, validateAllProperties: true);
            //});
            //// Use OptionsBuilder to register options with named support
            //services.AddOptions<IOptionSettings<TItem, TKey>>(TItem.ConfigKey)
            //    .Bind(section)
            //    .ValidateDataAnnotations();
        }


    }
}