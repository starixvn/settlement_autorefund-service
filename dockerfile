FROM hub.nexdev.net/base/dotnet/core/sdk:9.0-alpine AS builder
WORKDIR /src

COPY ./Sources/AutoRefund.Core ./AutoRefund.Core
COPY ./Sources/AutoEarlyRefund ./AutoEarlyRefund
COPY ./Sources/AutoRefund ./AutoRefund
COPY ./Sources/AutoRefundFM ./AutoRefundFM
COPY ./Sources/Fanex.AutoRefund.Service/Fanex.AutoRefund.Service.csproj ./Fanex.AutoRefund.Service/Fanex.AutoRefund.Service.csproj

WORKDIR /src/Fanex.AutoRefund.Service
RUN dotnet nuget add source https://nuget.nexdev.net/api/v2
RUN dotnet restore -v n

COPY ./Sources/Fanex.AutoRefund.Service .

RUN dotnet publish -r linux-x64 --self-contained false -c Release -o /app 

FROM hub.nexdev.net/base/dotnet/core/aspnet:9.0-alpine AS base

WORKDIR /app
COPY --from=builder /app .

ENV TZ="America/La_Paz"
ENV COMPlus_EnableDiagnostics=0


# Don t run as root
RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app && chown -R dotnetuser /tmp
USER dotnetuser
#

ENTRYPOINT ["dotnet", "Fanex.AutoRefund.Service.dll"]
