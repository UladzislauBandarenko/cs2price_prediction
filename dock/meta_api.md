# Meta API Endpoints

The **Meta** API section provides reference data related to weapon types, weapons, skins, wear tiers, patterns, and stickers.
All responses are returned in **JSON** format.

## 1. Get Weapon Types

### GET /api/meta/weapon-types

**Description:**
Retrieves a full list of available weapon types.

**Parameters:**
None.

**Response 200 OK:**
```json
[
  {
    "id": 0,
    "code": "string",
    "name": "string"
  }
]
```

## 2. Get Weapons by Weapon Type

### GET /api/meta/weapon-types/{weaponTypeId}/weapons

**Description:**
Retrieves a list of weapons belonging to the specified weapon type.

**Path Parameters:**

| Name | Type | Required | Description |
|------|--------|----------|-------------|
| weaponTypeId | integer | yes | ID of the weapon type |

**Example Request:**
GET /api/meta/weapon-types/1/weapons

**Example Response 200 OK:**
```json
[
  { "id": 1, "name": "AK-47" },
  { "id": 4, "name": "AUG" },
  { "id": 5, "name": "Famas" },
  { "id": 6, "name": "Galil AR" },
  { "id": 2, "name": "M4A1-S" },
  { "id": 3, "name": "M4A4" },
  { "id": 7, "name": "SG 553" }
]
```

## 3. Get Skins by Weapon

### GET /api/meta/weapons/{weaponId}/skins

**Description:**
Retrieves a list of skins associated with the selected weapon.

**Path Parameters:**

| Name | Type | Required | Description |
|------|----------|----------|-------------|
| weaponId | integer | yes | ID of the weapon |

**Response 200 OK:**
```json
[
  {
    "id": 0,
    "name": "string",
    "patternStyle": "string"
  }
]
```

## 4. Get Wear Tiers of a Skin

### GET /api/meta/skins/{skinId}/wear-tiers

**Description:**
Retrieves available wear tiers for the specified skin.

**Path Parameters:**

| Name | Type | Required | Description |
|--------|----------|----------|-------------|
| skinId | integer | yes | ID of the skin |

**Response 200 OK:**
```json
[
  {
    "id": 0,
    "name": "string"
  }
]
```

## 5. Get Patterns of a Skin

### GET /api/meta/skins/{skinId}/patterns

**Description:**
Retrieves all available pattern indexes for the selected skin.

**Path Parameters:**

| Name | Type | Required |
|--------|----------|----------|
| skinId | integer | yes |

**Response 200 OK:**
```json
[
  {
    "id": 0,
    "name": "string"
  }
]
```

## 6. Get Stickers

### GET /api/meta/stickers

**Description:**
Retrieves a list of available stickers. Supports filtering and limiting the number of results.

**Query Parameters:**

| Name | Type | Required | Description |
|--------|----------|----------|-------------|
| q | string | no | Text search filter |
| limit | integer | no | Max number of results (default: 50) |

**Example Request:**
GET /api/meta/stickers?q=crown&limit=10

**Response 200 OK:**
```json
[
  {
    "id": 0,
    "name": "string"
  }
]
```
