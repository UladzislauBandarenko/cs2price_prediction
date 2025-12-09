# Prediction API Documentation

The **Prediction API** is the main endpoint used by the ASP.NET Core backend to request a unified price prediction for any CS2 skin.  
It aggregates metadata, prepares ML feature vectors, calls the ML microservice, and returns a final structured response.

---

# POST /api/predict

**Description:**  
Performs a complete price prediction workflow for the selected skin configuration.  
The backend internally determines which ML model to use (Case Hardened, Fade, Doppler, Float-Sensitive, etc.) based on the skin metadata.

No authentication is required for this endpoint.

---

## Request Body (application/json)

```json
{
  "skinId": 1,
  "wearTierId": 1,
  "floatValue": 0.003,
  "isStattrak": true,
  "pattern": 3,
  "stickers": [
    0
  ]
}
```

### Field Descriptions

| Field | Type | Required | Description |
|-------|--------|----------|-------------|
| `skinId` | integer | yes | ID of the selected skin |
| `wearTierId` | integer | yes | Wear tier (FN, MW, FT, WW, BS) |
| `floatValue` | number | yes | Actual float value of the skin |
| `isStattrak` | boolean | yes | Whether the skin is StatTrak |
| `pattern` | integer | optional | Pattern index (used for CH, Fade, Doppler models) |
| `stickers` | array[int] | optional | List of sticker IDs applied |

---

## Successful Response (200)

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

### Response Fields

| Field | Type | Description |
|--------|---------|------------|
| `predicted_price` | number | Final predicted skin price |
| `stickers_features` | object | Extracted sticker-related features |

#### Stickers Features:

| Field | Description |
|--------|-------------|
| `stickers_count` | Total number of stickers |
| `stickers_total_value` | Combined base value of all stickers |
| `stickers_avg_value` | Average sticker value |
| `stickers_max_value` | Highest sticker price |

---

## Error Handling

- Generally returns **200 OK** if the request is valid.
- Validation errors for this endpoint are handled inside the API layer and normally not exposed to clients.
- Internal ML errors are not shown in the public response.

---

## Notes

- This is the **main endpoint** used by the client application.
- It performs all required logic to produce a final price:
  - DB lookup  
  - Feature construction  
  - ML model selection  
  - Prediction call  
  - Sticker value aggregation  
- Designed to be stable and easy to integrate.

