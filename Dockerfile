# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# First copy only the csproj (better for layer caching)
COPY cs2price_prediction.csproj ./
RUN dotnet restore

# Then copy the rest of the source code
COPY . ./

# Publish in Release mode, without UseAppHost (smaller image size)
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# Create a non-root user (security best practice)
RUN adduser --disabled-password --gecos "" appuser
USER appuser

# Default URL (can be overridden via ENV/docker compose)
ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# Copy published application
COPY --from=build /app/publish ./

# Put the CSV file where the application expects it
COPY --from=build /src/cs2_ml_service/data/stickers_dataset.csv ./cs2_ml_service/data/stickers_dataset.csv

EXPOSE 8080

ENTRYPOINT ["dotnet", "cs2price_prediction.dll"]
