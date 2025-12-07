FROM python:3.11-slim

WORKDIR /app

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1

# 🔹 ДОБАВЛЯЕМ curl (для healthcheck)
RUN apt-get update && apt-get install -y --no-install-recommends curl \
 && rm -rf /var/lib/apt/lists/*

# requirements.txt лежит в cs2_ml_service/
COPY cs2_ml_service/requirements.txt ./requirements.txt
RUN pip install --no-cache-dir --upgrade pip \
 && pip install --no-cache-dir -r requirements.txt

# копируем код (models/ и notebook/ уже отфильтрованы корневым .dockerignore)
COPY cs2_ml_service/. .

# non-root user
RUN useradd -m mluser
USER mluser

EXPOSE 8000

CMD ["uvicorn", "app:app", "--host", "0.0.0.0", "--port", "8000"]
