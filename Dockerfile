FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["APM.API.csproj", "."]
RUN dotnet restore "./APM.API.csproj"
COPY . .
RUN dotnet build "./APM.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./APM.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "APM.API.dll"]