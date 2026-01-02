# Meta API Documentation

## Overview

The Meta API provides read-only reference data related to CS2 weapons, skins, cosmetic attributes, and stickers.
This API is designed to be used by other system components such as pricing services, analytics modules, and machine learning pipelines.

All responses are returned in **JSON** format.

**Base URL:** `/api/v1/meta`  
**Authentication:** Not required (public API)

---

## Architecture Context

The Meta API acts as a reference data provider and does not store user-specific or transactional data.
It supplies normalized metadata used across the system to ensure consistency and validation.

---

## Error Handling

All error responses follow a unified format:

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error description"
  }
}
```

| HTTP Code | Meaning |
|----------|--------|
| 400 | Invalid request parameters |
| 404 | Resource not found |
| 500 | Internal server error |

---

## 1. Get Weapon Types

### GET /weapon-types

Returns a list of all supported weapon types.

**Response 200 OK**
```json
[
  {
    "id": 1,
    "code": "rifle",
    "name": "Rifles"
  }
]
```

---

## 2. Get Weapons by Weapon Type

### GET /weapon-types/{weaponTypeId}/weapons

Returns all weapons belonging to the specified weapon type.

**Path Parameters**

| Name | Type | Description |
|----|----|----|
| weaponTypeId | integer | Unique weapon type identifier |

**Response 200 OK**
```json
[
  { "id": 1, "name": "AK-47" },
  { "id": 2, "name": "M4A1-S" }
]
```

**Errors**
- `404` Weapon type not found

---

## 3. Get Skins by Weapon

### GET /weapons/{weaponId}/skins

Returns all skins associated with the selected weapon.

**Path Parameters**

| Name | Type | Description |
|----|----|----|
| weaponId | integer | Weapon identifier |

**Response 200 OK**
```json
[
  {
    "id": 10,
    "name": "Redline",
    "patternStyle": "Abstract"
  }
]
```

---

## 4. Get Wear Tiers of a Skin

### GET /skins/{skinId}/wear-tiers

Returns available wear tiers for the specified skin.

**Response 200 OK**
```json
[
  { "id": 1, "name": "Factory New" },
  { "id": 2, "name": "Minimal Wear" }
]
```

---

## 5. Get Patterns of a Skin

### GET /skins/{skinId}/patterns

Returns all pattern indexes for the selected skin.

**Response 200 OK**
```json
[
  { "id": 661, "name": "Pattern 661" }
]
```

---

## 6. Get Stickers

### GET /stickers

Returns a list of available stickers.

**Query Parameters**

| Name | Type | Description |
|----|----|----|
| q | string | Text search query |
| limit | integer | Maximum number of results (default: 50) |

**Example Request**
```
GET /stickers?q=crown&limit=10
```

**Response 200 OK**
```json
[
  { "id": 101, "name": "Crown (Foil)" }
]
```

---

