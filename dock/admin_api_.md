# Admin API Documentation

All **Admin** endpoints require **API authorization**:

```
Authorization: API_KEY 
```

These endpoints manage internal reference data used by the CS2 skin pricing and prediction system.  
They allow administrators to create, update, and delete metadata such as weapon types, skins, patterns, stickers, and wear tiers.

---

# 1. Case Hardened Gun Patterns

Case Hardened gun skins use blue-percentage metadata to determine rarity and price influence.

## POST /api/admin/patterns/case-hardened/gun

**Description:**  
Creates a new Case Hardened *gun* pattern entry.

**Authorization:** Required.

**Request Body:**
```json
{
  "skinId": 0,
  "pattern": 0,
  "playsideBlue": 0,
  "backsideBlue": 0
}
```

**Response 200 OK:**
```json
0
```

---

## PUT /api/admin/patterns/case-hardened/gun/{id}

**Description:**  
Updates an existing Case Hardened gun pattern.

**Authorization:** Required.

**Path Parameter:**  
`id` — ID of the pattern entry.

**Request Body:**
```json
{
  "playsideBlue": 0,
  "backsideBlue": 0
}
```

**Response 200 OK:**  
(no content)

---

## DELETE /api/admin/patterns/case-hardened/gun/{id}

**Description:**  
Deletes a Case Hardened gun pattern entry.

---

# 2. Case Hardened Knife Patterns

Knife Case Hardened patterns have additional color channels: blue, purple, gold.

## POST /api/admin/patterns/case-hardened/knife

**Description:**  
Creates a Case Hardened *knife* pattern entry.

**Request Body:**
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

**Response:**
```json
0
```

---

## PUT /api/admin/patterns/case-hardened/knife/{id}

Updates an existing knife CH pattern.

**Request Body:**
```json
{
  "backsideBlue": 0,
  "backsidePurple": 0,
  "backsideGold": 0,
  "playsideBlue": 0,
  "playsidePurple": 0,
  "playsideGold": 0
}
```

---

## DELETE /api/admin/patterns/case-hardened/knife/{id}

Deletes a pattern entry.

---

# 3. Doppler Phases

Doppler skins use phase metadata (e.g., Phase 1–4, Ruby, Sapphire).

## POST /api/admin/patterns/doppler/phases

Creates a Doppler phase.

**Request Body:**
```json
{
  "name": "string"
}
```

**Response:**
```json
0
```

---

## PUT /api/admin/patterns/doppler/phases/{id}

Updates a Doppler phase.

---

## DELETE /api/admin/patterns/doppler/phases/{id}

Deletes a Doppler phase.

---

# 4. Doppler Skin Phase Links

Connects a skin to a Doppler phase.

## POST /api/admin/patterns/doppler/skin-phases

**Request Body:**
```json
{
  "skinId": 0,
  "phaseId": 0
}
```

---

## PUT /api/admin/patterns/doppler/skin-phases/{id}

**Request Body:**
```json
{
  "phaseId": 0
}
```

---

## DELETE /api/admin/patterns/doppler/skin-phases/{id}

Deletes a link.

---

# 5. Fade Gun Patterns

Fade patterns contain fade percentage and rank.

## POST /api/admin/patterns/fade/gun

**Request Body:**
```json
{
  "skinId": 0,
  "pattern": 0,
  "fadePercentage": 0,
  "fadeRank": 0
}
```

---

## PUT /api/admin/patterns/fade/gun/{id}

**Request Body:**
```json
{
  "fadePercentage": 0,
  "fadeRank": 0
}
```

---

## DELETE /api/admin/patterns/fade/gun/{id}

Deletes a fade gun pattern.

---

# 6. Fade Knife Patterns

Same structure as guns but used on knives.

## POST /api/admin/patterns/fade/knife

**Request Body:**
```json
{
  "skinId": 0,
  "pattern": 0,
  "fadePercentage": 0,
  "fadeRank": 0
}
```

---

## PUT /api/admin/patterns/fade/knife/{id}

**Request Body:**
```json
{
  "fadePercentage": 0,
  "fadeRank": 0
}
```

---

## DELETE /api/admin/patterns/fade/knife/{id}

Deletes fade knife pattern.

---

# 7. Skins

## POST /api/admin/skins

Creates a new skin entry.

**Request Body:**
```json
{
  "weaponId": 0,
  "name": "string",
  "patternStyle": "string"
}
```

**Response:**
```json
{
  "id": 0,
  "name": "string",
  "patternStyle": "string"
}
```

---

## PUT /api/admin/skins/{id}

Updates a skin.

---

## DELETE /api/admin/skins/{id}

Deletes a skin.

---

# 8. Skin Wear Tier Mapping

Defines which wear tiers a skin supports.

## POST /api/admin/skin-wear-tiers

**Request Body:**
```json
{
  "skinId": 0,
  "wearTierId": 0
}
```

---

## PUT /api/admin/skin-wear-tiers

Updates a wear tier mapping.

**Request Body:**
```json
{
  "skinId": 0,
  "oldWearTierId": 0,
  "newWearTierId": 0
}
```

---

## DELETE /api/admin/skin-wear-tiers

Deletes a mapping.

**Request Body:**
```json
{
  "skinId": 0,
  "wearTierId": 0
}
```

---

# 9. Stickers

## POST /api/admin/stickers

Adds a sticker entry.

**Request Body:**
```json
{
  "name": "string",
  "referencePrice": 0
}
```

---

## PUT /api/admin/stickers/{id}

Updates sticker data.

---

## DELETE /api/admin/stickers/{id}

Deletes a sticker.

---

# 10. Weapons

## POST /api/admin/weapons

Creates a weapon entry.

**Request Body:**
```json
{
  "weaponTypeId": 0,
  "name": "string"
}
```

---

## PUT /api/admin/weapons/{id}

Updates a weapon.

---

## DELETE /api/admin/weapons/{id}

Deletes a weapon.

---

# 11. Weapon Types

## POST /api/admin/weapon-types

Creates a weapon type.

**Request Body:**
```json
{
  "code": "string",
  "name": "string"
}
```

---

## PUT /api/admin/weapon-types/{id}

Updates a weapon type.

---

## DELETE /api/admin/weapon-types/{id}

Deletes a weapon type.

---

# 12. Wear Tiers

## POST /api/admin/wear-tiers

Defines a new wear tier.

**Request Body:**
```json
{
  "name": "string"
}
```

---

## PUT /api/admin/wear-tiers/{id}

Updates wear tier.

---

## DELETE /api/admin/wear-tiers/{id}

Deletes wear tier.
