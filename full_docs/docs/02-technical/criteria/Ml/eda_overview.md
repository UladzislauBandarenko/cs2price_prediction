# Exploratory Data Analysis (EDA) Overview

## Purpose of EDA

Exploratory Data Analysis (EDA) was conducted to gain a structured understanding of the collected CS2 market data
before proceeding to feature engineering and model training.

The main objectives of EDA were:
- to analyze the structure and composition of the dataset;
- to study distributions of prices and key features;
- to identify missing values and inconsistencies;
- to understand relationships between attributes and the target variable;
- to validate domain assumptions about price formation in the CS2 market.

EDA serves as a critical bridge between raw data collection and machine learning modeling.

---

## Dataset Structure

The dataset consists of several thousand records, each corresponding to a **real completed market transaction**.

Each observation contains:
- a target variable: `price` — the final transaction price in USD;
- numerical features describing condition, float sensitivity, and sticker value;
- categorical features representing weapon type, skin name, and wear tier.

The dataset reflects real-world market conditions and therefore contains
noise, outliers, and non-uniform distributions.

---

## Missing Values Analysis

EDA revealed that missing values are present but limited in scope.

Typical patterns include:
- missing sticker-related information for items without stickers;
- absent optional attributes depending on skin type.

Given the domain context, missing values do **not represent errors** but rather
the absence of certain properties.
Therefore:
- numerical features are filled with `0.0`;
- categorical features are filled with `"unknown"`.

This strategy preserves semantic correctness and avoids introducing artificial bias.

---

## Price Distribution Characteristics

The target variable (`price`) exhibits a **strong right-skewed distribution**.

Key observations:
- the majority of items fall into a low-to-mid price range;
- a small number of rare items have very high prices;
- extreme values correspond to premium weapons, rare skins, or near-perfect float conditions.

This heavy-tailed distribution is typical for virtual item markets and
motivates the use of robust error metrics such as **MAE and RMSE**,
as well as non-linear models.

---

## Categorical Feature Analysis

### Weapon Type (`weapon`)

Weapon type is one of the most influential categorical features.

EDA confirms that:
- different weapons operate on fundamentally different price scales;
- premium weapons form a higher baseline price regardless of other attributes.

This makes `weapon` a critical feature for all downstream models.

---

### Wear Tier (`wear`)

Wear tiers are unevenly distributed:
- low-wear items (Factory New, Minimal Wear) dominate transactions;
- high-wear items appear less frequently and generally command lower prices.

However, EDA shows that wear alone is insufficient to explain price,
as its effect interacts strongly with float and weapon type.

---

## Numerical Feature Behavior

### Float Sensitivity

Float values are concentrated in the lower range (low wear),
which reflects real market demand.

EDA confirms:
- a negative relationship between `float` and `price`;
- significant dispersion for identical float values,
  indicating interaction with other features.

This supports the hypothesis that float is important but **not linearly sufficient**.

---

### Sticker-Related Features

Aggregated sticker features (count, total value, average value) show:
- high variance;
- occasional strong price impact;
- weak linear correlation with price.

These characteristics indicate non-linear influence,
making them suitable for tree-based models rather than linear regression.

---

## Feature Interactions and Non-Linearity

EDA highlights several important interaction patterns:
- weapon × float;
- weapon × wear;
- float × sticker value.

Price formation cannot be explained by additive linear effects.
Instead, it emerges from **interacting categorical and numerical factors**.

This observation directly influenced the choice of **CatBoost** as the primary model.

---

## Implications for Modeling

Based on EDA, the following conclusions were drawn:

- linear models are insufficient to capture market behavior;
- categorical variables must be handled natively, not via naive encoding;
- robustness to outliers is essential;
- feature interactions are as important as individual features.

These findings guided:
- feature selection,
- preprocessing strategy,
- model architecture choices in the ML stage.

---

## Summary

Exploratory Data Analysis confirmed that the CS2 skin market
is a complex, non-linear system with strong domain-specific structure.

The insights obtained during EDA ensured that subsequent machine learning models
were built on realistic assumptions and aligned with actual market dynamics.
