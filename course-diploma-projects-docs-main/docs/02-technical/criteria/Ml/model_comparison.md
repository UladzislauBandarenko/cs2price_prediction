# Model Comparison and Selection

## Motivation for Multiple Specialized Models

A central design decision of this project was the use of **multiple specialized machine learning models**
instead of a single universal predictor.

This choice is motivated by the nature of the CS2 skin market, where different item categories exhibit
fundamentally different price formation mechanisms.

In particular:
- different skin types rely on different dominant visual and numerical features;
- feature importance varies significantly across categories;
- strong non-linear and category-specific interactions are present.

As a result, a single global model would either:
- underfit specialized patterns, or
- overfit dominant categories at the expense of rarer ones.

To address this, the system employs **separate models**, each optimized for a specific market segment:
- Case Hardened weapons
- Case Hardened knives
- Doppler knives
- Fade knives
- Fade weapons
- Float-sensitive weapons

This modular approach improves both predictive accuracy and interpretability.

---

## Algorithm Selection: CatBoost

CatBoost was selected as the primary algorithm for all models based on both empirical results
and domain-specific requirements.

Key reasons for choosing CatBoost include:
- native handling of categorical features without one-hot encoding;
- strong performance on heterogeneous tabular data;
- robustness on small-to-medium-sized datasets;
- ability to model complex non-linear feature interactions;
- fast and stable inference suitable for production systems.

CatBoost also provides built-in tools for:
- feature importance analysis;
- SHAP-based explainability;
- early stopping and regularization.

These properties make it particularly well-suited for CS2 pricing data.

---

## Alternative Algorithms Considered

Several alternative algorithms were evaluated during the design phase.

| Algorithm | Reason for Rejection |
|---------|---------------------|
| Linear Regression | Unable to capture non-linear price dynamics |
| Random Forest | Weak handling of high-cardinality categorical features |
| XGBoost | Requires extensive feature encoding and preprocessing |
| Neural Networks | High risk of overfitting and low interpretability |

While some of these approaches performed adequately on subsets of the data,
none provided the same balance of accuracy, stability, and interpretability as CatBoost.

---

## Accuracy vs Latency Trade-off

In addition to predictive accuracy, **inference latency** was a critical constraint.

Design targets included:
- average prediction latency below 50 ms;
- models fully loaded in memory;
- no blocking of external APIs or services.

Through model size control and optimized inference configurations,
all CatBoost models meet these requirements.

This balance ensures that the system remains suitable for real-time or near-real-time usage
without sacrificing predictive quality.

---

## Architectural Implications

The choice of multiple specialized models directly influenced system architecture.

The machine learning layer is implemented as a **dedicated FastAPI microservice**, which provides:
- independent horizontal scaling;
- a clear and stable API contract;
- isolation of heavy ML dependencies from the core application.

This separation improves maintainability, deployment flexibility,
and aligns with modern best practices for production ML systems.

---

## Summary

The model selection strategy combines:
- domain-driven decomposition of the problem,
- a unified and well-justified algorithm choice,
- careful consideration of accuracy–latency trade-offs.

As a result, the final system achieves high predictive performance,
clear interpretability, and production readiness across multiple CS2 market segments.
