# Machine Learning Overview

## Role of Machine Learning in the Project

Machine Learning is the **core pillar** of the CS2 Skin Price Prediction system.  
In contrast to traditional backend-oriented projects, the primary value of this system lies not in data storage or API routing, but in its ability to **estimate fair market prices** for Counter-Strike 2 skins based on real historical transactions and complex visual attributes.

All other system components — backend API, database, containerization, and deployment — exist to **support, operationalize, and serve** the ML models in a production-ready environment.

This section documents the **complete ML lifecycle** of the project:
- data acquisition,
- exploratory data analysis (EDA),
- feature engineering,
- model selection,
- training and evaluation,
- architectural integration.

---

## Why CS2 Skin Price Prediction Is a Hard ML Problem

Predicting CS2 skin prices is **not a standard regression task**.

Key challenges include:
- Prices are driven by **visual attributes** (patterns, phases, fades), not only by item names.
- Identical skins may differ in price by **10–50×** depending on float, pattern, or phase.
- The market is **illiquid, noisy, and highly skewed**.
- There is **no official API** providing historical sales data.

Because of these factors, classical linear assumptions do not hold, and naïve approaches lead to unstable and misleading predictions.

---

## Data Authenticity Guarantee

> **All datasets used in this project consist exclusively of real completed market transactions.**

The following data sources were explicitly excluded:
- active market listings,
- asking prices,
- user estimates,
- synthetic or augmented data.

Each datapoint corresponds to a **confirmed sale**, which significantly increases the reliability and real-world relevance of the trained models, at the cost of substantially higher data collection complexity.

Details of this process are described in `data_collection.md`.

---

## Model Specialization Strategy

Instead of training a single universal model, the system is composed of **multiple specialized models**, each optimized for a specific category of skins.

This design choice is motivated by:
- fundamentally different price formation mechanisms,
- distinct feature importance profiles,
- strong category-specific non-linearities.

The following specialized models are implemented:

- Case Hardened weapons  
- Case Hardened knives  
- Doppler knives  
- Fade knives  
- Fade weapons  
- Float-sensitive weapons  

Each model is trained, validated, and evaluated independently using a tailored feature set and domain-specific assumptions.

---

## Algorithm Selection

All core models are based on **CatBoost Regressor**, chosen due to:
- native handling of categorical features,
- strong performance on tabular data,
- robustness on small-to-medium real-world datasets,
- minimal preprocessing requirements,
- fast and stable inference.

Alternative algorithms and their limitations are discussed in `model_comparison.md`.

---

## ML Section Structure

This directory is organized according to the logical stages of the ML pipeline:

```
Ml/
├── data_collection.md
├── eda_overview.md
├── model_comparison.md
├── models/
│   ├── case_hardened.md
│   ├── ch_knives.md
│   ├── doppler_knives.md
│   ├── fade_knives.md
│   ├── fade_weapons.md
│   └── float_sensitive.md
└── index.md
```

Each file in the `models/` directory documents a **complete EDA + ML pipeline**
for a specific skin category.

---

## Architectural Consequence

The ML layer is deployed as a **dedicated FastAPI microservice**, enabling:
- independent scaling,
- isolation of heavy ML dependencies,
- a clean API contract,
- production-ready inference latency.

This design follows modern best practices for real-world ML systems.
