#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Demo/OrderManagement/OrderManagement.csproj", "Demo/OrderManagement/"]
COPY ["Coast.RabbitMQ/Coast.RabbitMQ.csproj", "Coast.RabbitMQ/"]
COPY ["Coast.Core/Coast.Core.csproj", "Coast.Core/"]
COPY ["Coast.PostgreSql/Coast.PostgreSql.csproj", "Coast.PostgreSql/"]
RUN dotnet restore "Demo/OrderManagement/OrderManagement.csproj"
COPY . .
WORKDIR "/src/Demo/OrderManagement"
RUN dotnet build "OrderManagement.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderManagement.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderManagement.dll"]