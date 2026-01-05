#  Retrospective
##  Retrospective
This section reflects on the development of the **CS2 Skin Price Prediction System**, summarizing achieved results, encountered limitations, architectural trade-offs, and lessons learned. The project was implemented as a **production-ready ML system**, covering the full lifecycle from raw data processing to stable inference.


### Project Status Overview

By  the end of the research phase, the project has reached a **stable and fully functional state** with the following:

- A **working production system** capable of predicting prices for selected CS2 skin families.
- **Six independent machine learning models**, each tailored to a specific skin type or family.
- Models demonstrate **high predictive performance** on validation data and align well with real market behavior.
- The system operates on a **static dataset and fixed models**, without daily retraining or live data updates.
- This limitation is caused by **data provider NDA restrictions**, which prohibit automated continuous data ingestion.
- The current version of the system is considered **stable and fixed until September 25, 2025**, defining a clear research boundary.


### What Went Well

#### Technical Successes

- Adoption of **multiple specialized ML models** significantly outperformed a single universal predictor.
- **CatBoost** proved highly effective for tabular data with high-cardinality categorical features.
- Full ML lifecycle implemented: data acquisition → EDA → feature engineering → training → evaluation → deployment.
- **Low-latency inference (<50 ms)** achieved, suitable for real-time usage.
- Clean separation via a **FastAPI-based ML microservice**.
- Model behavior validated using **SHAP** for interpretability.

#### Process Successes

- EDA strongly influenced architectural decisions.
- Iterative experimentation enabled fast validation of assumptions.
- Continuous documentation improved system maintainability.
- Focus on production realism over purely academic metrics.

#### Personal Achievements

- Practical experience with **real-world ML system design**.
- Improved handling of noisy and incomplete market data.
- Stronger ability to justify ML decisions technically and economically.
- Enhanced communication of ML concepts to non-technical audiences.


### What Didn’t Go as Planned

| Planned | Actual Outcome | Cause | Impact |
|------|----------------|------|--------|
| Unified ML model | 6 specialized models | Domain heterogeneity | Medium |
| Continuous retraining | Static models | NDA restrictions | Medium |
| Large datasets | Smaller, cleaner data | Limited access to sales | Low |
| Simple ingestion | Complex pipeline | No public API | High |

#### Key Challenges

1. **Data Availability**
   - No official API for historical sold prices.
   - Reliance on third-party marketplace data.
   - Strict filtering to preserve label quality.

2. **Market Volatility**
   - Prices affected by updates, hype, and case changes.
   - Required robust models over short-term trend fitting.

3. **NDA-Driven Constraints**
   - Automated updates and retraining not permitted.
   - Resulted in a deliberately static but stable system.


### Technical Debt & Known Limitations

| ID | Issue | Severity | Description | Potential Improvement |
|----|------|----------|-------------|----------------------|
| TD-001 | Static datasets | Medium | No automatic updates | Scheduled retraining |
| TD-002 | Manual retraining | Medium | No ML CI/CD | Automated pipelines |
| TD-003 | Feature duplication | Low | Repeated schemas | Feature registry |


### Future Development Directions

#### Short-Term

1. Extend data preparation to **additional skin families**.
2. Introduce **prediction confidence intervals**.

#### Long-Term

3. Integrate pricing service into **skin trading platforms**.
4. Develop a **standalone pricing service** for end users.


### Lessons Learned

#### Technical

- Data quality is more important than data volume.
- Domain specialization significantly improves accuracy.
- Interpretability is essential for trust.
- ML systems must be designed with production constraints in mind.

#### Process

- EDA should precede architecture decisions.
- Early assumptions must be validated quickly.
- Documentation is a core part of system stability.


# Final Conclusion

This thesis explored the use of machine learning methods for estimating the market value of virtual items, using Counter-Strike 2 (CS2) skins as a case study. The relevance of this work is driven by the high volatility of the in-game item market and the lack of transparent pricing tools.

## Achieved Results {.unnumbered .unlisted}

During the course of the project, the following results were achieved:

1. **Market Data Collection and Preparation**  
   A large dataset of CS2 skin market data was collected and processed in cooperation with a partner trading platform. Due to non-disclosure agreements (NDA), the dataset does not include exact timestamps of individual sales; however, the available data proved sufficient for reliable analysis and model training.

2. **Machine Learning Models**  
   Multiple machine learning models were trained on a substantial volume of high-quality market data. The use of several specialized models instead of a single universal approach resulted in improved robustness and stable predictive performance for selected skin categories.

3. **Application Logic and Data Storage**  
   The full logic for user interaction with the prediction system was implemented, including request handling, access to trained models, and result delivery. A structured relational database was designed to support data storage and system operations.

4. **System Architecture**  
   A functional application architecture was developed, providing a foundation for integrating data processing, model inference, and user-facing components within a unified system.

## Limitations and Non-Implemented Components {.unnumbered .unlisted}

Despite the achieved results, several components were intentionally not implemented within the scope of the diploma project:

1. **Continuous Data Updates**  
   The system does not perform continuous or live data updates. This limitation is directly related to restrictions imposed by the data provider and the terms of the NDA, which prevent the disclosure and use of continuously refreshed market data in an academic context.

2. **Regular Model Retraining**  
   Automated periodic retraining of models (e.g., on a 24-hour cycle) was not enabled, as it requires continuous access to updated transactional data, which was not permitted under the existing data-sharing agreements.


## Temporal Scope and Validity {.unnumbered .unlisted}

As a result of these constraints, the developed solution represents a stable and reproducible research snapshot of the CS2 skin market, valid **until September 25, 2025**. This clearly defined temporal scope ensures transparency regarding the applicability and limitations of the presented results.


## Overall Assessment {.unnumbered .unlisted}

- The project meets its research objectives and demonstrates the practical applicability of machine learning for price estimation in digital markets.

- The dataset collection strategy proved to be appropriate and sufficient for training predictive models.

- Accurate price prediction is feasible for selected CS2 skin families.

- Model performance is limited by market volatility, data availability, and concept drift.

- Significant market changes require additional time and data for effective model adaptation.