

# 1. Introduction

The market of cosmetic items in Counter-Strike 2 (CS2) represents a complex digital economy in which prices are influenced by rarity, visual appearance, player preferences, and speculative demand. Unlike traditional in-game items, CS2 skins may reach extremely high prices when certain visual patterns are perceived as rare or desirable by the community.

Among all skin families, **Case Hardened** items occupy a special position. Their price is determined not only by the wear level (float value), but primarily by the **distribution of blue color on the texture**, especially on the playside of the weapon. This results in a highly non-linear pricing mechanism.

The goal of this diploma project is to perform a detailed exploratory data analysis of Case Hardened skins, identify the key factors influencing their market price, and use these insights to motivate the choice of machine learning models and features.

---

# 2. Exploratory Data Analysis (EDA)

## 2.1 Domain-Driven Feature Generation

### Purpose
To encode market heuristics related to Case Hardened pricing into numerical and categorical features.

### Description
The following engineered features are used:
- **`blue_score`** — continuous measure of blue dominance with higher weight for playside;
- **`blue_tier`** — discrete categorization of blue dominance (0–3);
- **`pattern_style`** — categorical classification (`blue_gem`, `blue_mix`, `other`).

### Analytical Conclusion
These features allow the dataset to reflect real market logic, where visually dominant blue patterns command higher prices.

---

## 2.2 Price Distribution

### Purpose
To understand the global statistical properties of the target variable (price) and to identify distributional characteristics that influence model selection and evaluation.

### Figure Placeholders
![Histogram of prices](docs/assets/images/ML/ch_gun/eda_price_distribution_hist.jpg)

*Figure 2.1 — Histogram of Case Hardened prices*

![Boxplot of prices](docs/assets/images/ML/ch_gun/eda_price_distribution_boxplot.jpg)

*Figure 2.2 — Boxplot of Case Hardened prices*

### Explanation
The histogram reveals a strongly right-skewed distribution of prices. The majority of Case Hardened skins are traded within a relatively low to medium price range, corresponding to common patterns with limited blue coverage. At the same time, a small number of observations form a long right tail, representing rare and highly desirable patterns.

The boxplot further emphasizes this behavior by showing a large number of high-value outliers. These points are not data errors, but rather represent genuinely rare market items, commonly referred to as Blue Gems, whose prices are driven by scarcity and collector demand.

### Analytical Conclusion
- the price distribution deviates significantly from normality;
- extreme values are meaningful and must not be removed as noise;
- model evaluation should rely on robust metrics, and modeling approaches must handle heavy-tailed targets.

---

## 2.3 Distribution by Weapon Type, Wear, and Pattern Style

### Purpose
To evaluate the balance of the dataset across key categorical variables and to understand potential sources of bias in model training.

### Figure Placeholders
![Weapon distribution](docs/assets/images/ML/ch_gun/eda_weapon_distribution.jpg)

*Figure 2.3 — Distribution by weapon type*

![Wear distribution](docs/assets/images/ML/ch_gun/eda_wear_distribution.jpg)

*Figure 2.4 — Distribution by wear category*

![Pattern style distribution](docs/assets/images/ML/ch_gun/eda_pattern_style_distribution.jpg)

*Figure 2.5 — Distribution by pattern_style*

### Explanation
The distribution by weapon type shows that certain rifles appear much more frequently in the dataset than others. This reflects real market supply, but also implies that the model will learn pricing patterns for common weapons more accurately than for rare ones.

The wear distribution indicates that Factory New and Minimal Wear conditions dominate the listings, which is expected since visually appealing skins are more frequently traded in better condition.

The pattern_style distribution highlights the extreme rarity of true blue_gem patterns compared to blue_mix and other patterns.

### Analytical Conclusion
- the dataset exhibits class imbalance across multiple categorical dimensions;
- higher prediction uncertainty is expected for rare weapons and rare pattern styles;
- model evaluation must be interpreted with this imbalance in mind.

---

## 2.4 Distribution of Blue-Related Features

### Purpose
To analyze the variability and rarity of blue color dominance, which is the central factor in Case Hardened pricing.

### Figure Placeholders
![Playside blue](docs/assets/images/ML/ch_gun/eda_playside_blue_distribution.jpg)

*Figure 2.6 — Distribution of playside_blue*

![Backside blue](docs/assets/images/ML/ch_gun/eda_backside_blue_distribution.jpg)

*Figure 2.7 — Distribution of backside_blue*

![Blue score](docs/assets/images/ML/ch_gun/eda_blue_score_distribution.jpg)

*Figure 2.8 — Distribution of blue_score*

![Blue tier](docs/assets/images/ML/ch_gun/eda_blue_tier_distribution.jpg)

*Figure 2.9 — Distribution of blue_tier*

### Explanation
The distributions of playside_blue and backside_blue show that high blue coverage is uncommon, especially on the playside, which is the visually dominant side of the weapon. Most skins have relatively low blue percentages, while only a small fraction exhibit extensive blue areas.

The blue_score distribution amplifies this observation by combining information from both sides with a higher weight for playside visibility. As a result, very high blue_score values are rare and correspond to visually dominant Blue Gem patterns.

The discrete blue_tier distribution further confirms this rarity: tier 3 skins appear significantly less frequently than lower tiers.

### Analytical Conclusion
The observed distributions accurately reflect the real-world scarcity structure of Case Hardened patterns and justify the strong price premium associated with high blue dominance.

---

## 2.5 Correlation Analysis of Numerical Features

### Purpose
To identify linear relationships between numerical variables and to obtain a first approximation of feature relevance.

### Figure Placeholder
![Correlation matrix](docs/assets/images/ML/ch_gun/eda_numeric_correlation_matrix.jpg)

*Figure 2.10 — Correlation matrix of numerical features*

### Explanation
The correlation matrix shows a clear positive association between blue-related features (blue_score and blue_tier) and price, confirming that increased blue dominance is generally associated with higher market value.

A negative correlation is observed between float value and price, indicating that increased wear tends to reduce skin value. Sticker-related features exhibit a positive correlation with price, reflecting their additive effect.

At the same time, none of the correlations are close to ±1, suggesting that price formation depends on complex interactions rather than simple linear effects.

### Analytical Conclusion
Moderate correlation values indicate that linear models are insufficient and motivate the use of non-linear machine learning methods capable of capturing feature interactions.

---

## 2.6 Relationship Between Blue Score and Price

### Purpose
To analyze how the aggregated measure of blue dominance (blue_score) influences the market price of Case Hardened skins and to assess whether this relationship is linear or non-linear.

### Figure Placeholder
![Blue score vs price](docs/assets/images/ML/ch_gun/eda_blue_score_vs_price.jpg)

*Figure 2.11 — Relationship between blue_score and price*

### Explanation
The scatter plot demonstrates a clear positive relationship between blue_score and price. As the value of blue_score increases, the average price level rises significantly. At low values of blue_score, prices are relatively concentrated within a narrow range, corresponding to standard, non-rare patterns.

As blue_score increases, the dispersion of prices also increases. This indicates that while high blue dominance strongly increases the expected value of a skin, the final price is additionally influenced by other factors such as weapon type, float value, presence of StatTrak, and sticker composition. In the upper range of blue_score, several extreme price values are observed, corresponding to rare Blue Gem patterns.

The relationship is clearly non-linear: price growth accelerates for higher values of blue_score, which suggests multiplicative effects rather than a simple linear dependence.

### Analytical Conclusion
Blue_score is one of the most influential numerical features in the dataset and acts as a primary driver of price formation for Case Hardened skins. However, the large variance observed at higher values confirms that accurate price prediction requires combining blue_score with contextual features and non-linear machine learning models.

---

## 2.7 Relationship Between Blue Tier and Price

### Purpose
To validate the discrete categorization of blue dominance and assess its impact on market price.

### Figure Placeholder
![Blue tier vs price](docs/assets/images/ML/ch_gun/eda_blue_tier_vs_price.jpg)

*Figure 2.12 — Price distribution by blue_tier*

### Explanation
The boxplot shows a clear monotonic relationship between blue_tier and price. Lower tiers (0 and 1) are associated with relatively low and tightly clustered prices, while tier 2 exhibits a noticeable increase in both median price and variability.

Tier 3 (Blue Gem) stands out with significantly higher median prices and a wide upper range, reflecting strong collector demand and speculative pricing.

### Analytical Conclusion
The blue_tier feature provides an interpretable and effective discretization of blue dominance and captures meaningful price differences between pattern categories.

---

## 2.8 Impact of StatTrak on Price

### Purpose
To quantify the influence of the StatTrak attribute on Case Hardened skin prices.

### Figure Placeholder
![StatTrak vs price](docs/assets/images/ML/ch_gun/eda_stattrak_vs_price.jpg)

*Figure 2.13 — Price distribution with and without StatTrak*

### Explanation
The boxplot comparison shows that StatTrak-enabled skins tend to have higher median prices than their non-StatTrak counterparts. This effect is observed across most price ranges, although it is generally smaller than the effect of blue dominance.

The presence of StatTrak adds an additional collectible component to the skin, which is reflected in higher market valuation.

### Analytical Conclusion
StatTrak has a consistent positive effect on price and should be included as a binary explanatory feature in the model.

---

## 2.9 Average Price by Weapon Type

### Purpose
To estimate baseline price differences between weapon models independent of pattern characteristics.

### Figure Placeholder
![Mean price by weapon](docs/assets/images/ML/ch_gun/eda_mean_price_by_weapon.jpg)

*Figure 2.14 — Mean price by weapon type*

### Explanation
The mean price varies substantially across different weapon types. Premium rifles exhibit significantly higher average prices than more common weapons, even when pattern-related characteristics are similar.

This reflects the inherent base value of the weapon itself, onto which pattern-specific premiums are added.

### Analytical Conclusion
Weapon type defines a baseline price level, while Case Hardened pattern features act as multiplicative modifiers. Therefore, weapon type is a critically important categorical feature for price prediction.

---

## 2.10 EDA Summary

The exploratory data analysis confirms that Case Hardened pricing is primarily driven by blue dominance, with additional influence from weapon type, wear, StatTrak, and stickers. These findings directly motivate the subsequent machine learning methodology.


# Machine Learning Models and Experiments

This chapter describes the machine learning methodology used to predict prices of CS2 Case Hardened skins.  
Special emphasis is placed on **justification of design choices**, model selection, and comparative analysis, which is essential for an academic diploma project.

---

## 3.1 Data Preprocessing and Train/Test Split

To build and evaluate machine learning models, the dataset was split into **training and test subsets** using an 80/20 ratio.  
As a result, **6,709 samples** were used for training and **1,678 samples** were reserved for testing.

The choice of this splitting strategy is motivated by several factors.

First, the dataset does not contain any **temporal structure**. Skin prices are collected as independent market observations rather than time series. Therefore, a chronological split would not provide additional benefits, while random shuffling ensures that all price ranges and pattern types are represented in both subsets.

Second, the dataset includes **rare but economically significant patterns**, such as Blue Gem Case Hardened skins. A random split increases the probability that such rare items appear in both training and test sets, allowing the model to learn their characteristics while still being evaluated on unseen high-value samples.

Third, reserving a sufficiently large test set ensures that the reported metrics represent **true generalization performance**, rather than optimistic in-sample estimates.

Categorical features (weapon type, wear category, pattern style, StatTrak) were passed to CatBoost using explicit categorical indices.  
This design choice is critical: instead of applying one-hot encoding, CatBoost internally uses **ordered target statistics**, which reduces dimensionality, mitigates overfitting, and preserves meaningful category-level information.

A separate validation set was not introduced explicitly. Instead, the test set was reused as an evaluation set for **early stopping**.  
This decision was made to avoid excessive data fragmentation, which is undesirable for medium-sized datasets. CatBoost’s built-in regularization, together with early stopping, provides sufficient protection against overfitting in this setting.

---

## 3.2 Baseline CatBoost Model

### 3.2.1 Model Motivation

CatBoost was selected as the primary non-linear model due to its strong empirical performance on structured tabular data.  
Unlike linear models, gradient boosting decision trees are capable of capturing:

- non-linear dependencies between features and price;
- interaction effects (e.g., between weapon type and blue dominance);
- threshold-like behavior common in market pricing.

These properties are especially important for Case Hardened skins, where price formation is driven by a combination of visual rarity, categorical attributes, and multiplicative effects rather than simple additive rules.

Additionally, CatBoost offers native support for categorical variables, eliminating the need for manual encoding and reducing the risk of information leakage.

---

### 3.2.2 Baseline Configuration

The baseline CatBoost regressor was trained using manually selected hyperparameters:

- **tree depth = 8**, providing sufficient capacity to model feature interactions without excessive complexity;
- **learning rate = 0.05**, ensuring stable and gradual convergence;
- **1,500 boosting iterations**, combined with early stopping;
- **RMSE** used as both the loss function and evaluation metric.

The choice of RMSE is deliberate: unlike MAE, RMSE penalizes large errors more strongly. This is particularly important in this domain, where underestimating expensive skins is more problematic than small errors on low-priced items.

Early stopping was enabled to prevent overfitting, especially given the presence of rare, extreme price values.

---

### 3.2.3 Baseline Performance

The baseline CatBoost model achieved the following results on the test set:

- **MAE:** 574.59 USD  
- **RMSE:** 3,579.18 USD  
- **R²:** 0.3695  

These results demonstrate a substantial improvement over linear models (discussed later), confirming that non-linear modeling is necessary.  
However, the relatively moderate R² value indicates that a significant portion of price variance remains unexplained, motivating further optimization.

---

## 3.3 Hyperparameter Optimization with Optuna

### 3.3.1 Optimization Strategy

To improve predictive performance, hyperparameter optimization was performed using **Optuna**, which implements Bayesian optimization.

Unlike grid search, Bayesian optimization adaptively explores the hyperparameter space by focusing on promising regions. This is particularly suitable for complex models such as CatBoost, where exhaustive search is computationally inefficient.

The optimization objective was to **minimize RMSE**, reinforcing the focus on reducing large prediction errors for high-value skins.

The following hyperparameters were optimized:

- tree depth (model complexity);
- learning rate (training dynamics);
- number of boosting iterations;
- L2 regularization strength (overfitting control).

A total of **20 optimization trials** were conducted, providing a balance between search depth and computational cost.

---

### 3.3.2 Tuned CatBoost Model

The best-performing configuration identified by Optuna included:

- **depth = 8**  
- **learning rate ≈ 0.147**  
- **iterations = 1,371**  
- **L2 regularization ≈ 9.22**

Compared to the baseline, this configuration increases regularization strength while allowing faster learning, resulting in improved generalization.

---

### 3.3.3 Tuned Model Performance

The tuned CatBoost model achieved the following results:

- **MAE:** 446.36 USD  
- **RMSE:** 2,645.68 USD  
- **R²:** 0.6243  

Compared to the baseline model, RMSE was reduced by more than **900 USD**, and explained variance increased substantially.  
This confirms that hyperparameter optimization is not merely a cosmetic step but a critical component of achieving competitive performance.

---

## 3.4 Linear Regression Baseline

### 3.4.1 Purpose of Linear Model

A linear regression model with one-hot encoded categorical features was trained as a classical baseline.  
Its role is not to achieve high accuracy, but to provide a **reference point** and test whether simple linear assumptions are sufficient for this problem.

---

### 3.4.2 Linear Model Performance

The linear regression model produced the following results:

- **MAE:** 1,012.46 USD  
- **RMSE:** 4,272.35 USD  
- **R²:** 0.1017  

The model systematically underestimates expensive skins and fails to capture non-linear price jumps associated with rare patterns.

---

### 3.4.3 Conclusion

The poor performance of linear regression clearly demonstrates that Case Hardened pricing cannot be modeled using linear relationships.  
This justifies the transition to more expressive non-linear models.

---

## 3.5 Model Comparison

Comparing all evaluated models leads to clear conclusions:

- **Linear Regression** severely underfits the data and fails to model market complexity.
- **Baseline CatBoost** captures non-linear dependencies and significantly improves accuracy.
- **Tuned CatBoost** provides the best balance between bias and variance.

The tuned CatBoost model consistently outperforms all alternatives across all metrics.

---

## 3.6 Interpretability and Model Validation

To validate that the model learns meaningful patterns rather than noise, feature importance and SHAP analysis were applied.

The analysis confirms that:

- `blue_score` and `blue_tier` are the dominant price drivers;
- weapon type establishes a baseline price level;
- float value and StatTrak act as secondary modifiers.

These results align closely with the findings from exploratory data analysis, providing additional confidence in model validity.

---

## 3.7 Inference Speed and Practical Considerations

Inference time measurements indicate that:

- linear regression is fastest but inaccurate;
- the tuned CatBoost model provides high accuracy with acceptable latency;
- simplified CatBoost configurations can be used when faster inference is required.

This allows flexible deployment depending on application constraints.

---

## 3.8 Final Model Selection

Based on predictive accuracy, robustness to outliers, interpretability, and inference efficiency,  
the **tuned CatBoost regressor** is selected as the final model for Case Hardened price prediction.

---

## 3.9 Summary

This machine learning study demonstrates that:

- non-linear models are essential for modeling Case Hardened prices;
- gradient boosting significantly outperforms linear approaches;
- hyperparameter optimization yields substantial gains;
- the final model is both empirically strong and economically interpretable.
