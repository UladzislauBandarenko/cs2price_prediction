# Test Plan — cs2price_prediction

## 1. Testing Approach and Project Type

The **cs2price_prediction** project uses a **combined (mixed) testing approach**, which includes both **quantitative** and **qualitative** testing.

### 1.1 Quantitative Testing
Quantitative testing focuses on:
- Measuring the performance of the machine learning model using metrics such as:
  - MAE (Mean Absolute Error)
  - RMSE (Root Mean Squared Error)
  - MAPE (Mean Absolute Percentage Error)
  - R² (Coefficient of Determination)
- Collecting measurable testing metrics:
  - Pass rate of executed test cases
  - Average response time of API endpoints
  - Number and severity of discovered defects

### 1.2 Qualitative Testing
Since the system does not include a graphical UI, qualitative testing is focused on:
- Evaluating the usability and clarity of the API documentation (Swagger)
- Checking the consistency and readability of request/response schemas
- Assessing the usefulness and informativeness of error messages

This mixed approach ensures both measurable model evaluation and thorough API quality assessment.

---

## 2. Structured Testing Workflow

The testing process follows a structured workflow consisting of seven stages:

1. **Requirements and API Analysis**  
   - Identify and document all testable endpoints and system components (see `docs/requirements.md`).

2. **Test Design**  
   - Prepare manual test cases for all API endpoints (positive, negative, boundary cases).
   - Define test case priorities (Smoke, High, Medium, Low).
   - Select testing and model evaluation metrics.

3. **Environment Setup**  
   - Deploy the API locally at `http://localhost:8087`.
   - Ensure that Swagger UI is available and functioning.
   - Create a Postman collection (`tools/postman_explain(meta predict).json`).

4. **Test Execution**  
   - Execute API test cases through Swagger and Postman.
   - Record actual results, measure response times, and verify schema correctness.
   - Calculate prediction model performance metrics.

5. **Defect Logging**  
   - Register defects in GitHub Issues or in the test report (`docs/test_report.md`).
   - Assign defect severity levels (Critical, High, Medium, Low).

6. **Retesting & Verification**  
   - Re-execute tests for resolved defects.
   - Document unresolved limitations (if out of scope).

7. **Final Reporting**  
   - Prepare a structured final test report summarizing:
     - Test results
     - Defects found and resolved
     - Model metrics
     - Environment details
     - Recommendations and conclusions

This workflow ensures consistency, reproducibility, and transparency across the entire testing process.

---

## 3. Test Scope, Goals, Objectives, and Entry/Exit Criteria

### 3.1 Test Scope (In Scope)

The following API endpoints are included in the testing scope:

#### **Prediction & AI Explanation**
- `POST /api/predict` — main price prediction endpoint  
- `POST /api/ai/explain` — model prediction explanation (v1)  
- `POST /api/ai/explain-v2` — model prediction explanation (v2)

#### **Metadata Endpoints**
- `GET /api/meta/weapon-types`
- `GET /api/meta/weapon-types/{weaponTypeId}/weapons`
- `GET /api/meta/weapons/{weaponId}/skins`
- `GET /api/meta/skins/{skinId}/wear-tiers`
- `GET /api/meta/skins/{skinId}/patterns`
- `GET /api/meta/stickers?q=&limit=`

#### **Included Testing Areas**
- Functional correctness of all listed endpoints  
- Input validation and error handling (400/404 cases)  
- Consistency of JSON schemas  
- Swagger documentation clarity  
- Prediction model metric evaluation  
- Basic response time measurements

### 3.2 Out of Scope

- Retraining or modifying the ML model  
- UI testing (no user interface exists)  
- Full-scale performance or load testing  
- Security testing beyond basic validation

---

## 3.3 Testing Goals

- Verify that all API endpoints respond correctly to valid and invalid inputs.  
- Confirm that error messages are informative and consistent.  
- Measure and document ML model performance using established metrics.  
- Validate completeness and usability of Swagger API documentation.  
- Identify defects and provide meaningful recommendations.

---

## 3.4 Testing Objectives

- Create a comprehensive set of manual test cases.  
- Execute all test cases and record results.  
- Measure system performance and model accuracy.  
- Log discovered defects with severity classification.  
- Produce a final test execution report.

---

## 3.5 Entry Criteria

Testing begins when the following conditions are met:

- The API service is running and accessible at `http://localhost:8087`.  
- Swagger UI is available and endpoints are documented.  
- The ML model is loaded and `/api/predict` returns valid output.  
- Test cases are prepared (`docs/test_cases.md`).  
- Postman collection is created.

---

## 3.6 Exit Criteria

Testing is considered complete when:

- ≥ 95% of High-priority test cases are executed.  
- No Critical or High defects remain unresolved.  
- All model metrics (MAE, RMSE, MAPE, R²) are calculated and documented.  
- Swagger documentation has been reviewed and evaluated.  
- The final test report (`docs/test_report.md`) is completed.

---

## 4. Conclusion

This test plan defines the structured workflow, scope, goals, and criteria needed to evaluate the **cs2price_prediction** system. It ensures that testing is systematic, reproducible, and aligned with the project’s complexity and risks.
