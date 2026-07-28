FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY RepairTracker.Shared/RepairTracker.Shared.csproj RepairTracker.Shared/
COPY RepairTracker.Client/RepairTracker.Client.csproj RepairTracker.Client/
COPY RepairTracker.Server/RepairTracker.Server.csproj RepairTracker.Server/
RUN dotnet restore RepairTracker.Server/RepairTracker.Server.csproj

COPY RepairTracker.Shared/ RepairTracker.Shared/
COPY RepairTracker.Client/ RepairTracker.Client/
COPY RepairTracker.Server/ RepairTracker.Server/
RUN dotnet publish RepairTracker.Server/RepairTracker.Server.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN mkdir -p wwwroot/uploads

COPY --from=build /app/publish .

ARG GIT_COMMIT=unknown
ENV GIT_COMMIT=$GIT_COMMIT
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RepairTracker.Server.dll"]
