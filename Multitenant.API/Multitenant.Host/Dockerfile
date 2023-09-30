FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Multitenant.Host/Multitenant.Host.csproj", "Multitenant.Host/"]
RUN dotnet restore "Multitenant.Host/Multitenant.Host.csproj"
COPY . .
WORKDIR "/src/Multitenant.Host"
RUN dotnet build "Multitenant.Host.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Multitenant.Host.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Multitenant.Host.dll"]

RUN apt-get update && apt-get install -y curl
RUN apt-get update && apt-get install -y iputils-ping