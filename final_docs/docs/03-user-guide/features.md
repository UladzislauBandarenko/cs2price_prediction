## Feature Walkthrough Documentation

This document explains the core features available to end users
and how they interact with the CS2 Skin Price Prediction system.


### Feature: Skin Price Prediction

#### Overview

The primary feature of the application is **predicting the fair market price**
of a CS2 skin based on historical real sales data.

Unlike listing-based tools, this system estimates **what the item is likely to sell for**,
not what sellers are asking.


#### How to Use

**Step 1:** Select weapon type  
Choose the base weapon (e.g. AK-47, AWP, Knife).

**Step 2:** Select skin and wear  
Specify the skin name and wear tier (FN, MW, FT, etc.).

**Step 3:** Enter float and additional attributes  
Optionally enter float value, StatTrak flag, sticker information, or pattern-related data.

**Step 4:** Submit for prediction  
Click the prediction button to receive an estimated market price.


#### Expected Result

The system returns:
- predicted price in USD
- category-specific evaluation logic
- stable result independent of current listings


#### Tips

- More detailed inputs lead to more accurate predictions
- Float-sensitive skins benefit significantly from precise float values
- Stickers and rare patterns noticeably affect the output


### Feature: Category-Aware Modeling

#### Overview

The system automatically routes each request to a **specialized ML model**
depending on the selected skin category.

This avoids the inaccuracies of a single universal model.

#### How It Works

- Case Hardened items use pattern-aware models
- Fade items use gradient-based features
- Float-sensitive items emphasize wear precision

This logic is transparent to the user but critical for accuracy.


### Feature: Real-Time Inference

#### Overview

All predictions are performed in real time.

#### Characteristics

- Average response time: under 50 ms
- Models fully loaded in memory
- No queueing or delayed responses

This allows the system to be used interactively during trading or analysis.

### Feature: No Account Required

#### Overview

The application does not require registration or login.

This design choice:
- lowers entry barrier
- avoids personal data storage
- simplifies compliance and security

### Feature Comparison

| Feature | Available |
|-------|-----------|
| Price Prediction | Yes |
| Real sales-based data | Yes |
| Pattern-aware pricing | Yes |
| Float-sensitive modeling | Yes |
| Account required | No |
