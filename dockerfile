FROM hub.nexdev.net/base/dotnet/core/sdk:8.0-alpine AS builder
WORKDIR /src

COPY ./Sources/AutoRefund ./AutoRefund
COPY ./Sources/Fanex.AutoRefund.Service/Fanex.AutoRefund.Service.csproj ./Fanex.AutoRefund.Service/Fanex.AutoRefund.Service.csproj

WORKDIR /src/Fanex.AutoRefund.Service
RUN dotnet nuget add source http://nuget.nexdev.net/api/v2
RUN dotnet restore -v n

COPY ./Sources/Fanex.AutoRefund.Service .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app 

FROM hub.nexdev.net/base/dotnet/core/aspnet:8.0-alpine AS base

WORKDIR /app
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "Fanex.AutoRefund.Service.dll"]
