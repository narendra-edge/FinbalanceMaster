#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["src/Onboarding/Masters_Onboard/Masters.Api/Masters.Api.csproj", "src/Onboarding/Masters_Onboard/Masters.Api/"]
COPY ["src/Onboarding/Masters_Onboard/Masters.Core/Masters.Core.csproj", "src/Onboarding/Masters_Onboard/Masters.Core/"]
COPY ["src/Onboarding/Masters_Onboard/Masters.Infrastructure/Masters.Infrastructure.csproj", "src/Onboarding/Masters_Onboard/Masters.Infrastructure/"]
RUN dotnet restore "src/Onboarding/Masters_Onboard/Masters.Api/Masters.Api.csproj"
COPY . .
WORKDIR "/src/src/Onboarding/Masters_Onboard/Masters.Api"
RUN dotnet build "Masters.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Masters.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Masters.Api.dll"]