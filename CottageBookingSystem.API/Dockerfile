
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CottageBookingSystem.API/CottageBookingSystem.API.csproj", "CottageBookingSystem.API/"]
COPY ["CottageBookingSystem.Core/CottageBookingSystem.Core.csproj", "CottageBookingSystem.Core/"]
COPY ["CottageBookingSystem.Data/CottageBookingSystem.Data.csproj", "CottageBookingSystem.Data/"]
COPY ["CottageBookingSystem.Service/CottageBookingSystem.Service.csproj", "CottageBookingSystem.Service/"]

RUN dotnet restore "CottageBookingSystem.API/CottageBookingSystem.API.csproj"

COPY . .

RUN dotnet publish "CottageBookingSystem.API/CottageBookingSystem.API.csproj" \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 10000

ENTRYPOINT ["dotnet", "CottageBookingSystem.API.dll"]
