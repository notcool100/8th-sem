FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the entire backend source code
COPY . .

# Restore dependencies
RUN dotnet restore "src/NtbEvent.Api/NtbEvent.Api.csproj"

# Build and publish the application
RUN dotnet publish "src/NtbEvent.Api/NtbEvent.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Build the runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy the published output from the build stage
COPY --from=build /app/publish .

# Expose the port Coolify will route to (typically ASP.NET Core 8 defaults to 8080 in containers)
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

ENTRYPOINT ["dotnet", "NtbEvent.Api.dll"]
