using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization; // For System.Text.Json compatibility

namespace Zentient.Results
{
    // --- System.Text.Json Converter for IResult and IResult<T> ---
    // This is crucial for proper serialization when Result/Result<T> are used in API responses.
    // This converter will ensure that only the necessary properties are serialized,
    // and it handles both success and failure states.
    public class ResultJsonConverter : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(IResult).IsAssignableFrom(typeToConvert);
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Result<>))
            {
                Type valueType = typeToConvert.GetGenericArguments()[0];
                return (JsonConverter)Activator.CreateInstance(
                    typeof(ResultGenericJsonConverter<>).MakeGenericType(valueType),
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public,
                    binder: null,
                    args: new object[] { options },
                    culture: null)!;
            }

            if (typeToConvert == typeof(Result))
            {
                return new ResultNonGenericJsonConverter(options);
            }

            return null;
        }

        private class ResultNonGenericJsonConverter : JsonConverter<Result>
        {
            private readonly JsonSerializerOptions _options;

            public ResultNonGenericJsonConverter(JsonSerializerOptions options)
            {
                // Create a new options instance to prevent infinite recursion
                // when serializing ErrorInfo or IResultStatus
                _options = new JsonSerializerOptions(options);
                _options.Converters.Remove(this); // Remove self to prevent loop
            }

            public override Result Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                // Deserialization for Result is generally less common for public APIs
                // but good to have a basic implementation.
                // For simplicity, this basic implementation might not fully reconstruct
                // complex scenarios or rely on default deserialization for properties.
                // A more robust implementation might require manual parsing of the JSON object.

                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected StartObject token.");
                }

                IResultStatus? status = null;
                List<ErrorInfo>? errors = null;
                List<string>? messages = null;

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString()!;
                        reader.Read(); // Move to property value

                        switch (propertyName)
                        {
                            case nameof(IResult.Status):
                                status = JsonSerializer.Deserialize<DefaultResultStatus>(ref reader, _options);
                                break;
                            case nameof(IResult.Errors):
                                errors = JsonSerializer.Deserialize<List<ErrorInfo>>(ref reader, _options);
                                break;
                            case nameof(IResult.Messages):
                                messages = JsonSerializer.Deserialize<List<string>>(ref reader, _options);
                                break;
                                // IsSuccess, IsFailure, Error are derived properties, no need to deserialize
                        }
                    }
                }

                if (status == null)
                {
                    // Fallback to default status if not present or something went wrong
                    // In a real scenario, you might want to throw or handle this more robustly.
                    status = ResultStatuses.Error;
                    errors ??= new List<ErrorInfo> { new ErrorInfo(ErrorCategory.General, "DeserializationError", "Could not determine result status during deserialization.") };
                }

                return new Result(status, messages, errors);
            }

            public override void Write(Utf8JsonWriter writer, Result value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                writer.WritePropertyName(nameof(IResult.IsSuccess));
                writer.WriteBooleanValue(value.IsSuccess);

                writer.WritePropertyName(nameof(IResult.IsFailure));
                writer.WriteBooleanValue(value.IsFailure);

                writer.WritePropertyName(nameof(IResult.Status));
                JsonSerializer.Serialize(writer, value.Status, value.Status.GetType(), _options); // Use specific type for status

                if (value.Messages.Any())
                {
                    writer.WritePropertyName(nameof(IResult.Messages));
                    JsonSerializer.Serialize(writer, value.Messages, _options);
                }

                if (value.Errors.Any())
                {
                    writer.WritePropertyName(nameof(IResult.Errors));
                    JsonSerializer.Serialize(writer, value.Errors, _options);
                }

                // Optionally serialize Error if desired, though Errors collection is primary
                if (!string.IsNullOrWhiteSpace(value.Error))
                {
                    writer.WritePropertyName(nameof(IResult.Error));
                    writer.WriteStringValue(value.Error);
                }

                writer.WriteEndObject();
            }
        }

        private class ResultGenericJsonConverter<T> : JsonConverter<Result<T>>
        {
            private readonly JsonSerializerOptions _options;

            public ResultGenericJsonConverter(JsonSerializerOptions options)
            {
                _options = new JsonSerializerOptions(options);
                _options.Converters.Remove(this);
            }

            public override Result<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected StartObject token.");
                }

                T? value = default;
                IResultStatus? status = null;
                List<ErrorInfo>? errors = null;
                List<string>? messages = null;
                bool valueFound = false; // To distinguish between null and absent 'Value'

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        break;
                    }

                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        string propertyName = reader.GetString()!;
                        reader.Read(); // Move to property value

                        switch (propertyName)
                        {
                            case nameof(IResult<T>.Value):
                                value = JsonSerializer.Deserialize<T>(ref reader, _options);
                                valueFound = true;
                                break;
                            case nameof(IResult.Status):
                                status = JsonSerializer.Deserialize<DefaultResultStatus>(ref reader, _options);
                                break;
                            case nameof(IResult.Errors):
                                errors = JsonSerializer.Deserialize<List<ErrorInfo>>(ref reader, _options);
                                break;
                            case nameof(IResult.Messages):
                                messages = JsonSerializer.Deserialize<List<string>>(ref reader, _options);
                                break;
                        }
                    }
                }

                if (status == null)
                {
                    status = ResultStatuses.Error;
                    errors ??= new List<ErrorInfo> { new ErrorInfo(ErrorCategory.General, "DeserializationError", "Could not determine result status during deserialization.") };
                }

                // If 'Value' was explicitly present and null, pass null. Otherwise, pass default(T).
                // This is a subtle point as default(T) is null for reference types.
                return new Result<T>(valueFound ? value : default, status, messages, errors);
            }

            public override void Write(Utf8JsonWriter writer, Result<T> value, JsonSerializerOptions options)
            {
                writer.WriteStartObject();

                writer.WritePropertyName(nameof(IResult.IsSuccess));
                writer.WriteBooleanValue(value.IsSuccess);

                writer.WritePropertyName(nameof(IResult.IsFailure));
                writer.WriteBooleanValue(value.IsFailure);

                writer.WritePropertyName(nameof(IResult.Status));
                JsonSerializer.Serialize(writer, value.Status, value.Status.GetType(), _options);

                if (value.Messages.Any())
                {
                    writer.WritePropertyName(nameof(IResult.Messages));
                    JsonSerializer.Serialize(writer, value.Messages, _options);
                }

                if (value.Errors.Any())
                {
                    writer.WritePropertyName(nameof(IResult.Errors));
                    JsonSerializer.Serialize(writer, value.Errors, _options);
                }
                else if (value.IsSuccess) // Only serialize Value if it's a success and there are no errors
                {
                    writer.WritePropertyName(nameof(IResult<T>.Value));
                    JsonSerializer.Serialize(writer, value.Value, _options);
                }

                if (!string.IsNullOrWhiteSpace(value.Error))
                {
                    writer.WritePropertyName(nameof(IResult.Error));
                    writer.WriteStringValue(value.Error);
                }

                writer.WriteEndObject();
            }
        }
    }
}
