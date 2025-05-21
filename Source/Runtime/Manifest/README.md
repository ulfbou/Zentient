# Zentient.Runtime.Manifest

**Zentient.Runtime.Manifest** is a powerful library designed to manage localized templates and manifests in .NET applications. It provides tools for resolving localized values, validating placeholder counts, and integrating configuration-based templates. The library is lightweight, extensible, and designed to seamlessly integrate into .NET projects.

## Features

- **Scoped Template Access**: Use enums for strongly-typed access to localized templates.
- **Lightweight Localization**: Retrieve templates based on the current UI culture or a specified culture.
- **Configuration-Based Templates**: Supports appsettings-based templates with optional placeholder validation.
- **Extensibility**: Core abstractions and extensible implementations such as `ConfigurationTemplateProvider`.
- **Manifest Management**: Parse, validate, and generate manifests dynamically based on runtime requirements.

## Installation

Install the core library using the .NET CLI:

```bash
dotnet add package Zentient.Runtime.Manifest --version 0.1.0
```

For configuration-based functionality, add:

```bash
dotnet add package Zentient.Runtime.Manifest.Configuration
```

For core abstractions only, use:

```bash
dotnet add package Zentient.Runtime.Manifest.Abstractions
```

## Getting Started

### Prerequisites

- **.NET 6.0 or later**
- **Microsoft.Extensions.*** libraries for configuration, logging, and dependency injection

### Service Registration

Register the template provider services in your `Startup` class or `Program.cs`:

```csharp
using Zentient.Runtime.Manifest;
using Zentient.Runtime.Manifest.Configuration;

services.AddTemplateProviderServices(); // Default configuration section: "Manifest"

services.AddTemplateProviderServices("CustomManifest", enforcePlaceholderCount: false);

services.AddTemplateProviderServices(options =>
{
    options.RootConfigKey = "CustomManifest";
    options.EnforcePlaceholderCount = true;
});
```

This registers an `ILocalizedValueProvider` for resolving templates from configuration.

## Usage Examples

### Resolving a Localized Value

```csharp
var provider = new ConfigurationTemplateProvider(configuration, logger);
var localizedValue = provider.Resolve("ScopeName", "KeyName", CultureInfo.CurrentCulture, "arg1", "arg2");
Console.WriteLine(localizedValue);
```

### Creating a Scoped Template Accessor

```csharp
public enum EmailTemplates
{
    Welcome,
    PasswordReset,
    Notification
}

public class EmailTemplateAccessor : ScopedTemplateAccessor<EmailTemplates>
{
    public EmailTemplateAccessor(ILocalizedValueProvider provider) : base(provider) { }

    public string GetWelcomeTemplate(params object[] args) =>
        GetValue(EmailTemplates.Welcome, args);

    public bool TryGetNotificationTemplate(out string template, CultureInfo culture, params object[] args) =>
        TryGetValue(EmailTemplates.Notification, out template, culture, args);
}
```

### Configuration Example

Add your templates to `appsettings.json`:

```json
{
  "Manifest": {
    "Welcome": "Welcome, {0}!",
    "PasswordReset": "Click here to reset your password, {0}.",
    "Notification": "Hello, {0}. You have a new notification."
  }
}
```

If using a custom root key, update the configuration section accordingly.

### Placeholder Enforcement

When `EnforcePlaceholderCount = true`, the library ensures the number of placeholders (`{0}`, `{1}`, etc.) matches the number of provided arguments. If the count does not match, an exception is thrown.

## Advanced Manifest Management

In addition to template localization, `Zentient.Runtime.Manifest` provides tools to handle runtime manifests effectively:

1. **Parsing a Manifest:**
   ```csharp
   var manifest = ManifestParser.Parse("path/to/manifest.json");
   Console.WriteLine(manifest.Name);
   ```

2. **Validating a Manifest:**
   ```csharp
   bool isValid = ManifestValidator.Validate(manifest);
   Console.WriteLine($"Manifest is valid: {isValid}");
   ```

3. **Generating a Manifest:**
   ```csharp
   var newManifest = ManifestGenerator.Create("MyApp", "1.0.0");
   ManifestWriter.Write("path/to/manifest.json", newManifest);
   ```

## Status

**Prototype – Pre-Release**  
This library is intended for internal use or early adopters. APIs may change in future versions.

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository.
2. Create a feature branch:
   ```bash
   git checkout -b feature-name
   ```
3. Commit your changes:
   ```bash
   git commit -m "Add feature name"
   ```
4. Push to your forked repository and create a pull request.

For more details, see [CONTRIBUTING.md](./CONTRIBUTING.md) and [CODE_OF_CONDUCT.md](./CODE_OF_CONDUCT.md).

## License

This project is licensed under the [MIT License](./LICENSE).

---

Streamline multilingual templates and manifests in .NET with **Zentient.Runtime.Manifest**—fast, lightweight, and strongly typed.
