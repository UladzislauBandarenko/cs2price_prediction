# ML Service API Documentation

## Overview

The ML Service API exposes internal prediction endpoints for different CS2 skin categories.
It is used **exclusively by the backend Prediction API** and is **not accessed directly by end users**.

All requests and responses use **JSON** format.
All endpoints return the **predicted price as a string**, as produced by CatBoost models.

**Base URL:** `/predict`  
**Authentication:** Not required (internal service)

---

## Architectural Role

The ML Service is a dedicated microservice responsible only for **machine learning inference**.

Responsibilities:
- Load trained ML models at startup
- Accept preprocessed feature vectors
- Return raw price predictions

Non-responsibilities:
- Business validation
- Entity existence checks (skin, pattern, wear tier, etc.)
- Authorization and access control

All domain validation is handled upstream by the **Prediction API**.

---

## Validation Model

The ML Service relies solely on **FastAPI + Pydantic** validation.

Validation characteristics:
- Type checking
- Required field enforcement
- Schema-level validation

Invalid requests return **HTTP 422 Unprocessable Entity**.

No domain-level validation (e.g. skin existence, pattern validity) is performed.

---

## Error Response (422)

```json
{
  "detail": [
    {
      "loc": ["body", "field_name"],
      "msg": "validation error",
      "type": "type_error"
    }
  ]
}
```

---

## 1. Predict Case Hardened

### POST /case-hardened

Predicts prices for Case Hardened weapons and knives using color distribution features.

### Request Body

```json
{
  "float": 0.0,
  "pattern": 0,
  "stattrak": 0,
  "backside_blue": 0,
  "backside_purple": 0,
  "backside_gold": 0,
  "playside_blue": 0,
  "playside_purple": 0,
  "playside_gold": 0,
  "weapon": "string",
  "skin": "string",
  "wear": "string"
}
```

### Response 200 OK

```json
"1699.42"
```

---

## 2. Predict Case Hardened Guns

### POST /ch-guns

Predicts prices for Case Hardened guns using extended feature vectors.

### Request Body

```json
{
  "weapon": "string",
  "skin": "string",
  "wear": "string",
  "pattern_style": "string",
  "float": 0.0,
  "pattern": 0,
  "stattrak": 0,
  "backside_blue": 0,
  "playside_blue": 0,
  "stickers_count": 0,
  "stickers_total_value": 0,
  "stickers_avg_value": 0,
  "stickers_max_value": 0,
  "slot0_price": 0,
  "slot1_price": 0,
  "slot2_price": 0,
  "slot3_price": 0,
  "blue_score": 0,
  "blue_tier": 0
}
```

### Response 200 OK

```json
"1325.11"
```

---

## 3. Predict Doppler

### POST /doppler

Predicts prices for Doppler skins based on phase, float value, and StatTrak flag.

### Request Body

```json
{
  "weapon": "string",
  "skin": "string",
  "wear": "string",
  "phase": "string",
  "float": 0.0,
  "stattrak": 0
}
```

### Response 200 OK

```json
"2480.00"
```

---

## 4. Predict Fade Guns

### POST /fade-guns

Predicts prices for Fade guns using fade percentage, rank, and sticker-derived features.

### Request Body

```json
{
  "float": 0.0,
  "pattern": 0,
  "stattrak": 0,
  "fade_percentage": 0,
  "fade_rank": 0,
  "stickers_count": 0,
  "stickers_total_value": 0,
  "stickers_avg_value": 0,
  "stickers_max_value": 0,
  "slot0_price": 0,
  "slot1_price": 0,
  "slot2_price": 0,
  "slot3_price": 0,
  "weapon": "string",
  "skin": "string",
  "wear": "string"
}
```

### Response 200 OK

```json
"987.65"
```

---

## 5. Predict Fade Knives

### POST /fade-knives

Predicts prices for Fade knives using fade-related features.

### Request Body

```json
{
  "float": 0.0,
  "pattern": 0,
  "stattrak": 0,
  "fade_percentage": 0,
  "fade_rank": 0,
  "weapon": "string",
  "skin": "string",
  "wear": "string"
}
```

### Response 200 OK

```json
"2100.30"
```

---

## 6. Predict Float-Sensitive Guns

### POST /float-sensitive-guns

Predicts prices for skins where float value has a strong influence.

### Request Body

```json
{
  "float": 0.0,
  "stattrak": 0,
  "stickers_count": 0,
  "stickers_total_value": 0,
  "stickers_avg_value": 0,
  "stickers_max_value": 0,
  "slot0_price": 0,
  "slot1_price": 0,
  "slot2_price": 0,
  "slot3_price": 0,
  "weapon": "string",
  "skin": "string",
  "wear": "string"
}
```

### Response 200 OK

```json
"745.80"
```

---

## Performance Notes

- All ML models are loaded at service startup
- Average prediction latency: **< 50 ms**
- Designed for synchronous internal calls

---

## Known Limitations

- The service assumes all input data is prevalidated
- No authentication or rate limiting is implemented
- Returned price values are strings due to model output formatting
