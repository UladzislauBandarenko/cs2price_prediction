## Problem Statement and Project Goals

### Market Context and Background

The economy of virtual items in modern online games has evolved into an independent and highly dynamic digital market with real-world financial value. One of the most prominent examples of such an economy is Counter-Strike 2 (CS2), where cosmetic in-game items (“skins”) are actively traded on both official and third-party marketplaces.

The pricing of CS2 skins is determined by a complex combination of factors, including:
- intrinsic item characteristics (rarity tier, wear level, float value, pattern index),
- additional modifiers (stickers, their quantity, condition, and placement, StatTrak),
- market supply and demand mechanisms,
- historical price trends and volatility,
- external events (game updates, changes in drop mechanics, esports events, marketplace policy changes).

The nonlinear, stochastic, and highly volatile nature of this market significantly complicates accurate price estimation.


### Problem Description

Despite the high liquidity and economic significance of the CS2 skin market, existing pricing and analytics tools exhibit several critical limitations:

- incomplete consideration of item-specific attributes,
- limited use of large-scale historical data,
- lack of transparency and interpretability in price formation,
- delayed data updates and low levels of automation,
- insufficient analytical and visualization capabilities.

As a result, users lack a reliable mechanism for assessing the fair market value of selected items, which increases financial risk and reduces decision-making efficiency.


### Problem Statement

**Who:**  
CS2 players, skin traders, market analysts, and third-party digital platforms.

**What:**  
There is no transparent, ML-driven system capable of providing accurate price estimates for selected CS2 skins while explaining the key factors influencing those prices.

**Why:**  
Inaccurate or opaque valuation increases uncertainty, financial risk, and undermines trust in existing analytical solutions.


### Research Scope and Time Frame

This project is conducted as a **research-oriented study** focused on the analysis and modeling of the CS2 skin market.

**The active market research period is strictly limited and runs until September 25, 2025.**

Within this time frame, the project involves:
- continuous collection and preprocessing of historical and current market data,
- analysis of price dynamics for selected items,
- training, validation, and comparative evaluation of machine learning models.

All models, experiments, and conclusions presented in this project are **based exclusively on data collected up to September 25, 2025**.


### Limitations of Model Adaptation to Market Changes

The CS2 skin market is characterized by high volatility and is subject to **categorical (structural) economic changes**, including:
- modifications to item drop and case mechanics,
- major game updates,
- external events affecting player behavior,
- changes in marketplace regulations or accessibility.

Machine learning models cannot adapt instantaneously to such changes. Accurate reflection of new market conditions requires additional time for:
- accumulation of representative post-change data,
- model retraining and recalibration,
- evaluation of market stabilization.

The duration of this adaptation period is not fixed and depends on the scale and economic impact of the change (*concept drift*).


### Project Goals

#### Primary Goal

To research and experimentally evaluate the feasibility of applying machine learning and large-scale market data analysis to predict fair market prices for selected CS2 skins, and to develop and deploy a web-based application that performs price analysis and prediction using continuously updated real-world market data obtained from a partner trading platform, including automated data updates and model retraining on a 24-hour basis.

#### Secondary Goals

- identify the most influential factors affecting price formation of CS2 skins;
- compare different machine learning models in terms of accuracy, stability, and interpretability;
- provide transparent and explainable model predictions;
- visualize historical price dynamics and prediction uncertainty;
- analyze limitations and risks of ML-based pricing in volatile digital markets;
- evaluate the impact of regular data updates and periodic model retraining on prediction quality.

### Business and Technical Goals

| Goal | Description | KPI |
|------|------------|-----|
| Pricing Accuracy | Improve price estimation quality using real and updated market data | ≥ 85–90% |
| Research Validity | Assess ML feasibility on real market data | Documented experiments |
| Transparency | Explain model decisions | SHAP / feature importance |
| Automation | Fully automated data ingestion and model retraining | ≤ 24h refresh cycle |
| Data Freshness | Ensure up-to-date market information | ≤ 24h data delay |
| System Reliability | Stable web application operation | ≥ 99% uptime |

### Project Objectives

- collect and preprocess large-scale historical and current CS2 market data obtained from a partner trading platform;
- design and implement a scalable data pipeline supporting continuous market monitoring and 24-hour data updates;
- develop automated workflows for periodic retraining and validation of machine learning models;
- train and compare ML models (e.g., Random Forest, XGBoost, LSTM) for CS2 skin price prediction;
- quantitatively evaluate prediction errors, robustness, and stability under market changes;
- develop a web application that:
  - outputs predicted prices for selected CS2 skins;
  - visualizes historical trends and key influencing factors;
  - explicitly communicates prediction uncertainty and model confidence;
- ensure compliance with data protection, security, and ethical use standards.


### Success Criteria

The project is considered successful if:
- machine learning models demonstrate reproducible and stable performance under regular data and model updates;
- prediction errors and model behavior are quantitatively evaluated over time;
- the web application operates with automated data refresh and model retraining cycles;
- predicted prices are accurate, interpretable, and transparently explained;
- limitations and risks of the proposed approach are clearly identified and empirically justified.


### Non-Goals

The project explicitly does **not** aim to:
- facilitate or mediate trading transactions;
- provide financial or investment advice;
- guarantee profit or eliminate market risk;
- replace existing trading platforms or marketplaces.
