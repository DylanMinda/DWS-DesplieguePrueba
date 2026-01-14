FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Copiar archivos de proyecto
COPY ["DWS/DWS.csproj", "DWS/"]
COPY ["MedIQ-API/MedIQ-API.csproj", "MedIQ-API/"]
COPY ["MedIQ-Modelos/MedIQ-Modelos.csproj", "MedIQ-Modelos/"]

# 2. Restaurar dependencias
RUN dotnet restore "DWS/DWS.csproj"

# 3. Copiar todo el código
COPY . .

# 4. TRUCO PARA EVITAR EL ERROR: 
# Borramos los appsettings de los otros proyectos para que no choquen con DWS
RUN rm MedIQ-API/appsettings.json MedIQ-API/appsettings.Development.json || true

# 5. Compilar y publicar
WORKDIR "/src/DWS"
RUN dotnet publish "DWS.csproj" -c Release -o /app/publish /p:ErrorOnDuplicatePublishOutputFiles=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "DWS.dll"]