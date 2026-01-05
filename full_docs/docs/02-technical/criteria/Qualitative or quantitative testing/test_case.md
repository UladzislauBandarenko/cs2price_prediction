# Full Manual Test Cases — cs2price_prediction 

This document contains a comprehensive and detailed set of manual test cases for the cs2price_prediction REST API.
Each test case includes: **ID**, **Title**, **Preconditions**, **Steps**, **Expected Result**, and **Priority**.

Base URL used in examples: `http://localhost:8087`

---

## How to use this document
- Execute tests in priority order: Smoke → High → Medium → Low.
- Record results in `docs/test_report.md` with columns: Executed, Passed, Failed, Comments.
- For performance measurements use Postman Runner, `curl -w "%{time_total}
"`, or a load tool (k6/jMeter).
- For each failed test collect request/response bodies, status codes, headers and server logs.

---

# 1. AiExplanation — POST /api/ai/explain

**Description:** Returns a human-readable explanation for a provided predicted price and skin features. Tests verify validation, semantics and robustness.

### TC_EXPL_V1_01 — Valid request (Happy Path)
**Preconditions:** API is running and reachable.  
**Steps:**  
1. Send POST `/api/ai/explain` with body:
```json
{
  "predictedPrice": 3525,
  "skinId": 13,
  "wearTierId": 1,
  "floatValue": 0.023,
  "isStattrak": false,
  "pattern": 332,
  "stickers": [1,6,65,5]
}
```
**Expected Result:**  
- 200 OK  
- Response JSON contains `"explanation"` field with a non-empty string.  
- Content-Type header = `application/json`.  
**Priority:** High

### TC_EXPL_V1_02 — Missing required field `predictedPrice`
**Preconditions:** API running.  
**Steps:** Send same request but omit `predictedPrice`.  
**Expected Result:**  
- 400 Bad Request  
- Response JSON contains `error`/`message` describing the missing field.  
**Priority:** High

### TC_EXPL_V1_03 — Wrong type for `predictedPrice`
**Steps:** Send request with `"predictedPrice": "three thousand"`.  
**Expected Result:**  
- 400 Bad Request or validation error; message indicates wrong data type.  
**Priority:** High

### TC_EXPL_V1_04 — Empty stickers array
**Steps:** Send request with `"stickers": []`.  
**Expected Result:**  
- 200 OK  
- `explanation` should not reference sticker values or should explicitly state no stickers influence the price.  
**Priority:** Medium

### TC_EXPL_V1_05 — Very large stickers array
**Steps:** Send request with 500 sticker IDs in the `stickers` array.  
**Expected Result:**  
- Either 200 OK (processed) or 413 Payload Too Large; service must not crash.  
- If 200 — measure and document response time increase.  
**Priority:** Medium

### TC_EXPL_V1_06 — Semantic relevance check
**Steps:** Send a request with Factory New wear and very low float value.  
**Expected Result:**  
- The `explanation` text should mention wear tier and float value as positive contributors to price. (Manual verification)  
**Priority:** Medium

### TC_EXPL_V1_07 — Invalid Content-Type header
**Steps:** Send the valid JSON body but set header `Content-Type: text/plain`.  
**Expected Result:**  
- 400 Bad Request or 415 Unsupported Media Type depending on API behavior; no server crash.  
**Priority:** Low

### TC_EXPL_V1_08 — Extremely large predictedPrice value
**Steps:** Send request with `"predictedPrice": 999999999`.  
**Expected Result:**  
- API handles input safely: either 200 with sensible explanation or 400 if value considered invalid.  
**Priority:** Low

### TC_EXPL_V1_09 — Zero predictedPrice edge-case
**Steps:** Send request with `"predictedPrice": 0`.  
**Expected Result:**  
- 200 OK; explanation addresses the low/zero price (e.g., market anomaly or worthless item).  
**Priority:** Low

### TC_EXPL_V1_10 — Malformed JSON
**Steps:** Send invalid JSON (e.g., trailing comma or missing brace).  
**Expected Result:**  
- 400 Bad Request with parse error message.  
**Priority:** High

### TC_EXPL_V1_11 — Partially invalid sticker values
**Steps:** Send `stickers` array containing valid and invalid items, e.g. `[1, "abc", -5, 9999999]`.  
**Expected Result:**  
- Either 400 Bad Request listing invalid sticker IDs, or API filters invalid entries and proceeds; behavior must be documented.  
**Priority:** High

### TC_EXPL_V1_12 — Rate limit / stress spot-check
**Steps:** Send 50 requests per second for 30 seconds (load test) using a tool.  
**Expected Result:**  
- API remains available or returns 429 Too Many Requests; error rate and latency recorded.  
**Priority:** Medium

---

# 2. AiExplanationV2 — POST /api/ai/explain-v2

**Description:** Second version of explanation endpoint — may return different phrasing or additional details.

### TC_EXPL_V2_01 — Valid request (Happy Path)
**Steps:** POST `/api/ai/explain-v2` with the same valid body as TC_EXPL_V1_01.  
**Expected Result:**  
- 200 OK; `explanation` field present and non-empty.  
**Priority:** High

### TC_EXPL_V2_02 — Comparative analysis V1 vs V2
**Steps:** Send identical inputs to `/api/ai/explain` and `/api/ai/explain-v2`.  
**Expected Result:**  
- Both return 200; record differences in wording, length, and factors mentioned. Note which version is more informative.  
**Priority:** Medium

### TC_EXPL_V2_03 — Negative/invalid predictedPrice
**Steps:** Send `"predictedPrice": -100` or non-sensical value.  
**Expected Result:**  
- 400 Bad Request or documented handling (e.g., clamp to zero).  
**Priority:** Medium

### TC_EXPL_V2_04 — Repeatability check
**Steps:** Send the same request 5 times in short succession.  
**Expected Result:**  
- Either deterministic identical explanations (preferred) or variations documented and explained.  
**Priority:** Low

### TC_EXPL_V2_05 — Empty stickers handling
**Steps:** Send with `"stickers": []`.  
**Expected Result:**  
- 200 OK; explanation should not reference sticker influence.  
**Priority:** Low

---

# 3. Meta API — General notes
**Description:** Read-only endpoints that provide metadata (weapon types, weapons, skins, patterns, stickers). Tests check schema, validation, search, pagination and robustness.

---

## 3.1 GET /api/meta/weapon-types

### TC_META_WT_01 — Retrieve weapon types (Happy Path)
**Preconditions:** API running.  
**Steps:** GET `/api/meta/weapon-types`  
**Expected Result:**  
- 200 OK  
- Response body is an array of objects with fields `{ id:int, code:string, name:string }`  
- Array length > 0  
**Priority:** Smoke

### TC_META_WT_02 — Schema types validation
**Steps:** Validate each object's `id` is integer, `code` and `name` are strings.  
**Expected Result:** All items conform to type expectations.  
**Priority:** High

### TC_META_WT_03 — Unsupported Accept header
**Steps:** Send request with `Accept: application/xml`.  
**Expected Result:** 406 Not Acceptable or fallback to JSON; document behavior.  
**Priority:** Low

---

## 3.2 GET /api/meta/weapon-types/{weaponTypeId}/weapons

### TC_META_WT_WEAP_01 — Valid weaponTypeId
**Steps:** GET `/api/meta/weapon-types/2/weapons`  
**Expected Result:**  
- 200 OK; array of weapon objects `{ id, name }`.  
**Priority:** Smoke

### TC_META_WT_WEAP_02 — Non-integer path param
**Steps:** GET `/api/meta/weapon-types/abc/weapons`  
**Expected Result:**  
- 400 Bad Request; message indicates invalid path parameter.  
**Priority:** High

### TC_META_WT_WEAP_03 — Non-existent ID
**Steps:** GET `/api/meta/weapon-types/9999/weapons`  
**Expected Result:** 404 Not Found or empty array per API contract. Document behavior.  
**Priority:** High

### TC_META_WT_WEAP_04 — Boundary values (0, negative)
**Steps:** GET with `weaponTypeId=0` and `-1`.  
**Expected Result:** 400 Bad Request or 404; behavior documented.  
**Priority:** Medium

### TC_META_WT_WEAP_05 — Parallel requests stress check
**Steps:** Send 100 parallel GET requests to this endpoint.  
**Expected Result:** API remains responsive; document errors/latency.  
**Priority:** Medium

---

## 3.3 GET /api/meta/weapons/{weaponId}/skins

### TC_META_WEAP_SKIN_01 — Valid weaponId
**Steps:** GET `/api/meta/weapons/16/skins`  
**Expected Result:** 200; array of skins each with `{ id, name, patternStyle }`.  
**Priority:** Smoke

### TC_META_WEAP_SKIN_02 — Non-existent weaponId
**Steps:** GET `/api/meta/weapons/9999/skins`  
**Expected Result:** 404 Not Found or empty array; document.  
**Priority:** High

### TC_META_WEAP_SKIN_03 — Validate patternStyle values
**Steps:** Check that `patternStyle` values are within documented set (e.g., `fade_gun`, `float_gun`).  
**Expected Result:** All values are valid.  
**Priority:** Medium

---

## 3.4 GET /api/meta/skins/{skinId}/wear-tiers

### TC_META_WEAR_01 — Valid skinId
**Steps:** GET `/api/meta/skins/13/wear-tiers`  
**Expected Result:** 200; array contains common tiers such as `Factory New`, `Minimal Wear`, etc.  
**Priority:** High

### TC_META_WEAR_02 — Invalid skinId format
**Steps:** GET `/api/meta/skins/abc/wear-tiers`  
**Expected Result:** 400 Bad Request.  
**Priority:** High

### TC_META_WEAR_03 — Boundary skinId (min and max known)
**Steps:** GET for `skinId=1` and `skinId=<max known id>`  
**Expected Result:** 200 OK or documented behavior.  
**Priority:** Medium

---

## 3.5 GET /api/meta/skins/{skinId}/patterns

### TC_META_PATTERNS_01 — Valid skinId patterns list
**Steps:** GET `/api/meta/skins/13/patterns`  
**Expected Result:** 200; array of pattern objects/IDs; includes known `332`.  
**Priority:** High

### TC_META_PATTERNS_02 — Pattern absence is acceptable
**Steps:** Query for a skin that doesn't have a specific pattern; verify response.  
**Expected Result:** 200 with array not containing that pattern; document behavior.  
**Priority:** Medium

### TC_META_PATTERNS_03 — Uniqueness of pattern ids
**Steps:** Check response list for duplicate IDs.  
**Expected Result:** No duplicates.  
**Priority:** Low

---

## 3.6 GET /api/meta/stickers

### TC_META_STICK_01 — Search by query parameter `q`
**Steps:** GET `/api/meta/stickers?q=titan&limit=50`  
**Expected Result:** 200; array length ≤ 50 and items' names contain substring `titan` (case-insensitive).  
**Priority:** High

### TC_META_STICK_02 — No query parameters (default)
**Steps:** GET `/api/meta/stickers`  
**Expected Result:** 200; default list returned; record length.  
**Priority:** Medium

### TC_META_STICK_03 — `limit` edge cases
**Steps:** Call with `limit=0`, `limit=1`, `limit=5000`.  
**Expected Result:** Behavior documented — `limit=0` may return empty array or default; large limit may be truncated or cause 413.  
**Priority:** Medium

### TC_META_STICK_04 — Special characters and encoding in `q`
**Steps:** GET with `q=%3Cscript%3E` or `q=titan%20(foil)`  
**Expected Result:** 200 or 400; input must be safely handled and not executed.  
**Priority:** Medium

### TC_META_STICK_05 — SQL injection attempt in `q`
**Steps:** `q="'; DROP TABLE stickers;--"`  
**Expected Result:** 400 or safe handling; no side effects in the database.  
**Priority:** High

### TC_META_STICK_06 — Case-insensitive search verification
**Steps:** Perform searches with `titan`, `TITAN`, `TiTaN`.  
**Expected Result:** Equivalent results or behavior documented.  
**Priority:** Low

---

# 4. Prediction — POST /api/predict

**Description:** Main endpoint returning predicted price and computed sticker features based on input.

### TC_PRED_HAPPY_01 — Happy Path (base)
**Preconditions:** API running, model loaded.  
**Steps:** POST `/api/predict` with:
```json
{
  "skinId": 13,
  "wearTierId": 1,
  "floatValue": 0.023,
  "isStattrak": false,
  "pattern": 332,
  "stickers": [1,6,65,5]
}
```
**Expected Result:**  
- 200 OK  
- JSON contains `predicted_price` (numeric) and `stickers_features` object with keys: `stickers_count`, `stickers_total_value`, `stickers_avg_value`, `stickers_max_value`.  
**Priority:** Smoke

### TC_PRED_HAPPY_02 — isStattrak = true
**Steps:** Same as happy path but `"isStattrak": true`.  
**Expected Result:** 200 OK; predicted_price value may differ. Document delta behavior.  
**Priority:** High

### TC_PRED_01 — Missing `skinId`
**Steps:** Omit `skinId` from request.  
**Expected Result:** 400 Bad Request; error message indicates missing `skinId`.  
**Priority:** High

### TC_PRED_02 — Invalid type for `floatValue`
**Steps:** `"floatValue": "abc"`  
**Expected Result:** 400 Bad Request; type error message.  
**Priority:** High

### TC_PRED_03 — Non-existent `skinId`
**Steps:** Use `skinId: 999999`  
**Expected Result:** 404 Not Found or documented alternative.  
**Priority:** High

### TC_PRED_04 — `pattern` not in `/api/meta/skins/{skinId}/patterns`
**Steps:** Provide a pattern ID that is not present in the skin patterns list.  
**Expected Result:** 400 Bad Request or 200 with warning. Document behavior.  
**Priority:** High

### TC_PRED_05 — Negative `floatValue`
**Steps:** `"floatValue": -0.1`  
**Expected Result:** 400 Bad Request or sanitized/clamped value; behavior documented.  
**Priority:** High

### TC_PRED_06 — Very small `floatValue` (precision)
**Steps:** `"floatValue": 0.000001`  
**Expected Result:** 200 OK; no numerical instability (no NaN/Inf).  
**Priority:** Medium

### TC_PRED_07 — Duplicate sticker IDs
**Steps:** `"stickers": [1,1,1,6]`  
**Expected Result:** 200 OK; define whether `stickers_count` counts unique stickers or total entries and document.  
**Priority:** Medium

### TC_PRED_08 — Invalid sticker IDs (non-integers, negative)
**Steps:** `"stickers": [999999, -1, "abc"]`  
**Expected Result:** 400 Bad Request or API filters invalid IDs and returns a warning. Document behavior.  
**Priority:** High

### TC_PRED_09 — Empty body or non-JSON payload
**Steps:** POST with empty body or `Content-Type: text/plain`.  
**Expected Result:** 400 Bad Request or 415 Unsupported Media Type.  
**Priority:** High

### TC_PRED_10 — Concurrency stress test (20 parallel requests)
**Steps:** Use a script or load tool to send 20 parallel valid POST requests.  
**Expected Result:** API handles requests without crashing; measure latency and error rate.  
**Priority:** Medium

### TC_PRED_11 — Response schema validation
**Steps:** Validate that response includes `predicted_price` and `stickers_features` with required keys and types.  
**Expected Result:** Schema matches documentation.  
**Priority:** High

### TC_PRED_12 — Prediction monotonicity / sanity check
**Steps:** Prepare a set of inputs varying `floatValue` from 0 to 1 (e.g., 0.0, 0.1, 0.2,...1.0) keeping other params constant; record predicted_price.  
**Expected Result:** Document relationship between float and price (e.g., lower float often increases price). No NaN/Inf. (This is a black-box sanity check.)  
**Priority:** Medium

### TC_PRED_13 — Effect of `isStattrak`
**Steps:** Send identical requests differing only by `isStattrak` true/false; compare predicted_price.  
**Expected Result:** If model considers stattrak, predicted_price should be higher for true; document expected delta.  
**Priority:** Medium

### TC_PRED_14 — High-precision floatValue
**Steps:** `"floatValue": 0.0234567890123456`  
**Expected Result:** 200 OK; response handles precision; document rounding.  
**Priority:** Low

### TC_PRED_15 — Sticker values arithmetic correctness
**Steps:** Use sticker IDs with known values (if available) to calculate expected `stickers_total_value`; compare with API response.  
**Expected Result:** `stickers_total_value` equals sum of sticker values; `stickers_avg_value` computed correctly.  
**Priority:** High

### TC_PRED_16 — Malformed JSON (parser errors)
**Steps:** Send malformed JSON payload.  
**Expected Result:** 400 Bad Request; parse error message.  
**Priority:** High

### TC_PRED_17 — Missing Content-Type header
**Steps:** POST valid JSON body but omit `Content-Type` header.  
**Expected Result:** 400 or 415; behavior documented.  
**Priority:** Medium

### TC_PRED_18 — Very large request payload (many stickers + extra fields)
**Steps:** Send payload with many stickers and extra unused fields.  
**Expected Result:** 200 or 413; service should not crash. Document behavior.  
**Priority:** Medium

### TC_PRED_19 — Authorization checks (if applicable)
**Steps:** Call protected endpoint without token (if any endpoints require auth).  
**Expected Result:** 401 Unauthorized or 403 Forbidden. If no auth required, mark N/A.  
**Priority:** Medium

### TC_PRED_20 — Retry and timeout behavior
**Steps:** Simulate intermittent failures and retry client-side; observe API behavior for timeouts.  
**Expected Result:** API returns consistent errors and does not leak resources.  
**Priority:** Low

---

# Traceability Matrix (summary)

| Feature area | Endpoints | Major test IDs |
|--------------|-----------|----------------|
| Prediction | POST /api/predict | TC_PRED_HAPPY_01, TC_PRED_01..20 |
| Explanation v1 | POST /api/ai/explain | TC_EXPL_V1_01..12 |
| Explanation v2 | POST /api/ai/explain-v2 | TC_EXPL_V2_01..05 |
| Meta data | GET /api/meta/* | TC_META_* |

---

# Notes for testers
- Keep a reproducible environment description in the report: OS, API build tag, model version, DB snapshot date.  
- Attach request/response samples and response times to each test result.  
- For manual semantic checks (explanations), include a short rationale why the explanation is correct or not.

---

# End of test cases document
