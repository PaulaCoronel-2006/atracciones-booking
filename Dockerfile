FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Microservicios.Atracciones.Booking.API/Microservicios.Atracciones.Booking.API.csproj", "Microservicios.Atracciones.Booking.API/"]
COPY ["Microservicios.Atracciones.Booking.Business/Microservicios.Atracciones.Booking.Business.csproj", "Microservicios.Atracciones.Booking.Business/"]
COPY ["Microservicios.Atracciones.Booking.DataAccess/Microservicios.Atracciones.Booking.DataAccess.csproj", "Microservicios.Atracciones.Booking.DataAccess/"]
COPY ["Microservicios.Atracciones.Booking.DataManagement/Microservicios.Atracciones.Booking.DataManagement.csproj", "Microservicios.Atracciones.Booking.DataManagement/"]

RUN dotnet restore "Microservicios.Atracciones.Booking.API/Microservicios.Atracciones.Booking.API.csproj"

COPY . .
WORKDIR "/src/Microservicios.Atracciones.Booking.API"
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Microservicios.Atracciones.Booking.API.dll"]
