FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY RepairTracker/RepairTracker.csproj RepairTracker/
RUN dotnet restore RepairTracker/RepairTracker.csproj

COPY RepairTracker/ RepairTracker/
RUN dotnet publish RepairTracker/RepairTracker.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN mkdir -p wwwroot/uploads

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RepairTracker.dll"]
