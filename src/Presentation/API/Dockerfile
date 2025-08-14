# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Presentation/API/API.csproj", "API/"]
COPY ["Core/Application/Application.csproj", "Application/"]
COPY ["Infrastructure/Persistence/Persistence.csproj", "Infrastructure/"]
COPY ["Core/Domain/Domain.csproj", "Domain/"]

RUN dotnet restore "API/API.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src"
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]
