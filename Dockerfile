FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src

COPY ["GameSnakeRemake.csproj", "."]
RUN dotnet restore "GameSnakeRemake.csproj"

COPY . .
RUN dotnet build "GameSnakeRemake.csproj" -c Release -o /app/build
RUN dotnet publish "GameSnakeRemake.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GameSnakeRemake.dll"]
