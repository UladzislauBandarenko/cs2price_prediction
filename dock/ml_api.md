# ML Service API Documentation

The ML service exposes prediction endpoints for different CS2 skin categories.  
All endpoints accept **JSON** requests and return a **predicted price** as a string.

Validation follows FastAPI/Pydantic rules—invalid requests return HTTP **422**.

---

# 1. Predict Case Hardened

## POST /predict/case-hardened

**Description:**  
Predicts the price for Case Hardened weapons and knives using color distribution on playside/backside.

**Request Body:**
```json
{
  "float": 0,
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

**Successful Response (200):**
```json
"string"
```

**Validation Error (422):**
```json
{
  "detail": [
    {
      "loc": ["string", 0],
      "msg": "string",
      "type": "string"
    }
  ]
}
```

---

# 2. Predict CH Guns

## POST /predict/ch-guns

**Description:**  
Predicts prices for Case Hardened guns using extended feature vectors including sticker stats and blue scores.

**Request Body:**
```json
{
  "weapon": "string",
  "skin": "string",
  "wear": "string",
  "pattern_style": "string",
  "float": 0,
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

**Response (200):**
```json
"string"
```

---

# 3. Predict Doppler

## POST /predict/doppler

**Description:**  
Predicts prices for Doppler skins based on phase, float, and stattrak.

**Request Body:**
```json
{
  "weapon": "string",
  "skin": "string",
  "wear": "string",
  "phase": "string",
  "float": 0,
  "stattrak": 0
}
```

**Response (200):**
```json
"string"
```

---

# 4. Predict Fade Guns

## POST /predict/fade-guns

**Description:**  
Predicts prices for Fade guns using fade percentage, fade rank, and sticker values.

**Request Body:**
```json
{
  "float": 0,
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

**Response (200):**
```json
"string"
```

---

# 5. Predict Fade Knives

## POST /predict/fade-knives

**Description:**  
Predicts prices for Fade knives using fade percentage and fade rank.

**Request Body:**
```json
{
  "float": 0,
  "pattern": 0,
  "stattrak": 0,
  "fade_percentage": 0,
  "fade_rank": 0,
  "weapon": "string",
  "skin": "string",
  "wear": "string"
}
```

**Response (200):**
```json
"string"
```

---

# 6. Predict Float-Sensitive Guns

## POST /predict/float-sensitive-guns

**Description:**  
Predicts prices for skins where float value has a strong influence.

**Request Body:**
```json
{
  "float": 0,
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

**Response (200):**
```json
"string"
```

---

# Error Handling

All endpoints return:

### 422 Unprocessable Entity  
When request fields fail validation due to:
- wrong types  
- missing required fields  
- invalid enum-like values  

---

# Notes

- All prediction values are returned as strings due to CatBoost model output formatting.
- ML service loads 6 CatBoost models at startup; predictions are fast (<50 ms).
- No authentication is required for ML endpoints (they are internal and called only by the API container).

