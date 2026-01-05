# Data Collection

## Overview

Data collection was the **most challenging, resource-intensive, and critical stage** of this project.

Unlike typical machine learning tasks that rely on publicly available datasets or open APIs,
the CS2 skin market does **not provide transparent or structured historical price data**.
As a result, building a high-quality dataset required significant manual effort,
custom tooling, and direct cooperation with a real marketplace.

The primary goal of the data collection phase was to obtain **reliable, unbiased, real-world prices**
that accurately reflect actual market behavior.

---

## Cooperation with a Real Marketplace

To achieve realistic and trustworthy data, it was necessary to **cooperate with a large CS2 marketplace platform (DMarket)**.

This cooperation was essential because:

- there is **no official Valve API** for historical sold prices;
- public endpoints expose only **active listings**, not completed transactions;
- most third-party sources provide **approximate or aggregated values**, unsuitable for ML.

Access to **real completed sales data** allowed the project to work with
prices that reflect **actual buyer–seller agreement**, not speculative asking prices.

Such access is rarely available publicly and represents a major constraint in academic research
on virtual item markets.

---

## Source of Data

All data used in this project comes exclusively from **real completed transactions**.

The following strict filtering policy was applied:

| Data Type | Used |
|---------|------|
| Real sold prices | ✅ |
| Historical transactions | ✅ |
| Active listings | ❌ |
| Asking prices | ❌ |
| Synthetic or generated data | ❌ |
| Manual price estimates | ❌ |

Only **confirmed sales events** were included in the dataset.

This decision significantly increased the reliability of the target variable (`price`)
and reduced systematic bias.

---

## Why Active Listings Were Excluded

Active market listings were deliberately excluded from the dataset.

Listings do **not represent true market value** because:

- sellers often intentionally overprice items;
- many listings never result in a sale;
- prices can remain unchanged for long periods without demand;
- listing prices reflect *intent*, not *transaction outcome*.

Using listing prices would introduce severe noise and distort the learning signal,
especially for rare or premium skins.

---

## Data Volume vs Data Quality

Due to the limited availability of real transaction data,
the dataset is **moderate in size but high in quality**.

This was a conscious design choice:

- fewer samples,
- much higher signal-to-noise ratio,
- significantly improved model generalization.

In market prediction tasks, **data quality is more important than raw volume**.
A smaller dataset of verified sales is more informative than millions of speculative listings.

---

## Key Challenges in Data Collection

### 1. Absence of Public Historical APIs

There is no official or public API that provides:
- historical sold prices,
- pattern-level attributes,
- float-specific price breakdowns.

Each of these elements had to be reconstructed from raw transaction data.

---

### 2. Visual and Domain-Specific Attributes

CS2 skins are not purely numeric objects.

Their value depends on:
- categorical properties (weapon, skin, wear),
- ordinal attributes (wear tiers),
- continuous but non-linear features (float),
- hidden visual properties (fade quality, blue dominance, sticker placement).

Extracting and aligning these features with transaction prices required
domain knowledge and careful preprocessing.

---

### 3. Market Volatility

The CS2 market is highly dynamic.

Prices fluctuate due to:
- game updates and balance changes;
- new case releases and drop rates;
- esports events and influencer exposure;
- shifts in player demand.

As a result, collected data naturally contains noise and non-stationarity,
which must be handled by robust machine learning models.

---

## Resulting Dataset Characteristics

The final dataset exhibits realistic market properties:

- heavy-tailed price distribution;
- strong feature interactions;
- non-linear dependencies;
- presence of outliers and rare premium items.

These characteristics strongly influenced:
- feature engineering strategy,
- choice of evaluation metrics,
- selection of CatBoost as the primary model.

---

## Ethical and Legal Considerations

All data collection was conducted responsibly:

- no personal user data was collected;
- no private user information was accessed;
- no Terms of Service were violated;
- data was used strictly for academic research purposes.

The dataset reflects aggregated market behavior without identifying individual participants.

---

## Summary

The data collection phase required significantly more effort than model training itself.
However, it enabled the creation of a **realistic, unbiased, and production-grade dataset**.

This foundation is critical for building machine learning models that can operate
on real CS2 market data rather than artificial or speculative approximations.
