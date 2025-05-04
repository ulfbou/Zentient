## Zentient.Runtime.Manifest.Configuration

**Zentient.Runtime.Manifest.Configuration** provides a configuration-backed implementation of `ILocalizedValueProvider` for the [Zentient.Runtime.Manifest](https://www.nuget.org/packages/Zentient.Runtime.Manifest) system. Templates are resolved from standard .NET configuration sources such as `appsettings.json`, making it ideal for environment-specific deployments.

### Features

- Implements `ILocalizedValueProvider` via `ConfigurationTemplateProvider`
- Supports culture-specific resolution using `CultureInfo.CurrentUICulture` or an explicit culture
- Optional placeholder count enforcement for formatted strings

### Installation

```bash
dotnet add package Zentient.Runtime.Manifest.Configuration
```

### Usage

In your startup or DI registration:

```csharp
using Zentient.Runtime.Manifest;
using Zentient.Runtime.Manifest.Configuration;

services.AddTemplateProviderServices(); // Uses "Manifest" section by default
```

Customize configuration:

```csharp
services.AddTemplateProviderServices(options =>
{
    options.RootConfigKey = "MyTemplates";
    options.EnforcePlaceholderCount = true;
});
```

### Configuration Example

```json
{
  "MyTemplates": {
    "Welcome": "Welcome, {0}!",
    "Farewell": "Goodbye, {0}."
  }
}
```

### Stability

**Prototype – Use with caution in production environments.**  
API surface may evolve based on user feedback and wider usage.
