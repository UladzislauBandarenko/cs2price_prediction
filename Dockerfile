# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# сначала только csproj дл€ кэша
COPY cs2price_prediction.csproj ./
RUN dotnet restore

# потом весь код
COPY . ./

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# non-root user (безопасность)
RUN adduser --disabled-password --gecos "" appuser
USER appuser

ENV ASPNETCORE_URLS=http://0.0.0.0:8080

# публикуем приложение
COPY --from=build /app/publish ./

# ?? ƒќЅј¬Ћя≈ћ: кладЄм CSV туда же, где его ищет Program.cs
COPY --from=build /src/cs2_ml_service/data/stickers_dataset.csv ./cs2_ml_service/data/stickers_dataset.csv

EXPOSE 8080

ENTRYPOINT ["dotnet", "cs2price_prediction.dll"]
