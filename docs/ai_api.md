# AI Explanation API Documentation

## Overview

The AI Explanation API provides natural-language explanations for predicted skin prices.
It explains how different attributes of a skin configuration influence the final predicted price produced by the machine learning model.

All responses are returned in **JSON** format.

**Base URL:** `/api/v1/ai`  
**Authentication:** Not required

---

## Architecture Context

This API acts as an interpretability layer on top of the pricing prediction model.
It does not calculate prices itself and only explains already predicted values.

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

| Field | Validation Rule |
|------|-----------------|
| predictedPrice | Must be greater than 0 |
| skinId | Must reference an existing skin |
| wearTierId | Must correspond to the provided float value |
| floatValue | Must be between 0.0 and 1.0 |
| pattern | Must exist for the selected skin |
| stickers | Must reference existing sticker IDs |

---

## 1. AI Explanation (v1)

### POST /explain

Generates a concise AI-generated explanation for a predicted skin price.

### Request Body

```json
{
  "predictedPrice": 1699,
  "skinId": 1,
  "wearTierId": 1,
  "floatValue": 0.0003,
  "isStattrak": true,
  "pattern": 3,
  "stickers": [0]
}
```

### Request Fields

| Field | Type | Description |
|------|------|-------------|
| predictedPrice | number | Predicted price |
| skinId | integer | Skin identifier |
| wearTierId | integer | Wear tier identifier |
| floatValue | number | Skin float value |
| isStattrak | boolean | StatTrak flag |
| pattern | integer | Pattern index |
| stickers | integer[] | Sticker IDs |

---

### Response 200 OK

```json
{
  "explanation": "The model predicted a price of 1699.00 USDT primarily due to the excellent condition and rare pattern of the skin."
}
```

---

### Validation Errors

#### Predicted price equals 0

**HTTP 400**
```json
{
  "error": {
    "code": "INVALID_PRICE",
    "message": "Predicted price must be greater than 0"
  }
}
```

#### Skin not found

**HTTP 404**
```json
{
  "error": {
    "code": "SKIN_NOT_FOUND",
    "message": "Skin with the specified ID was not found"
  }
}
```

#### Pattern not found

**HTTP 404**
```json
{
  "error": {
    "code": "PATTERN_NOT_FOUND",
    "message": "Pattern is not available for the selected skin"
  }
}
```

#### Wear tier does not match float value

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

## 2. AI Explanation (v2)

### POST /explain-v2

Provides a more detailed explanation with deeper analysis of pricing factors.

### Request Body

Same structure and validation rules as **v1**.

---

### Response 200 OK

```json
{
  "explanation": "The predicted price is influenced by the skin's near-perfect condition, rarity of the pattern, and StatTrak status."
}
```

---

