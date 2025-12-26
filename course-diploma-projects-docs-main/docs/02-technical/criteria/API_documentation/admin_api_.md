# Admin API Documentation

## Overview

The Admin API provides endpoints for managing internal reference data used by the CS2 skin pricing and prediction system.
These endpoints allow administrators to create, update, and delete metadata such as weapon types, weapons, skins, patterns,
stickers, and wear tiers.

All responses are returned in **JSON** format.

**Base URL:** `/api/v1/admin`  
**Authentication:** Required (API Key)

---

## Authentication

All Admin API endpoints require API key authorization.

**Header format:**
```
Authorization: API_KEY
```

Requests without a valid API key are rejected.

---

## Architecture Context

The Admin API is an internal management interface.
It is not intended for public clients and is protected via API key authorization.
All operations directly modify reference data used by Meta, Prediction, and AI Explanation APIs.

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
| 401 | Unauthorized |
| 404 | Resource not found |
| 500 | Internal server error |

---

## Common Validation Rules

The following validation rules apply to all Admin endpoints:

- Referenced entities (skin, weapon, pattern, sticker, wear tier) must exist
- Numeric values must be within valid domain ranges
- Duplicate entries are not allowed
- Pattern identifiers must be unique per skin
- Foreign key constraints are strictly enforced

---

## 1. Case Hardened Gun Patterns

### POST /patterns/case-hardened/gun

Creates a new Case Hardened gun pattern entry.

**Request Body**
```json
{
  "skinId": 0,
  "pattern": 0,
  "playsideBlue": 0,
  "backsideBlue": 0
}
```

**Validation**
- skinId must reference an existing skin
- pattern must not already exist for the skin
- blue values must be between 0 and 100

---

### PUT /patterns/case-hardened/gun/{id}

Updates an existing Case Hardened gun pattern.

**Validation**
- pattern entry must exist

---

### DELETE /patterns/case-hardened/gun/{id}

Deletes a Case Hardened gun pattern.

---

## 2. Case Hardened Knife Patterns

### POST /patterns/case-hardened/knife

Creates a Case Hardened knife pattern entry.

**Request Body**
```json
{
  "skinId": 0,
  "pattern": 0,
  "backsideBlue": 0,
  "backsidePurple": 0,
  "backsideGold": 0,
  "playsideBlue": 0,
  "playsidePurple": 0,
  "playsideGold": 0
}
```

**Validation**
- skinId must exist
- pattern must be unique per skin
- color percentages must be between 0 and 100

---

## 3. Doppler Phases

### POST /patterns/doppler/phases

Creates a Doppler phase.

**Validation**
- phase name must be unique

---

### DELETE /patterns/doppler/phases/{id}

Deletes a Doppler phase.

---

## 4. Doppler Skin Phase Links

### POST /patterns/doppler/skin-phases

Links a skin to a Doppler phase.

**Validation**
- skinId must exist
- phaseId must exist
- duplicate links are not allowed

---

## 5. Fade Gun Patterns

### POST /patterns/fade/gun

Creates a fade gun pattern.

**Validation**
- fadePercentage must be between 0 and 100
- fadeRank must be positive

---

## 6. Fade Knife Patterns

Same validation rules as fade gun patterns.

---

## 7. Skins

### POST /skins

Creates a new skin.

**Validation**
- weaponId must exist
- skin name must be unique per weapon

---

## 8. Skin Wear Tier Mapping

Defines which wear tiers are supported by a skin.

**Validation**
- skinId must exist
- wearTierId must exist
- mapping must be unique

---

## 9. Stickers

### POST /stickers

Creates a sticker entry.

**Request Body**
```json
{
  "name": "string",
  "referencePrice": 0
}
```

**Validation**
- sticker name must be unique
- referencePrice must be greater than 0

---

## 10. Weapons

### POST /weapons

Creates a weapon.

**Validation**
- weaponTypeId must exist
- weapon name must be unique

---

## 11. Weapon Types

### POST /weapon-types

Creates a weapon type.

**Validation**
- code must be unique
- name must be unique

---

## 12. Wear Tiers

### POST /wear-tiers

Creates a wear tier.

**Validation**
- wear tier name must be unique

---

## Versioning Strategy

The Admin API uses URL-based versioning:

```
/api/v1/admin
```

Breaking changes require a new major version.

---

## Known Limitations

- Admin endpoints are intended for trusted internal use only
- Bulk operations are not supported
