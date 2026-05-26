FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/SmartOutbox.Core/SmartOutbox.Core.csproj", "src/SmartOutbox.Core/"]
COPY ["src/SmartOutbox.EntityFramework/SmartOutbox.EntityFramework.csproj", "src/SmartOutbox.EntityFramework/"]
COPY ["src/SmartOutbox.RabbitMQ/SmartOutbox.RabbitMQ.csproj", "src/SmartOutbox.RabbitMQ/"]
COPY ["src/SmartOutbox.SampleApi/SmartOutbox.SampleApi.csproj", "src/SmartOutbox.SampleApi/"]

RUN dotnet restore "src/SmartOutbox.SampleApi/SmartOutbox.SampleApi.csproj"

COPY . .
WORKDIR "/src/src/SmartOutbox.SampleApi"
RUN dotnet publish "SmartOutbox.SampleApi.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SmartOutbox.SampleApi.dll"]
