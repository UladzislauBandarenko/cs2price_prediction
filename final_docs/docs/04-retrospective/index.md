# Retrospective
## Retrospective
This section reflects on the development of the **CS2 Skin Price Prediction System**, summarizing achieved results, encountered limitations, architectural trade-offs, and lessons learned. The project was implemented as a **production-ready ML system**, covering the full lifecycle from raw data processing to stable inference.

## Project Status Overview

By the end of the research phase, the project has reached a **stable and fully functional state** with the following characteristics:

- A **working production system** capable of predicting prices for selected CS2 skin families.
- **Six independent machine learning models**, each tailored to a specific skin type or family.
- Models demonstrate **high predictive performance** on validation data and align well with real market behavior.
- The system operates on a **static dataset and fixed models**, without daily retraining or live data updates.
- **Data collection was completed on September 25**, after which the dataset was frozen to ensure stability, reproducibility, and consistent model training.
- The current version of the system represents a **stable research snapshot**, defining a clear temporal boundary for analysis and evaluation.

## What Went Well

### Technical Successes

- Adoption of **multiple specialized ML models** significantly outperformed a single universal predictor.
- **CatBoost** proved highly effective for tabular data with high-cardinality categorical features.
- The full ML lifecycle was implemented: data acquisition → EDA → feature engineering → training → evaluation → deployment.
- **Low-latency inference (<50 ms)** achieved, suitable for real-time usage.
- Clean separation via a **FastAPI-based ML microservice**.
- Model behavior validated using **SHAP** for interpretability.

### Process Successes

- Exploratory data analysis strongly influenced architectural decisions.
- Iterative experimentation enabled fast validation of assumptions.
- Continuous documentation improved system maintainability.
- Focus on production realism rather than purely academic metrics.

### Personal Achievements

- Practical experience with **real-world ML system design**.
- Improved handling of noisy and incomplete market data.
- Stronger ability to justify ML decisions technically and economically.
- Enhanced communication of ML concepts to non-technical audiences.

## What Didn’t Go as Planned

| Planned | Actual Outcome | Cause | Impact |
|------|----------------|------|--------|
| Unified ML model | 6 specialized models | Domain heterogeneity | Medium |
| Continuous retraining | Static models | Fixed dataset after collection cutoff | Medium |
| Large datasets | Smaller, cleaner data | Strict quality filtering | Low |
| Simple ingestion | Complex pipeline | No public historical API | High |

### Key Challenges

1. **Data Availability**
   - No official public API for historical sold prices.
   - Reliance on third-party marketplace data.
   - Strict filtering to preserve label quality.

2. **Market Volatility**
   - Prices affected by updates, hype, and case changes.
   - Required robust models rather than short-term trend fitting.

3. **Temporal Fixation of Data**
   - Data collection was intentionally finalized on September 25.
   - All models were trained on the same frozen dataset to ensure experimental consistency.

## Technical Debt & Known Limitations

| ID | Issue | Severity | Description | Potential Improvement |
|----|------|----------|-------------|----------------------|
| TD-001 | Static dataset | Medium | No automatic updates after cutoff date | Periodic dataset refresh |
| TD-002 | Manual retraining | Medium | No ML CI/CD pipeline | Automated retraining |
| TD-003 | Feature duplication | Low | Repeated schemas | Central feature registry |

## Future Development Directions

### Short-Term

1. Extend data preparation to **additional skin families**.
2. Introduce **prediction confidence intervals**.

### Long-Term

3. Integrate the pricing service into **skin trading platforms**.
4. Develop a **standalone pricing service** for end users.

## Lessons Learned

### Technical

- Data quality is more important than data volume.
- Domain specialization significantly improves accuracy.
- Interpretability is essential for trust.
- ML systems must be designed with production constraints in mind.

### Process

- EDA should precede architecture decisions.
- Early assumptions must be validated quickly.
- Documentation is a core component of system stability.

# Final Conclusion

This thesis explored the use of machine learning methods for estimating the market value of virtual items, using Counter-Strike 2 (CS2) skins as a case study. The relevance of this work is driven by the high volatility of the in-game item market and the lack of transparent pricing tools.

## Achieved Results {.unnumbered .unlisted}

During the course of the project, the following results were achieved:

1. **Market Data Collection and Preparation**  
   A comprehensive dataset of CS2 skin market data was collected and processed in cooperation with a partner trading platform.  
   Data collection was **completed on September 25**, after which the dataset was fixed to ensure stable training conditions and reproducibility of results.

2. **Machine Learning Models**  
   Multiple machine learning models were trained on high-quality market data.  
   The use of several specialized models instead of a single universal approach resulted in improved robustness and stable predictive performance for selected skin categories.

3. **Application Logic and Data Storage**  
   The full logic for user interaction with the prediction system was implemented, including request handling, access to trained models, and result delivery.  
   A structured relational database was designed to support data storage and system operations.

4. **System Architecture**  
   A functional application architecture was developed, providing a foundation for integrating data processing, model inference, and user-facing components within a unified system.

## Limitations and Non-Implemented Components {.unnumbered .unlisted}

Despite the achieved results, several components were intentionally not implemented within the scope of the diploma project:

1. **Continuous Data Updates**  
   The system does not perform continuous or live data updates.  
   This decision was made to preserve experimental consistency after the completion of data collection.

2. **Regular Model Retraining**  
   Automated periodic retraining was not enabled, as the project operates on a fixed dataset defined by the research cutoff date.

## Temporal Scope and Validity {.unnumbered .unlisted}

As a result, the developed solution represents a **stable and reproducible research snapshot** of the CS2 skin market, valid **as of September 25, 2025**.  
This clearly defined temporal scope ensures transparency regarding the applicability and limitations of the presented results.

## Overall Assessment {.unnumbered .unlisted}

- The project meets its research objectives and demonstrates the practical applicability of machine learning for price estimation in digital markets.
- The dataset collection strategy proved to be sufficient for training predictive models.
- Accurate price prediction is feasible for selected CS2 skin families.
- Model performance is limited by market volatility, data availability, and concept drift.
- Significant market changes require additional data and retraining for effective model adaptation.
