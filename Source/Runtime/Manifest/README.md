
# Zentient.Runtime.Manifest

**Zentient.Runtime.Manifest** provides a streamlined and strongly typed approach to managing localized templates in .NET applications. Using enums as template keys and scopes, this package enables lightweight multi-language support through configuration and modern dependency injection practices.

## Overview

This package offers:

- **Scoped Template Access:** Create enum-based accessors for resolving localized templates.
- **Lightweight Localization:** Retrieve templates based on the current UI culture or a specified culture.
- **Flexible Configuration:** Supports appsettings-based templates with optional placeholder validation.
- **Extensibility:** Core functionality only—implementations like `ConfigurationTemplateProvider` and related options are defined in `Zentient.Runtime.Manifest.Configuration`. Shared interfaces (e.g., `ILocalizedValueProvider`) are defined in `Zentient.Runtime.Manifest.Abstractions`.

> **Note:** This library offers lightweight template localization. It is not a replacement for .resx, `ResourceManager`, or `IStringLocalizer`.

## Prerequisites

- **.NET 6.0 or later**
- **Microsoft.Extensions.*** libraries for configuration, logging, and dependency injection

## Installation

Install using either NuGet Package Manager or the .NET CLI:

```bash
dotnet add package Zentient.Runtime.Manifest
```

To use the configuration-based implementation:

```bash
dotnet add package Zentient.Runtime.Manifest.Configuration
```

> The core abstractions are included in:

```bash
dotnet add package Zentient.Runtime.Manifest.Abstractions
```

## Usage

### Service Registration

Add template provider services using one of the available overloads:

```csharp
using Zentient.Runtime.Manifest;
using Zentient.Runtime.Manifest.Configuration;

services.AddTemplateProviderServices(); // Uses "Manifest" section by default

services.AddTemplateProviderServices("CustomManifest", enforcePlaceholderCount: false);

services.AddTemplateProviderServices(options =>
{
    options.RootConfigKey = "CustomManifest";
    options.EnforcePlaceholderCount = true;
});
```

This registers an `ILocalizedValueProvider` implementation that resolves templates from configuration.

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

`GetValue(...)` uses `CultureInfo.CurrentUICulture` if no culture is specified. Keys are resolved via `ToString()` on the enum. Ensure configuration keys match.

### Configuration Example

In `appsettings.json`:

```json
{
  "Manifest": {
    "Welcome": "Welcome, {0}!",
    "PasswordReset": "Click here to reset your password, {0}.",
    "Notification": "Hello, {0}. You have a new notification."
  }
}
```

If you use a custom configuration root, reflect that in both your settings and registration.

### Placeholder Enforcement

If `EnforcePlaceholderCount = true`, the template provider ensures that the number of `{0}`, `{1}`, etc., placeholders matches the number of provided arguments. An exception is thrown if the count does not match.

## Status

Prototype – Pre-Release  
Initial version intended for internal use or early adopters. APIs may change.

## Contributing

Bug reports, discussions, and feature suggestions are welcome!  
Please see [CONTRIBUTING.md](./CONTRIBUTING.md) and [CODE_OF_CONDUCT.md](./CODE_OF_CONDUCT.md).

## License

MIT – see [LICENSE](./LICENSE)

## Resources

- **Documentation**
- **GitHub Issues**

---

Streamline multilingual templates in .NET—fast, lightweight, and strongly typed.