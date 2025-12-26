# Requirements and Feature Breakdown — cs2price_prediction

## Overview
The system consists of a machine learning model that predicts CS2 skin prices and a REST API that exposes prediction, explanation and metadata endpoints.

## Functional requirements by endpoint

### 1. POST /api/predict
**Purpose:** Return a price prediction and sticker-related features for a given skin configuration.

**Input (JSON):**
- skinId (integer) — required
- wearTierId (integer) — required
- floatValue (float) — required
- isStattrak (boolean) — required
- pattern (integer) — optional/required according to API contract
- stickers (array of integers) — optional

**Success response (200 JSON):**
- predicted_price (number)
- stickers_features:
  - stickers_count (int)
  - stickers_total_value (number)
  - stickers_avg_value (number)
  - stickers_max_value (number)

**Error cases:**
- 400 Bad Request — missing or invalid fields
- 404 Not Found — skinId not present (or other resource not found)
- 500 Server Error — unexpected server errors (log internally)

**Testable aspects:**
- Schema correctness
- Value ranges (predicted_price >= 0)
- Response time (SLA: avg ≤ 1s)
- Handling of edge values (floatValue = 0.0, 1.0; empty stickers; very large sticker arrays)

---

### 2. POST /api/ai/explain and POST /api/ai/explain-v2
**Purpose:** Return a human-readable explanation for a given prediction (v1 and v2).

**Input (JSON):**
- predictedPrice (number) — required
- skinId, wearTierId, floatValue, isStattrak, pattern, stickers — same as /api/predict

**Success response (200 JSON):**
- explanation (string) — non-empty text explaining model factors

**Testable aspects:**
- Presence and non-emptiness of `explanation`
- Basic semantic relevance (mentions wearTier, floatValue, stickers)
- Validation of input types (predictedPrice must be numeric)
- Compare differences between explain and explain-v2 (document differences)

---

### 3. GET /api/meta/weapon-types
**Purpose:** Return list of weapon types.
**Response:** array of { id, code, name }
**Testable aspects:** non-empty array, correct types

### 4. GET /api/meta/weapon-types/{weaponTypeId}/weapons
**Purpose:** Return weapons for a given weapon type.
**Testable aspects:** valid weaponTypeId → 200 with array; invalid → 404 or empty array by contract

### 5. GET /api/meta/weapons/{weaponId}/skins
**Purpose:** Return skins for a weapon (id, name, patternStyle)
**Testable aspects:** presence of expected patternStyle values, non-empty arrays for valid weaponId

### 6. GET /api/meta/skins/{skinId}/wear-tiers
**Purpose:** Return wear tiers (Factory New, Minimal Wear, etc.)
**Testable aspects:** expected set contains typical tiers; invalid skinId → 404

### 7. GET /api/meta/skins/{skinId}/patterns
**Purpose:** Return available pattern ids for a skin
**Testable aspects:** pattern used in /api/predict should exist in this list for valid skinId

### 8. GET /api/meta/stickers?q=&limit=
**Purpose:** Search stickers by substring and limit results
**Query params:**
- q (string) — optional
- limit (int) — optional (default documented)
**Testable aspects:**
- q filters names (case-insensitive)
- limit restricts array length
- response schema correctness

---

## Non-functional requirements
- Performance: /api/predict average response time ≤ 1 second under normal conditions
- Reliability: Service should not crash on invalid input
- Documentation: Swagger must contain parameter descriptions and example responses for /api/predict and /api/ai/explain endpoints
- Consistency: Error responses must be JSON and contain `error` and `message` fields

---

## Traceability
Each functional requirement above will be mapped to one or more test cases in `docs/test_cases.md`. Test cases will cover positive, negative and boundary scenarios and will reference the requirement ID (e.g., F1-PREDICT-01).
