# Prediction API Documentation

## Overview

The Prediction API is the main endpoint used by the ASP.NET Core backend to generate a unified price prediction
for any CS2 skin configuration.

The API aggregates metadata, prepares feature vectors, selects the appropriate machine learning model,
invokes the ML microservice, and returns a final structured prediction result.

All responses are returned in **JSON** format.

**Base URL:** `/api/v1`  
**Authentication:** Not required

---

## Architecture Context

This endpoint represents the core prediction workflow of the system.
It integrates database metadata, business validation rules, and multiple ML models
(Case Hardened, Fade, Doppler, Float-Sensitive, etc.) into a single prediction pipeline.

---

## Common Error Format

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error description"
  }
}
```

| HTTP Code | Description |
|----------|------------|
| 400 | Validation error |
| 404 | Resource not found |
| 500 | Internal server error |

---

## Input Validation Rules

The Prediction API applies the same validation rules as the AI Explanation API, with additional constraints.

| Field | Validation Rule |
|------|-----------------|
| skinId | Must reference an existing skin |
| wearTierId | Must correspond to the provided float value |
| floatValue | Must be between 0.0 and 1.0 |
| pattern | Must exist for the selected skin |
| stickers | Maximum 4 stickers allowed |
| stickers[] | Each sticker ID must exist |

If any validation rule is violated, the request is rejected.

---

## POST /predict

Performs a complete price prediction workflow for the selected skin configuration.

---

## Request Body (application/json)

```json
{
  "skinId": 1,
  "wearTierId": 1,
  "floatValue": 0.003,
  "isStattrak": true,
  "pattern": 3,
  "stickers": [0]
}
```

---

## Request Fields

| Field | Type | Required | Description |
|------|------|----------|-------------|
| skinId | integer | yes | Selected skin identifier |
| wearTierId | integer | yes | Wear tier identifier |
| floatValue | number | yes | Actual float value |
| isStattrak | boolean | yes | StatTrak flag |
| pattern | integer | no | Pattern index |
| stickers | integer[] | no | Applied sticker IDs (max 4) |

---

## Successful Response (200 OK)

```json
{
  "predicted_price": 1699.6503984707747,
  "stickers_features": {
    "stickers_count": 0,
    "stickers_total_value": 0,
    "stickers_avg_value": 0,
    "stickers_max_value": 0
  }
}
```

---

## Response Fields

| Field | Type | Description |
|------|------|-------------|
| predicted_price | number | Final predicted price |
| stickers_features | object | Extracted sticker-related features |

### Stickers Features

| Field | Description |
|------|-------------|
| stickers_count | Total number of stickers |
| stickers_total_value | Combined base value of all stickers |
| stickers_avg_value | Average sticker value |
| stickers_max_value | Maximum sticker value |

---

## Validation Error Examples

### Too Many Stickers

**HTTP 400**
```json
{
  "error": {
    "code": "STICKER_LIMIT_EXCEEDED",
    "message": "A maximum of 4 stickers is allowed"
  }
}
```

---

### Skin Not Found

**HTTP 404**
```json
{
  "error": {
    "code": "SKIN_NOT_FOUND",
    "message": "Skin with the specified ID was not found"
  }
}
```

---

### Pattern Not Found

**HTTP 404**
```json
{
  "error": {
    "code": "PATTERN_NOT_FOUND",
    "message": "Pattern is not available for the selected skin"
  }
}
```

---

### Wear Tier and Float Mismatch

**HTTP 400**
```json
{
  "error": {
    "code": "WEAR_TIER_MISMATCH",
    "message": "Wear tier does not correspond to the provided float value"
  }
}
```

---

## Versioning Strategy

The API uses URL-based versioning:

```
/api/v1/predict
```

Breaking changes require a new major version.

---


