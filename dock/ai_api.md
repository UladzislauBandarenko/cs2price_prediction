# AI Explanation API

The **AI Explanation** endpoints provide detailed reasoning behind predicted skin prices.

---

## 1. AI Explanation (v1)

### POST /api/ai/explain

**Description:**  
Generates an AI-powered explanation for the predicted price of a specific skin configuration.

**Parameters:**  
None.

**Request Body (application/json):**
```json
{
  "predictedPrice": 1699,
  "skinId": 1,
  "wearTierId": 1,
  "floatValue": 0.0003,
  "isStattrak": true,
  "pattern": 3,
  "stickers": [
    0
  ]
}
```

**Response 200 OK:**
```json
{
  "explanation": "The model predicted a price of 1699.00 USDT for the AK-47 Case Hardened primarily due to its excellent condition, indicated by the Factory New wear tier and a very low float value of 0.0003. This low float means the skin is almost pristine, which typically leads to a higher price. Additionally, the skin has a good blue score of 6.21, with a blue tier of 1, suggesting that the blue pattern is prominent and desirable. The presence of StatTrak also adds value, as it tracks kills and enhances the skin's appeal. Since there are no meaningful stickers on this weapon, they do not influence the price, allowing the other factors to play a more significant role in determining its high predicted value."
}
```

---

## 2. AI Explanation (v2)

### POST /api/ai/explain-v2

**Description:**  
An enhanced version of the explanation model, providing deeper analysis of predicted pricing factors.

**Parameters:**  
None.

**Request Body (application/json):**
```json
{
  "predictedPrice": 1699,
  "skinId": 1,
  "wearTierId": 1,
  "floatValue": 0.0003,
  "isStattrak": true,
  "pattern": 3,
  "stickers": [
    0
  ]
}
```

**Response 200 OK:**
```json
{
  "explanation": "The predicted price of 1699.00 USDT is mainly influenced by the AK-47 Case Hardened being in Factory New condition with an extremely low float value of 0.0003, which means it is in near-perfect condition and highly desirable. The pattern number 3 and the very low blue percentages on both the playside and backside contribute to a unique and rare appearance, supported by the high blue score of 6.21 and the top blue tier of 1, indicating this is a highly sought-after pattern. The presence of StatTrak adds value by tracking kills, making the skin more valuable. Since there are no meaningful stickers, they do not affect the price at all. Overall, the combination of excellent wear, rare pattern, high blue tier, and StatTrak status drives the high predicted price."
}
```

