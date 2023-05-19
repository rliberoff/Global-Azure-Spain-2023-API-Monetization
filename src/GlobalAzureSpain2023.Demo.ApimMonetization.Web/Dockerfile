#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["GlobalAzureSpain2023.Demo.ApimMonetization.Web/GlobalAzureSpain2023.Demo.ApimMonetization.Web.csproj", "GlobalAzureSpain2023.Demo.ApimMonetization.Web/"]
RUN dotnet restore "GlobalAzureSpain2023.Demo.ApimMonetization.Web/GlobalAzureSpain2023.Demo.ApimMonetization.Web.csproj"
COPY . .
WORKDIR "/src/GlobalAzureSpain2023.Demo.ApimMonetization.Web"
RUN dotnet build "GlobalAzureSpain2023.Demo.ApimMonetization.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GlobalAzureSpain2023.Demo.ApimMonetization.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GlobalAzureSpain2023.Demo.ApimMonetization.Web.dll"]