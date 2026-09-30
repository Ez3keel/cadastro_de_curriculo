FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Curriculos.sln .
COPY src/Curriculos.Api/Curriculos.Api.csproj src/Curriculos.Api/
COPY tests/Curriculos.Api.Tests/Curriculos.Api.Tests.csproj tests/Curriculos.Api.Tests/
RUN dotnet restore Curriculos.sln

COPY . .
RUN dotnet publish src/Curriculos.Api/Curriculos.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Curriculos.Api.dll"]
