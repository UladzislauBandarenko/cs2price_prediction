## Qualitative and Quantitative Testing Technical Documentation
  


###  Introduction

Within the scope of the academic discipline **“Qualitative or Quantitative Testing”**, the **cs2price_prediction** system was evaluated using a combined testing approach. The objective of this testing phase was to assess the system from both quantitative and qualitative perspectives, covering machine learning accuracy, API correctness, performance, and usability.

The system under test is a machine learning–based solution designed to predict market prices of CS2 skins and expose the prediction logic via a REST API. Due to the complexity and volatility of the CS2 market, comprehensive testing was required to validate both numerical performance and user-facing behavior.


###  Quantitative Testing

####  Quantitative Testing of Machine Learning Models

Quantitative testing was primarily applied to evaluate the accuracy and stability of the machine learning models. Instead of a single generalized model, multiple **specialized regression models** were trained to account for different pricing mechanics across item categories.

The following standard regression metrics were used:

- **MAE (Mean Absolute Error)** — average absolute prediction error  
- **RMSE (Root Mean Squared Error)** — penalizes large deviations and reflects model stability  
- **R² (Coefficient of Determination)** — measures explanatory power of the model  

These metrics provide an objective numerical assessment of model performance.


####  Overview of Model Performance

Six specialized models were evaluated, each trained on a specific subset of weapon or skin types:

| Model | MAE | RMSE | R² | Interpretation |
|-------|--------|----------|-----------|----------------|
| **doppler_knives** | 304.15 | 563.21 | **0.8909** | High accuracy; low error relative to price variance; very good fit. |
| **fade_weapon** | 606.06 | 2834.46 | **0.5696** | Moderate accuracy; high variance in fade weapons reduces model stability. |
| **ch_knives** | 335.90 | 816.42 | **0.9841** | Excellent performance; very strong model fit. |
| **case_hardened_gun** | 446.35 | 2645.68 | **0.6243** | Moderate performance; high variability of CH patterns increases error. |
| **fade_knives** | 468.73 | 2876.82 | **0.7680** | Good performance; complexity caused by pattern dependency. |
| **float_sensitive_weapons** | 192.94 | 739.91 | **0.6783** | Lowest MAE; effective for float-driven pricing. |

The results demonstrate **good to excellent predictive quality** across most categories. Knife-based models show the strongest performance, while fade-based weapons exhibit higher error due to intrinsic market volatility.


####  Quantitative Testing of API Performance

Quantitative testing was also applied to evaluate non-functional requirements, particularly performance:

- Metadata endpoints: **3–4 ms average response time**
- Prediction endpoint: **5–6 ms average response time**
- Explanation endpoints: **~2000 ms average response time**

The increased latency of explanation endpoints is expected due to additional reasoning and feature attribution logic.


###  Qualitative Testing

Quantitative metrics alone are insufficient to fully evaluate a user-facing system. Therefore, qualitative testing was conducted to assess correctness, usability, and consistency.

####  Qualitative Evaluation Criteria

Qualitative testing focused on the following aspects:

- Correctness and completeness of API responses  
- Stability under invalid or malformed input  
- Clarity and usefulness of error messages  
- Semantic relevance of explanation texts  
- Consistency of response formats across endpoints  

This evaluation was performed from the perspective of an end user interacting with the system.


###  Test Cases and Postman-Based Testing

####  User-Accessible Endpoints

Test cases were created for **all endpoints with which a user can interact**, including:

- `/api/predict`
- `/api/ai/explain`
- `/api/ai/explain-v2`
- `/api/meta/*` (weapons, skins, wear tiers, patterns, stickers)


####  Test Case Design

Each test case includes:

- Input data and preconditions  
- Expected output or system behavior  
- Error scenarios and boundary conditions  

The test cases cover:

- Positive scenarios (valid input)
- Negative scenarios (missing or invalid fields)
- Boundary values (extreme float values, empty sticker arrays)
- Schema and type validation
- Error handling consistency

Each test case is mapped to functional requirements, ensuring traceability.


####  Test Case Implementation Using Postman

All test cases were **implemented as automated Postman collections**. Postman was selected due to its support for REST API testing and automation.

Postman test scripts were used to:

- Validate HTTP status codes  
- Verify response schemas  
- Assert business rules (e.g., predicted price ≥ 0)  
- Measure response times  

Separate collections were created for Predict, Explain, and Meta endpoints.


####  Test Execution Results

- Total automated test cases: **84**
- Total endpoints covered: **100%**
- Passed tests: **84**
- Failed tests: **0**
- Overall pass rate: **100%**

These results confirm correct and stable behavior of all user-facing endpoints.


###  Integrated Testing Results

The combined application of qualitative and quantitative testing provides a comprehensive system evaluation:

- Quantitative testing confirms **accuracy, stability, and performance**
- Qualitative testing confirms **correctness, usability, and consistency**
- Automated testing ensures **reproducibility and reliability**

This integrated approach fully соответствует objectives of the discipline **“Qualitative or Quantitative Testing”**.

###  Conclusion Qualitative and Quantitative Testing

The conducted qualitative and quantitative testing demonstrates that the **cs2price_prediction** system meets all defined functional and non-functional requirements. The machine learning models achieve strong predictive performance, while the REST API provides reliable and user-consistent interaction.

All detailed test cases, Postman collections, quantitative metrics, and testing reports are available in the project repository, where the testing process is documented in greater detail.

