# Zentient.Results

[![NuGet Badge](https://buildstats.info/nuget/Zentient.Results)](https://www.nuget.org/packages/Zentient.Results)

A robust and flexible result object for .NET 9, providing a standardized way to represent the outcome of operations, whether successful or not. This library encourages explicit error handling and clear communication of operation status.

## Overview

`Zentient.Results` introduces the `IResult` and `IResult<TValue>` interfaces, along with concrete `Result` and `Result<T>` structs. These structures allow you to encapsulate the outcome of a function or operation, including:

* **Success or Failure:** Clearly indicates whether the operation succeeded.
* **Value (Generic Result):** Holds the result of a successful operation.
* **Errors:** A collection of detailed `ErrorInfo` objects for failures, including category, code, message, and optional data/inner errors.
* **Messages:** A collection of informational messages for successful operations.
* **Status:** An `IResultStatus` object providing a numerical code and description of the outcome (e.g., Success, BadRequest, NotFound).
* **Monadic Operations:** Includes `Map`, `Bind`, and `Tap` for composable operations.
* **Implicit Conversions:** Simplifies the creation of successful generic results.
* **JSON Serialization:** Built-in `System.Text.Json` converters for seamless API integration.

## Installation

You can install the `Zentient.Results` NuGet package using the .NET CLI:

```bash
dotnet add package Zentient.Results
````

Or using the NuGet Package Manager in Visual Studio.

## Key Features

### Clear Result Representation

The `Result` and `Result<T>` structs provide a clear and consistent way to represent operation outcomes, making your code more readable and maintainable.

```csharp
public IResult<User> GetUserById(int id)
{
    var user = _dbContext.Users.Find(id);
    if (user is null)
    {
        return Result<User>.NotFound(new ErrorInfo(ErrorCategory.NotFound, "UserNotFound", $"User with ID '{id}' not found."));
    }
    return Result<User>.Success(user);
}

public IResult UpdateUserProfile(User user)
{
    if (string.IsNullOrWhiteSpace(user.Email))
    {
        return Result.Validation(new[] { new ErrorInfo(ErrorCategory.Validation, "InvalidEmail", "Email cannot be empty.") });
    }
    _dbContext.Users.Update(user);
    _dbContext.SaveChanges();
    return Result.Success("User profile updated successfully.");
}
```

### Detailed Error Information

The `ErrorInfo` struct provides structured information about errors, making it easier to understand and handle failures.

```csharp
if (result.IsFailure)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error Category: {error.Category}, Code: {error.Code}, Message: {error.Message}");
        if (error.Data != null)
        {
            Console.WriteLine($"  Data: {error.Data}");
        }
        if (error.InnerErrors.Any())
        {
            Console.WriteLine("  Inner Errors:");
            foreach (var innerError in error.InnerErrors)
            {
                Console.WriteLine($"    Code: {innerError.Code}, Message: {innerError.Message}");
            }
        }
    }
}
```

### Predefined Result Statuses

The `ResultStatuses` static class offers a set of commonly used result statuses, promoting consistency across your application.

```csharp
return Result.Failure(new ErrorInfo(ErrorCategory.Authorization, "InsufficientPermissions", "User does not have required permissions."), ResultStatuses.Forbidden);
```

### Monadic Operations for Composition

`Zentient.Results` includes `Map` and `Bind` methods for `Result<T>`, enabling you to chain operations in a functional style while handling potential failures gracefully.

```csharp
public IResult<string> GetUserName(int userId) =>
    GetUserById(userId)
        .Map(user => user.Name);

public IResult<OrderDetails> GetOrderDetails(int orderId) =>
    GetOrder(orderId)
        .Bind(order => _orderService.FetchDetails(order));
```

### Seamless JSON Serialization

The library provides custom `System.Text.Json` converters (`ResultJsonConverter`) to ensure that `Result` and `Result<T>` objects are serialized correctly when used in ASP.NET Core APIs or other scenarios involving JSON serialization.

```csharp
[HttpGet("{id}")]
public ActionResult<Result<User>> Get(int id)
{
    var result = _userService.GetUserById(id);
    return result.IsSuccess ? Ok(result) : NotFound(result);
}
```

### Implicit Conversion

Creating successful generic results is simplified with implicit conversion from the value type `T`.

```csharp
public IResult<int> GetCount()
{
    int count = _dataService.CountItems();
    return count; // Implicitly converted to Result<int>.Success(count)
}
```

## Usage Examples

### Returning a Successful Result with a Value

```csharp
public IResult<Customer> GetCustomer(int id)
{
    var customer = _customerRepository.GetById(id);
    if (customer != null)
    {
        return Result<Customer>.Success(customer, "Customer retrieved successfully.");
    }
    return Result<Customer>.NotFound(new ErrorInfo(ErrorCategory.NotFound, "CustomerNotFound", $"Customer with ID '{id}' not found."));
}
```

### Returning a Failure Result with Multiple Validation Errors

```csharp
public IResult CreateOrder(OrderDto orderDto)
{
    var errors = new List<ErrorInfo>();
    if (orderDto.TotalAmount <= 0)
    {
        errors.Add(new ErrorInfo(ErrorCategory.Validation, "InvalidAmount", "Total amount must be greater than zero."));
    }
    if (string.IsNullOrWhiteSpace(orderDto.ShippingAddress))
    {
        errors.Add(new ErrorInfo(ErrorCategory.Validation, "MissingAddress", "Shipping address is required."));
    }

    if (errors.Any())
    {
        return Result.Validation(errors);
    }

    // Proceed with order creation
    var order = MapToOrder(orderDto);
    _orderRepository.Add(order);
    _orderRepository.SaveChanges();
    return Result.Success("Order created successfully.");
}
```

### Using Monadic Operations

```csharp
public IResult<string> GetCustomerEmail(int customerId) =>
    GetCustomer(customerId)
        .Bind(customer => _emailService.GetEmailForCustomer(customer.Id));

// Assume _emailService.GetEmailForCustomer returns IResult<string>
```

## Contribution Guidelines

Contributions to `Zentient.Results` are welcome\! Please ensure that your contributions:

  * Are compatible with .NET 9.
  * Include appropriate unit tests.
  * Follow the existing code style.
  * Clearly explain the purpose and benefits of your changes.

Feel free to submit pull requests or open issues for bug reports and feature requests.

## License

`Zentient.Results` is licensed under the [MIT License](https://www.google.com/search?q=LICENSE).
