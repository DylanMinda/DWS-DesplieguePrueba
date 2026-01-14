FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar todos los archivos .csproj para restaurar
COPY ["DWS/DWS.csproj", "DWS/"]
COPY ["MedIQ-API/MedIQ-API.csproj", "MedIQ-API/"]
COPY ["MedIQ-Modelos/MedIQ-Modelos.csproj", "MedIQ-Modelos/"]
RUN dotnet restore "DWS/DWS.csproj"

# Copiar todo el código y compilar
COPY . .
WORKDIR "/src/DWS"
RUN dotnet publish "DWS.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV PORT=10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "DWS.dll"]