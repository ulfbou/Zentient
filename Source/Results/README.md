
# Zentient.Results 0.1.0

**Lightweight Result Handling for .NET**

Zentient.Results is a compact and extensible .NET library designed to provide a robust alternative to traditional exception handling for representing the outcomes of operations. It encapsulates success or failure states, with optional values, detailed error information, and informational messages. Zentient.Results is ideal for developers seeking a functional programming style, streamlined error propagation, and clean business logic without over-reliance on exceptions for control flow.

---

## 🚀 Installation

Zentient.Results is distributed as a NuGet package.

**.NET CLI:**

```sh
dotnet add package Zentient.Results --version 0.1.0
```

**NuGet Package Manager Console:**

```ps
Install-Package Zentient.Results -Version 0.1.0
```

> **Prerequisite:**  
> This library targets **.NET 9.0**. Ensure your project is configured for .NET 9.0 or a compatible framework.

---

## ✨ Usage Examples

Zentient.Results offers both non-generic (`Result`) and generic (`Result<T>`) types for handling operations with or without return values.

### Basic Success and Failure

```csharp
using Zentient.Results;

// Non-generic Result
IResult operationResult = Result.Success("Operation completed successfully!");
if (operationResult.IsSuccess)
{
    Console.WriteLine(operationResult.Messages.FirstOrDefault()); // Output: Operation completed successfully!
}

operationResult = Result.Failure(new ErrorInfo(ErrorCategory.General, "E001", "Something went wrong."));
if (operationResult.IsFailure)
{
    Console.WriteLine(operationResult.Error); // Output: Something went wrong.
    Console.WriteLine(operationResult.Errors.First().Code); // Output: E001
}

// Generic Result<T>
IResult<int> calculateResult = Result.Success(42, "Calculation successful.");
if (calculateResult.IsSuccess)
{
    Console.WriteLine($"Value: {calculateResult.Value}, Message: {calculateResult.Messages.FirstOrDefault()}");
    // Output: Value: 42, Message: Calculation successful.
}

calculateResult = Result<int>.Failure(
    value: default, // Or some partial result if applicable
    error: new ErrorInfo(ErrorCategory.Validation, "V002", "Input was invalid."),
    status: ResultStatuses.BadRequest);

if (calculateResult.IsFailure)
{
    Console.WriteLine($"Error: {calculateResult.Error}, Status: {calculateResult.Status.Description}");
    // Output: Error: Input was invalid., Status: Bad Request
}
```

---

### Monadic Operations (for `Result<T>`)

Zentient.Results supports common monadic operations like `Map`, `Bind`, and `Tap`, enabling elegant chaining of operations.

```csharp
using Zentient.Results;

IResult<string> GetUserName(int userId)
{
    if (userId > 0)
        return Result<string>.Success($"User_{userId}");
    return Result<string>.Failure(default, new ErrorInfo(ErrorCategory.NotFound, "USER_NF", "User not found."));
}

IResult<int> GetUserAge(string userName)
{
    // Simulate fetching age based on user name
    if (userName.StartsWith("User_"))
        return Result<int>.Success(30);
    return Result<int>.Failure(default, new ErrorInfo(ErrorCategory.Validation, "INV_NM", "Invalid user name format."));
}

// Chaining operations with Map and Bind
IResult<string> result = GetUserName(123)
    .Map(name => name.ToUpper()) // Transforms the value if successful
    .Bind(upperName => GetUserAge(upperName).Map(age => $"{upperName} is {age} years old.")) // Chains another Result
    .Tap(finalMessage => Console.WriteLine($"Successfully processed: {finalMessage}")) // Performs a side effect on success
    .OnFailure(errors => Console.WriteLine($"Failed: {errors.First().Message}")); // Handles failure

// Example for a failed path
IResult<string> failedResult = GetUserName(0)
    .Map(name => name.ToUpper())
    .Bind(upperName => GetUserAge(upperName).Map(age => $"{upperName} is {age} years old."))
    .Tap(finalMessage => Console.WriteLine($"Successfully processed: {finalMessage}"))
    .OnFailure(errors => Console.WriteLine($"Failed: {errors.First().Message}")); // Output: Failed: User not found.
```

---

## 📚 API Reference Summary

- `IResult`: The non-generic interface for operation outcomes. Provides `IsSuccess`, `IsFailure`, `Errors`, `Messages`, `Error`, and `Status`.
- `IResult<T>`: The generic interface for operations that return a value. Extends `IResult` and adds `Value`, as well as monadic methods (`Map`, `Bind`, `Tap`, `OnSuccess`, `OnFailure`, `Match`, `GetValueOrThrow()`, and `GetValueOrDefault()`).
- `Result`: The non-generic struct implementation of `IResult`, offering static factory methods for creating success and various failure types (e.g., `Success()`, `Failure()`, `NotFound()`, `Validation()`).
- `Result<T>`: The generic struct implementation of `IResult<T>`, providing similar static factory methods and implementation of monadic operations.
- `ErrorInfo`: A struct representing detailed error information, including `Category` (e.g., Validation, NotFound), `Code`, `Message`, `Data`, and `InnerErrors`.
- `ErrorCategory`: An enum with strongly-typed categories for common error types.
- `IResultStatus`: An interface representing the status of a result (e.g., HTTP-like status codes).
- `DefaultResultStatus`: A default implementation of `IResultStatus`.
- `ResultStatuses`: A static class providing predefined common result statuses (e.g., `Success`, `BadRequest`, `NotFound`, `InternalServerError`).
- `ResultJsonConverter`: A `System.Text.Json` converter for correct serialization of `Result` and `Result<T>` types in JSON payloads—particularly useful for API responses.

---

## 📝 License

This project is licensed under the MIT License. See the [LICENSE](./LICENSE) file for full details.

---

## 🤝 Contributing

Contributions are welcome! If you have suggestions, new features, or bug reports, please open an issue or submit a pull request on the [GitHub repository](https://github.com/ulfbou/Zentient).

---

> **Zentient.Results** — Write explicit, robust, and maintainable result handling for modern .NET.
