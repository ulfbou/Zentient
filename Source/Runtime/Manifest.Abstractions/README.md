## Zentient.Runtime.Manifest.Abstractions

**Zentient.Runtime.Manifest.Abstractions** defines the core contracts used across the Zentient.Runtime.Manifest ecosystem. It is intended as a minimal dependency for consumers who want to implement or integrate custom localization mechanisms.

### Included Interfaces

- `ILocalizedValueProvider`  
  Resolves localized templates by key and culture.
- `IScopedTemplateAccessor<TEnumScope>`  
  Contract for strongly typed, enum-based scoped accessors.

### Installation

```bash
dotnet add package Zentient.Runtime.Manifest.Abstractions
```

### Use Case

Use this package when:
- Building custom template providers (e.g., from a database or external API)
- Sharing contracts between internal libraries without implementation dependencies

### Example

```csharp
public class MyCustomTemplateProvider : ILocalizedValueProvider
{
    public string GetValue(string key, CultureInfo? culture = null, params object[] args)
    {
        // Custom logic here
    }

    public bool TryGetValue(string key, out string value, CultureInfo? culture = null, params object[] args)
    {
        // Custom logic here
    }
}
```

### Compatibility

This package contains no implementation logic and has no external runtime dependencies beyond .NET Standard 2.0+.
