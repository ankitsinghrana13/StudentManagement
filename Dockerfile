# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["StudentManagement/StudentManagement.csproj", "StudentManagement/"]
COPY ["StudentManagement.Application/StudentManagement.Application.csproj", "StudentManagement.Application/"]
COPY ["StudentManagement.Core/StudentManagement.Core.csproj", "StudentManagement.Core/"]
COPY ["StudentManagement.Infrastructure/StudentManagement.Infrastructure.csproj", "StudentManagement.Infrastructure/"]

RUN dotnet restore "StudentManagement/StudentManagement.csproj"

COPY . .
WORKDIR "/src/StudentManagement"
RUN dotnet publish "StudentManagement.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "StudentManagement.dll"]
