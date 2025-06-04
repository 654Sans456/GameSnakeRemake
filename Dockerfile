FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["SnakeRemake.csproj", "."]
RUN dotnet restore "SnakeRemake.csproj"

COPY . .
RUN dotnet build "SnakeRemake.csproj" -c Release -o /app/build
RUN dotnet publish "SnakeRemake.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "SnakeRemake.dll"]
