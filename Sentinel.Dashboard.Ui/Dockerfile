FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build-env
WORKDIR /app

# Copy everything and build
COPY . ./
RUN dotnet publish -c Release  -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
RUN apk add --no-cache icu-libs
RUN apk add --no-cache tzdata
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN adduser --disabled-password \
  --home /app \
  --gecos '' dotnetuser && chown -R dotnetuser /app

WORKDIR /app
COPY --from=build-env /app/out .

USER dotnetuser
ENTRYPOINT ["dotnet", "Sentinel.Dashboard.Ui.dll"]