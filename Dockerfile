FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env-9.0
WORKDIR /app
COPY . .
RUN dotnet restore "Source/Runtime/Manifest.Configuration/Zentient.Runtime.Manifest.Configuration.csproj"
RUN dotnet restore "Source/Runtime/Manifest.Abstractions/Zentient.Runtime.Manifest.Abstractions.csproj"
RUN dotnet restore "Source/Runtime/Manifest/Zentient.Runtime.Manifest.csproj"
RUN dotnet build -c Release "Source/Runtime/Manifest.Configuration/Zentient.Runtime.Manifest.Configuration.csproj"
RUN dotnet build -c Release "Source/Runtime/Manifest.Abstractions/Zentient.Runtime.Manifest.Abstractions.csproj"
RUN dotnet build -c Release "Source/Runtime/Manifest/Zentient.Runtime.Manifest.csproj"

FROM build-env-9.0 AS publish-env-9.0
WORKDIR /app
COPY --from=build-env-9.0 /app .
RUN dotnet publish -c Release "Source/Runtime/Manifest.Configuration/Zentient.Runtime.Manifest.Configuration.csproj" -o out/net6.0 -f net6.0 /p:PackageOutputPath=nupkgs
RUN dotnet publish -c Release "Source/Runtime/Manifest.Configuration/Zentient.Runtime.Manifest.Configuration.csproj" -o out/net7.0 -f net7.0 /p:PackageOutputPath=nupkgs
RUN dotnet publish -c Release "Source/Runtime/Manifest.Configuration/Zentient.Runtime.Manifest.Configuration.csproj" -o out/net8.0 -f net8.0 /p:PackageOutputPath=nupkgs
RUN dotnet publish -c Release "Source/Runtime/Manifest.Configuration/Zentient.Runtime.Manifest.Configuration.csproj" -o out/net9.0 -f net9.0 /p:PackageOutputPath=nupkgs

RUN dotnet publish -c Release "Source/Runtime/Manifest.Abstractions/Zentient.Runtime.Manifest.Abstractions.csproj" -o out/net6.0 -f net6.0 /p:PackageOutputPath=nupkgs
RUN dotnet publish -c Release "Source/Runtime/Manifest.Abstractions/Zentient.Runtime.Manifest.Abstractions.csproj" -o out/net7.0 -f net7.0 /p:PackageOutputPath=nupkgs
RUN dotnet publish -c Release "Source/Runtime/Manifest.Abstractions/Zentient.Runtime.Manifest.Abstractions.csproj" -o out/net8.0 -f net8.0 /p:PackageOutputPath=nupkgs
RUN dotnet publish -c Release "Source/Runtime/Manifest.Abstractions/Zentient.Runtime.Manifest.Abstractions.csproj" -o out/net9.0 -f net9.0 /p:PackageOutputPath=nupkgs

RUN dotnet publish -c Release "Source/Runtime/Manifest/Zentient.Runtime.Manifest.csproj" -o out/net8.0 -f net8.0 /p:PackageOutputPath=nupkgs
RUN dotnet publish -c Release "Source/Runtime/Manifest/Zentient.Runtime.Manifest.csproj" -o out/net9.0 -f net9.0 /p:PackageOutputPath=nupkgs

FROM alpine:latest
RUN apk add --no-cache ca-certificates
COPY --from=publish-env-9.0 /app/nupkgs /app/nupkgs
WORKDIR /app/nupkgs
