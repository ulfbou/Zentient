# Zentient.Runtime.Manifest

The `Zentient.Runtime.Manifest` library provides powerful tools for managing localized templates and manifests in .NET applications. It offers an extensible design for resolving localized values, validating placeholder counts, and integrating configuration-based templates.

## Installation

To install the `Zentient.Runtime.Manifest` package, use the following command:

```bash
dotnet add package Zentient.Runtime.Manifest --version 0.1.0
```

## Key Components

The library includes the following key components:

### 1. `ILocalizedValueProvider`

An interface for resolving localized values based on a scope, key, culture, and optional arguments.

```csharp
public interface ILocalizedValueProvider
{
    string Resolve(string scope, string key, CultureInfo culture, params object[] args);
    string Resolve(string scope, string key, params object[] args);
    bool TryResolve(string scope, string key, out string value, CultureInfo? culture = null, params object[] args);
}
```

### 2. `ConfigurationTemplateProvider`

An implementation of `ILocalizedValueProvider` that retrieves localized templates from configuration with caching and validation support.

- **Features:**
  - Caching of resolved templates.
  - Validation of placeholder counts in templates.
  - Integration with `IConfiguration` for dynamic configuration-based templates.

```csharp
var provider = new ConfigurationTemplateProvider(configuration, logger, rootConfigKey: "Manifest", enforcePlaceholderCount: true);
```

### 3. `ConfigurationTemplateProviderOptions`

Options for configuring the behavior of `ConfigurationTemplateProvider`, including the root configuration key and whether to enforce placeholder count validation.

```csharp
var options = new ConfigurationTemplateProviderOptions
{
    RootConfigKey = "Manifest",
    EnforcePlaceholderCount = true
};
```

### 4. `ScopedTemplateAccessor<TEnumScope>`

An abstract class for accessing localized templates based on an enumeration. This allows strongly-typed access to templates within a specific scope.

```csharp
public abstract class ScopedTemplateAccessor<TEnumScope> where TEnumScope : Enum
{
    protected string GetValue(TEnumScope key, params object[] args);
    protected string GetValue(TEnumScope key, CultureInfo culture, params object[] args);
    protected bool TryGetValue(TEnumScope key, out string value, CultureInfo? culture = null, params object[] args);
}
```

### 5. `TemplateProviderServiceCollectionExtensions`

Extension methods for registering the `ConfigurationTemplateProvider` and related services in a dependency injection container.

```csharp
services.AddTemplateProviderServices(rootConfigKey: "Manifest", enforcePlaceholderCount: true);
services.AddTemplateProviderServices(options =>
{
    options.RootConfigKey = "Manifest";
    options.EnforcePlaceholderCount = true;
});
```

## Usage Examples

### Resolving a Localized Value

```csharp
var provider = new ConfigurationTemplateProvider(configuration, logger);
var localizedValue = provider.Resolve("ScopeName", "KeyName", CultureInfo.CurrentCulture, "arg1", "arg2");
```

### Registering Services in Dependency Injection

```csharp
services.AddTemplateProviderServices(options =>
{
    options.RootConfigKey = "Manifest";
    options.EnforcePlaceholderCount = true;
});
```

---

For additional details and advanced usage, refer to the source code in the repository.

# Zentient.Runtime.Manifest

The `Zentient.Runtime.Manifest` module is a core component of the `Zentient` project, providing utilities and tools for working with runtime manifests. It is written entirely in C# and designed to be highly modular and efficient for developers building .NET applications.

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Folder Structure](#folder-structure)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

## Overview

The `Zentient.Runtime.Manifest` module focuses on enabling developers to manage and handle runtime manifest data effectively. A manifest typically contains metadata about the application or its runtime behavior, and this module provides the tools to parse, manipulate, validate, and generate such manifests.

## Features

- **Manifest Parsing:** Read and interpret manifest files with ease.
- **Manifest Validation:** Ensure manifest data adheres to predefined schemas or rules.
- **Manifest Generation:** Create new manifests dynamically based on runtime requirements.
- **Integration-Ready:** Designed to integrate seamlessly with other parts of the `Zentient` project or external .NET applications.

## Folder Structure

The module is organized as follows:

```
Manifest/
├── [Core Code Files]
├── [Utilities]
├── [Schemas and Models]
```

This directory contains all the components necessary for implementing runtime manifest functionality.

## Getting Started

### Prerequisites

- .NET SDK installed on your system.
- A C# IDE such as Visual Studio, Rider, or VS Code with C# extensions.

### Installation

1. Clone the `Zentient` repository:
   ```bash
   git clone https://github.com/ulfbou/Zentient.git
   cd Zentient/Source/Runtime/Manifest
   ```

2. Open the project in your preferred IDE.

3. Build the solution to restore dependencies.

### Running the Module

To run or test the `Zentient.Runtime.Manifest` module, navigate to the `Manifest` directory and execute:
```bash
dotnet run
```

## Usage

The `Zentient.Runtime.Manifest` module provides an API for handling manifest functionalities. Below are some example use cases:

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

Refer to the source code for more detailed usage examples and API documentation.

## Contributing

Contributions are welcome! Follow these steps to contribute to the module:

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

## License

This project is licensed under the [MIT License](LICENSE.md).

---

For any questions or support, feel free to open an issue or start a discussion in the main repository!
