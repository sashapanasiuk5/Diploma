# =========================
# Base runtime image
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

USER root

WORKDIR /app

EXPOSE 8080
EXPOSE 8081

# Install Python + venv tools
RUN apt-get update && \
    apt-get install -y python3 python3-pip python3-venv build-essential && \
    rm -rf /var/lib/apt/lists/*

# =========================
# Build .NET app
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG BUILD_CONFIGURATION=Release

WORKDIR /src

COPY ["DBGuard.AdminApp/DBGuard.AdminApp.csproj", "DBGuard.AdminApp/"]

RUN dotnet restore "DBGuard.AdminApp/DBGuard.AdminApp.csproj"

COPY . .

WORKDIR "/src/DBGuard.AdminApp"

RUN dotnet build "DBGuard.AdminApp.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/build

# =========================
# Publish .NET app
# =========================
FROM build AS publish

ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "DBGuard.AdminApp.csproj" \
    -c $BUILD_CONFIGURATION \
    -o /app/publish \
    /p:UseAppHost=false

# =========================
# Final image
# =========================
FROM base AS final

WORKDIR /app

# Copy .NET publish
COPY --from=publish /app/publish .

# Copy ML model
COPY ml-model /ml-model

# Create Python virtual environment
RUN python3 -m venv /opt/venv

# Upgrade pip inside venv
RUN /opt/venv/bin/pip install --upgrade pip

# Install requirements
RUN /opt/venv/bin/pip install --no-cache-dir \
    -r /ml-model/requirements.txt

# Copy startup script
COPY start.sh /start.sh

RUN chmod +x /start.sh

ENTRYPOINT ["/start.sh"]