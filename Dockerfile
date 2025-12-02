FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BasketManagementAPI.sln", "."]
COPY ["BasketManagementAPI/BasketManagementAPI.csproj", "BasketManagementAPI/"]
RUN dotnet restore "BasketManagementAPI/BasketManagementAPI.csproj"

COPY . .
WORKDIR /src/BasketManagementAPI
RUN dotnet publish "BasketManagementAPI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BasketManagementAPI.dll"]