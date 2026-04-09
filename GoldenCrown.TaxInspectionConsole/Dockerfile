FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["GoldenCrown.TaxInspectionConsole/*.csproj", "./GoldenCrown.TaxInspectionConsole/"]
RUN dotnet restore "./GoldenCrown.TaxInspectionConsole/GoldenCrown.TaxInspectionConsole.csproj"
COPY . .
RUN dotnet build "./GoldenCrown.TaxInspectionConsole/GoldenCrown.TaxInspectionConsole.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./GoldenCrown.TaxInspectionConsole/GoldenCrown.TaxInspectionConsole.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GoldenCrown.TaxInspectionConsole.dll"]
