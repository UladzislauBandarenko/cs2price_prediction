
## ML models Technical Documentation

### Data Collection 

####  Context and Research Motivation

Data collection was the **most complex, time-consuming, and critical stage** of this project.
Unlike many traditional machine learning problems, the CS2 skin market does not provide
open, standardized, or transparent access to historical transaction data.

There is **no official Valve API** that exposes completed sales or realized transaction prices.
Most publicly available sources provide only:

- active marketplace listings;
- seller-defined asking prices;
- approximate or aggregated price indicators.

Such data reflects **seller intent rather than actual market outcomes**.
Many listed items are never sold at the displayed price, remain unsold for long periods,
or are repeatedly relisted with changing values.

Using such sources would introduce **systematic bias**, high noise, and distorted learning signals,
making them unsuitable for supervised machine learning models that aim to predict real market prices.

As a result, reliable data acquisition became a fundamental research challenge rather than
a simple preprocessing step.


####  Limitations of Public CS2 Market Data

Publicly accessible CS2 price sources suffer from several critical limitations:

1. **Lack of transaction finality**  
   Listing prices do not represent completed buyer–seller agreements.

2. **Survivorship bias**  
   Unsold or overpriced items remain visible, while completed sales disappear.

3. **Price stickiness**  
   Listing prices may remain unchanged despite shifts in demand.

4. **Absence of attribute-level pricing**  
   Public data rarely links prices to float values, patterns, or sticker composition.

These limitations make public data sources fundamentally incompatible
with accurate price modeling.


####  Partnership-Based Data Acquisition (DMarket)

To overcome these constraints, this project relied on **direct cooperation with the project partner — DMarket**,
one of the largest CS2 marketplace platforms.

This partnership enabled access to **real completed transaction data**, which is rarely available
for academic research in virtual item markets.

The dataset consists **exclusively of finalized sales**, meaning:

- each record corresponds to a confirmed transaction;
- prices reflect actual buyer–seller agreement;
- no speculative or placeholder values are present.

This approach ensures that the target variable (`price`) represents **true market-clearing prices**.


####  Strict Data Inclusion and Exclusion Policy

To preserve dataset integrity, a strict filtering policy was applied.

##### Included Data

| Data Type | Included |
|---------|----------|
| Completed transactions | Yes |
| Real sold prices | Yes |

##### Explicitly Excluded Data

| Data Type | Excluded |
|---------|----------|
| Active listings | No |
| Asking prices | No |
| Unsold offers | No |
| Synthetic or generated data | No |
| Manual price estimates | No |

This policy eliminates speculative bias and significantly improves the reliability
of the learning signal.


####  Data Scope and Attribute Granularity

Each transaction record contains detailed attribute-level information, including:

- weapon type and specific weapon name;
- skin name and cosmetic category;
- wear tier (categorical);
- exact float value (continuous);
- StatTrak flag;
- pattern identifiers (where applicable);
- sticker configuration and composition;
- final transaction price in USD.

Such **fine-grained attribute representation** is essential, as CS2 skin pricing
is driven by complex interactions between cosmetic attributes rather than
simple additive effects.


####  Temporal Coverage and Market Dynamics

The collected data spans multiple market periods and naturally reflects:

- short-term price fluctuations;
- effects of in-game updates and balance changes;
- release of new cases and skins;
- shifts in player demand and trading behavior.

As a result, the dataset exhibits **non-stationarity**, which is a realistic property
of real-world financial and virtual item markets.

This characteristic was explicitly considered during model selection and evaluation.


####  Data Quality vs. Data Volume Trade-off

Due to restricted access to completed transactions,
the final dataset is **moderate in size but high in quality**.

This trade-off was intentional:

- fewer samples;
- lower label noise;
- higher signal-to-noise ratio;
- improved model generalization.

For market price prediction tasks,
**data quality dominates raw volume**.


####  Data Validation and Consistency Checks

Before further analysis, the dataset underwent multiple validation steps:

- verification of numeric ranges (float values ∈ [0, 1]);
- consistency checks between wear tiers and float values;
- validation of pattern identifiers;
- removal of duplicate or malformed records.

These checks ensured internal consistency and prevented data leakage.


####  Ethical and Legal Considerations

All data collection and usage were conducted responsibly:

- no personal user data was collected;
- no private identifiers were accessed;
- no individual trading behavior can be reconstructed;
- data usage complies with marketplace policies;
- the dataset is used strictly for academic research.

The dataset reflects **aggregated market behavior** without enabling identification
of individual participants.


####  Limitations of the Dataset

Despite its high quality, the dataset has unavoidable limitations:

- limited samples for extremely rare items;
- uneven distribution across price ranges;
- market volatility and non-stationarity;
- dependence on partner-provided access.

These limitations were explicitly considered during modeling and evaluation.


####  Data Collection Summary

The data collection phase required substantially more effort than model training itself.
However, it resulted in a **realistic, unbiased, and production-grade dataset**
based exclusively on **real completed transactions from DMarket**.

This dataset provides a solid foundation for exploratory data analysis,
feature engineering, and machine learning modeling,
enabling predictions based on **actual CS2 market behavior rather than speculative approximations**.


### EDA Methodology

####  Purpose of Generalized EDA

Exploratory Data Analysis (EDA) in this project was not performed as a single,
one-size-fits-all procedure. Instead, EDA was conducted **separately for each model-specific dataset**
corresponding to different CS2 item categories.

The primary goal of EDA was to **identify meaningful features, understand price formation mechanisms,
and justify modeling decisions** prior to training machine learning models.

A total of **six independent EDA pipelines** were executed for the following datasets:

- Doppler knives  
- Fade knives  
- Fade weapons  
- Case Hardened knives  
- Case Hardened weapons  
- Float-sensitive weapons  

While the concrete features and visualizations differ between categories,
the analytical methodology remained consistent across all datasets.


####  Unified EDA Workflow

For each dataset, EDA followed a structured and repeatable workflow designed to ensure
comparability of conclusions and reproducibility of results.

The following analytical stages were applied to every dataset.


####  Dataset Structure and Feature Composition

At the initial stage, each dataset was inspected to determine:

- number of observations;
- feature composition;
- data types (numerical vs categorical);
- presence of category-specific attributes (e.g., phase, pattern, fade rank).

This step ensured that each dataset was suitable for supervised regression
and helped identify features that were unique to specific item categories.

####  Missing Values and Data Completeness

Missing values were analyzed for all features.

Importantly, missing values in CS2 market data usually represent **absence of a property**
rather than data corruption (e.g., absence of stickers or non-applicable patterns).

Based on this observation:

- numerical features were filled with `0.0`;
- categorical features were filled with `"unknown"`.

No dataset exhibited systematic missingness that could introduce bias
or require complex imputation strategies.


####  Target Variable Analysis

For each dataset, the distribution of the target variable (`price`) was analyzed
using histograms and boxplots.

Across all categories, prices exhibited:

- strong right-skewness;
- heavy-tailed distributions;
- presence of rare, high-priced outliers.

Outliers were retained, as they represent economically meaningful events
and are critical for realistic market modeling.

This analysis motivated the use of **robust error metrics**
and discouraged assumptions of normality.


####  Categorical Feature Analysis

Categorical features such as weapon type, cosmetic phase, pattern group,
and wear tier were analyzed individually for each dataset.

EDA consistently revealed:

- strong imbalance between common and rare categories;
- significant differences in average price across categories;
- category-dependent variance in price.

These findings indicated that categorical variables play a dominant role
in price formation and must be handled natively by the learning algorithm.


####  Numerical Feature Analysis

Numerical features, most notably float value and sticker-derived attributes,
were analyzed for distributional properties and relationship with price.

Across all datasets:

- float values exhibited non-linear relationships with price;
- sticker features showed high variance and weak linear correlation;
- identical numerical values could correspond to a wide range of prices.

This confirmed that numerical features interact strongly with categorical context
and cannot be modeled independently.


####  Feature Interaction Analysis

EDA consistently identified strong interaction effects, including:

- weapon type × float value;
- cosmetic phase or pattern × weapon category;
- wear tier × float value;
- float value × sticker configuration.

These interaction patterns demonstrate that CS2 price formation
is inherently non-linear and context-dependent.


####  Implications for Feature Engineering

Insights obtained during EDA directly informed feature engineering decisions:

- float values were preserved as continuous variables;
- categorical features were not aggressively encoded or reduced;
- interaction effects were intentionally preserved;
- outliers were retained to reflect real market behavior.


####  Implications for Model Selection

Based on EDA results, the following modeling decisions were justified:

- rejection of purely linear models as primary predictors;
- preference for tree-based gradient boosting algorithms;
- segmentation of the problem into category-specific models;
- prioritization of robustness over strict distributional assumptions.

These conclusions directly motivated the choice of **CatBoost**
as the primary modeling algorithm.


####  Reproducibility and Detailed EDA Reports

A complete, detailed EDA (including visualizations and dataset-specific conclusions)
was performed separately for each of the six datasets.

Due to space constraints, only the generalized EDA methodology is presented here.

**Full EDA reports for each dataset-model pair are available in the project repository**,
where all figures, intermediate analyses, and category-specific findings can be reviewed in detail.


####  EDA Summary

Exploratory Data Analysis was conducted as a systematic, repeatable process
across all six datasets used in this project.

Although the concrete characteristics of each dataset differ,
the unified EDA methodology ensured consistent feature discovery,
robust modeling assumptions, and reproducible analytical conclusions.


### Modeling Methodology

#### Modeling Objectives

The primary objective of the modeling stage was to develop machine learning models
capable of accurately predicting CS2 skin prices based on real market transaction data.

Given the heterogeneity of CS2 items and the complexity of price formation mechanisms,
the modeling process focused on:

- capturing non-linear relationships;
- handling high-cardinality categorical features;
- maintaining robustness to outliers;
- ensuring interpretability and reproducibility;
- achieving low inference latency suitable for production use.


####  Rationale for Multiple Specialized Models

A key architectural decision of this project was to **avoid a single global model**
and instead train **multiple specialized models**, each optimized for a specific
item category.

This decision was motivated by the following observations:

- different item categories rely on different dominant price drivers;
- feature importance varies significantly across categories;
- price distributions differ substantially between weapons and skin types;
- a global model risks underfitting rare but valuable segments.

As a result, six independent models were trained:

- Doppler knives  
- Fade knives  
- Fade weapons  
- Case Hardened knives  
- Case Hardened weapons  
- Float-sensitive weapons  

Each model was trained, optimized, and evaluated independently.


###  Baseline Models


####  General Training Strategy

The modeling stage aimed to construct accurate, robust, and interpretable machine learning models
for CS2 skin price prediction using real completed transaction data.

Given the strong heterogeneity of the CS2 market, a **category-specific modeling strategy** was adopted.
Instead of training a single global model, independent models were trained for each market segment.

This approach allows:
- capturing category-specific price drivers;
- reducing bias toward dominant item groups;
- improving accuracy for rare but valuable items.

All models were trained using supervised regression on historical transaction data.

####  Baseline Models

Two baseline approaches were used consistently across all categories.

#####  Linear Regression Baseline

A linear regression model with one-hot encoded categorical features was trained
as a classical baseline.

Purpose:
- provide a reference point;
- test whether linear assumptions are sufficient.

Across all categories, linear regression failed to capture non-linear price behavior
and systematically underestimated expensive items.


#####  CatBoost Baseline

CatBoost was selected as the primary non-linear baseline model due to:
- native categorical feature handling;
- robustness to outliers;
- strong performance on tabular data;
- minimal preprocessing requirements.

Baseline CatBoost models used conservative hyperparameters to avoid overfitting.


####  Hyperparameter Optimization (Optuna)

Hyperparameter optimization was performed using **Optuna**,
which applies Bayesian optimization to efficiently search the parameter space.

Optimized parameters included:
- tree depth;
- learning rate;
- number of boosting iterations;
- L2 leaf regularization.

The optimization objective was **RMSE**,
as large errors on high-priced items have higher economic cost.

Each optimization was limited to approximately **20 trials**
to balance performance gains and computational cost.


####  Category-Specific Training Results

##### float_sensitive_weapons

| Model | MAE | RMSE | R² | Inference Time (s) |
|------|-----|------|----|-------------------|
| CatBoost Baseline | **192.94** | **739.91** | **0.6783** | 0.00334 |
| CatBoost Tuned | 197.94 | 789.79 | 0.6335 | 0.00334 |
| Linear Regression | 301.63 | 1178.02 | 0.1845 | 0.00489 |

**Analysis:**  
Baseline CatBoost achieved the best overall performance.
Hyperparameter tuning did not improve results, indicating that float is already well-captured
by the baseline configuration.

**Selected model:** CatBoost Baseline


#####  fade_weapon

| Model | MAE | RMSE | R² | Inference Time (s) |
|------|-----|------|----|-------------------|
| CatBoost Baseline | 624.86 | 2938.32 | 0.5374 | 0.00265 |
| CatBoost Tuned | **606.06** | **2834.46** | **0.5696** | 0.00160 |
| Linear Regression | 987.93 | 4760.43 | -0.2141 | 0.00457 |

**Analysis:**  
Hyperparameter tuning significantly improved performance.
Despite optimization, fade weapons remain difficult to model due to strong visual variability.

**Selected model:** CatBoost Tuned


#####  fade_knives

| Model | MAE | RMSE | R² |
|------|-----|------|----|
| CatBoost Baseline | 532.65 | 3129.10 | 0.7256 |
| CatBoost Tuned | **468.74** | **2876.82** | **0.7680** |
| Linear Regression | 1239.98 | 5551.91 | 0.1361 |

**Analysis:**  
Optuna optimization reduced RMSE by approximately 250 USD and increased explained variance.
Linear regression completely fails to model fade knife pricing.

**Selected model:** CatBoost Tuned


#####  doppler_knives

| Model | MAE | RMSE | R² |
|------|-----|------|----|
| CatBoost Baseline | 310 | 573 | 0.887 |
| CatBoost Tuned | **304** | **563** | **0.891** |
| Linear Regression | 642 | 991 | 0.663 |

**Analysis:**  
Baseline CatBoost already performed strongly.
Hyperparameter tuning provided small but consistent improvements.

**Selected model:** CatBoost Tuned


##### ch_knives

| Model | MAE | RMSE | R² |
|------|-----|------|----|
| CatBoost Baseline | 335.90 | 816.42 | 0.9841 |
| CatBoost Tuned | **178.98** | **564.46** | **0.9924** |
| Linear Regression | 2189.06 | 6062.03 | 0.1214 |

**Analysis:**  
Hyperparameter optimization dramatically improved performance.
Linear regression severely underfits the data.

**Selected model:** CatBoost Tuned


#####  case_hardened_gun

| Model | MAE | RMSE | R² |
|------|-----|------|----|
| CatBoost Tuned | **446.36** | **2645.68** | **0.6243** |
| Linear Regression | 1012.46 | 4272.35 | 0.1017 |

**Analysis:**  
Optimization reduced RMSE by over 900 USD compared to baseline.
Case Hardened guns remain inherently volatile due to rare pattern effects.

**Selected model:** CatBoost Tuned


####  Model Selection Summary

| Category | Selected Model | Reason |
|--------|---------------|--------|
| float_sensitive_weapons | CatBoost Baseline | Best RMSE; tuning unnecessary |
| fade_weapon | CatBoost Tuned | Improved RMSE and R² |
| fade_knives | CatBoost Tuned | Significant optimization gains |
| doppler_knives | CatBoost Tuned | Highest overall accuracy |
| ch_knives | CatBoost Tuned | Near-perfect fit |
| case_hardened_gun | CatBoost Tuned | Large RMSE reduction |


#### Reproducibility and Experiment Tracking

All experiments, hyperparameter searches, and evaluation metrics
were tracked using **Weights & Biases (W&B)**.

Public experiment dashboards:
- https://wandb.ai/20220481-https-en-ehuniversity-lt-/cs2-fade-knives-pricing  
- https://wandb.ai/20220481-https-en-ehuniversity-lt-/cs2-ch-knife-pricing  
- https://wandb.ai/20220481-https-en-ehuniversity-lt-/cs2-ch-pricing  
- https://wandb.ai/20220481-https-en-ehuniversity-lt-/cs2-fade-pricing  
- https://wandb.ai/20220481-https-en-ehuniversity-lt-/cs2-fade-weapons-pricing  
- https://wandb.ai/20220481-https-en-ehuniversity-lt-/cs2-doppler-knives-pricing  

Complete training pipelines, feature engineering code,
and model artifacts are available in the project repository.



