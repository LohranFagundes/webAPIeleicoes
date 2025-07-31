# Use the official .NET SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Add a build argument for the version
ARG APP_VERSION=1.1.0

WORKDIR /app

# Copy the project file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application files
COPY . ./

# Build and publish the application
RUN dotnet publish -c Release -o out /p:Version=${APP_VERSION}

# Install Entity Framework tools for migrations in build stage
RUN dotnet tool install --global dotnet-ef --version 8.0.15
ENV PATH="$PATH:/root/.dotnet/tools"

# Run migrations during build (if connection string is available)
# This is optional and will be handled at runtime
RUN echo "Build completed with EF tools available"

# Use the official ASP.NET Core runtime image for the final application
FROM mcr.microsoft.com/dotnet/aspnet:8.0

# Add a build argument for the version
ARG APP_VERSION=1.1.0

WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/out .

# Create directory for uploaded photos (hybrid system fallback)
RUN mkdir -p /app/wwwroot/uploads/candidates

# Create simple startup script that waits for DB
RUN echo '#!/bin/bash\n\
echo "=== Starting Election API with Hybrid Photo System ==="\n\
echo "🚀 API Version: ${APP_VERSION}"\n\
echo "📸 Features: ImageSharp optimization, Smart photo endpoints"\n\
echo "⏳ Waiting for database to be ready..."\n\
sleep 15\n\
echo "🌟 Starting application..."\n\
exec dotnet ElectionApi.Net.dll' > /app/start.sh \
    && chmod +x /app/start.sh

# Expose the port the application will run on
EXPOSE 5110

# Set environment variables
ENV ASPNETCORE_URLS=http://0.0.0.0:5110
ENV APP_VERSION=${APP_VERSION}

# Set the entry point to use our startup script
ENTRYPOINT ["/app/start.sh"]
