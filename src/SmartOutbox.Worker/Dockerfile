FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/SmartOutbox.Core/SmartOutbox.Core.csproj", "src/SmartOutbox.Core/"]
COPY ["src/SmartOutbox.EntityFramework/SmartOutbox.EntityFramework.csproj", "src/SmartOutbox.EntityFramework/"]
COPY ["src/SmartOutbox.RabbitMQ/SmartOutbox.RabbitMQ.csproj", "src/SmartOutbox.RabbitMQ/"]
COPY ["src/SmartOutbox.Worker/SmartOutbox.Worker.csproj", "src/SmartOutbox.Worker/"]

RUN dotnet restore "src/SmartOutbox.Worker/SmartOutbox.Worker.csproj"

COPY . .
WORKDIR "/src/src/SmartOutbox.Worker"
RUN dotnet publish "SmartOutbox.Worker.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SmartOutbox.Worker.dll"]
