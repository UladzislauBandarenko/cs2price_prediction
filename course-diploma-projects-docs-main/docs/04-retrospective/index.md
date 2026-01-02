# 4. Retrospective

This section reflects on the development of the **CS2 Skin Price Prediction System**,
summarizing key successes, encountered difficulties, technical trade-offs, and lessons learned.
The project was developed as a full-cycle ML system, from raw data acquisition to production-ready inference.

---

## What Went Well ✅

### Technical Successes

- Successfully built **multiple specialized ML models** instead of a single universal predictor, which significantly improved accuracy.
- Chosen **CatBoost** proved to be a strong fit for tabular data with high-cardinality categorical features.
- End-to-end ML pipeline was implemented: data collection → EDA → feature engineering → training → evaluation → deployment.
- Real-time inference performance (<50 ms) achieved without sacrificing model quality.
- Clean separation between ML logic and backend via a dedicated **FastAPI ML microservice**.

### Process Successes

- Incremental development with early validation of assumptions using EDA.
- Clear structuring of documentation alongside implementation.
- Continuous validation of model outputs against real market intuition.
- Focus on production realism instead of academic-only metrics.

### Personal Achievements

- Deep understanding of **real-world ML system design**, not just model training.
- Hands-on experience with noisy, biased, and incomplete real market data.
- Improved ability to justify architectural decisions technically and economically.
- Stronger skills in explaining ML systems in clear, non-technical language.

---

## What Didn't Go As Planned ⚠️

| Planned | Actual Outcome | Cause | Impact |
|-------|----------------|-------|--------|
| Unified ML model | Multiple specialized models | Strong domain differences | Medium |
| Large datasets | Smaller but cleaner datasets | Access to real sales is limited | Low |
| Simple data collection | Complex data acquisition | No public historical API | High |
| Linear baseline sufficient | Non-linear models required | Complex price formation | Low |

### Challenges Encountered

1. **Data Collection Complexity**
   - Problem: No official API for historical sold prices.
   - Impact: Significant time spent acquiring reliable data.
   - Resolution: Cooperation with third-party marketplace data and strict filtering.

2. **Market Noise and Volatility**
   - Problem: Prices change due to external events (updates, cases, hype).
   - Impact: Increased variance in labels.
   - Resolution: Focus on robust models and real transaction prices.

3. **Feature Engineering Difficulty**
   - Problem: Visual attributes (float, pattern) are non-linear and context-dependent.
   - Impact: Required multiple iterations of EDA and model refinement.
   - Resolution: Domain-driven feature design and SHAP validation.

---

## Technical Debt & Known Issues

| ID | Issue | Severity | Description | Potential Fix |
|----|------|----------|-------------|---------------|
| TD-001 | Manual feature schemas | Medium | Features duplicated across models | Centralized schema registry |
| TD-002 | Limited automated tests | Medium | Focus was on ML quality | Add unit tests for pipelines |
| TD-003 | Static datasets | Low | No live retraining | Scheduled retraining pipeline |

### Code Quality Issues

- Some feature engineering logic could be further modularized.
- Notebook-to-production code duplication exists in early experiments.
- Model retraining is manual rather than automated.

---

## Future Improvements (Backlog)

### High Priority

1. **Automated Data Pipeline**
   - Description: Scheduled ingestion of new sold transactions.
   - Value: Keeps models up-to-date with market trends.
   - Effort: High.

2. **Confidence Intervals**
   - Description: Provide uncertainty bounds for predictions.
   - Value: Improves user trust and interpretability.
   - Effort: Medium.

### Medium Priority

3. **Online Monitoring**
   - Description: Track prediction drift and performance degradation.
   - Value: Early detection of model decay.

4. **Model Versioning**
   - Description: Explicit version control for deployed models.
   - Value: Safer updates and rollbacks.

### Nice to Have

5. User-facing explanations of price drivers.
6. Historical price trend visualization.
7. A/B testing between model versions.

---

## Lessons Learned

### Technical Lessons

| Lesson | Context | Application |
|------|--------|-------------|
| Real data > big data | Listings are misleading | Always prioritize label quality |
| Specialization matters | One model ≠ all cases | Use domain-aware models |
| Interpretability is critical | Stakeholder trust | Always validate with SHAP |
| ML ≠ just training | Deployment & latency | Think production-first |

### Process Lessons

| Lesson | Context | Application |
|------|--------|-------------|
| EDA drives architecture | Early insights mattered | Always explore first |
| Documentation saves time | Complex system | Document decisions |
| Iterative design wins | Wrong early assumptions | Validate fast |

### What Would Be Done Differently

| Area | Current Approach | What Would Change | Why |
|-----|------------------|------------------|-----|
| Planning | Model-first | Data-first | Data constraints dominate |
| Scope | Broad categories | Even more specialization | Improves accuracy |
| Automation | Manual retraining | CI-based retraining | Scalability |
| Testing | Post-hoc | Earlier testing | Faster iteration |

---

## Personal Growth

### Skills Developed

| Skill | Before Project | After Project |
|------|----------------|---------------|
| Machine Learning | Intermediate | Advanced |
| Data Engineering | Beginner | Intermediate |
| System Architecture | Beginner | Intermediate |
| Technical Writing | Intermediate | Advanced |

### Key Takeaways

1. Real-world ML is mostly about **data quality**, not algorithms.
2. Domain knowledge is as important as model choice.
3. Production constraints must be considered from day one.

---

*Retrospective completed: 2025*
