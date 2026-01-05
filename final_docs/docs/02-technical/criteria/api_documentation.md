## API Technical Documentation


###  API Architecture Overview

The **cs2price_prediction** system consists of several logically separated APIs.
Each API is responsible for a specific functional area and follows a clear separation
between user-facing and internal system components.

#### API Groups

| API | Purpose | Accessibility |
|----|--------|---------------|
| **Meta API** | Reference and metadata access | Public |
| **Prediction API** | Price prediction workflow | Public |
| **AI Explanation API** | Prediction interpretability | Public |
| **Admin API** | Administrative management | Internal |
| **ML API** | Machine learning inference | Internal |

This document provides detailed documentation for all public APIs.
Internal APIs are described briefly with references to the project repository.


### Meta API Documentation

#### Overview

The Meta API provides read-only reference data related to CS2 weapons, skins, cosmetic attributes, and stickers.
It is designed to be used by pricing services, analytics modules, and machine learning pipelines.

All responses are returned in **JSON** format.

**Base URL:** `/api/v1/meta`  
**Authentication:** Not required


#### Architecture Context

The Meta API acts as a centralized reference data provider and does not store user-specific
or transactional data. It supplies normalized metadata used across the system.


#### Error Handling

```json
{
  "error": {
    "code": "ERROR_CODE",
    "message": "Human-readable error description"
  }
}
```

| HTTP Code | Meaning |
|----------|--------|
| 400 | Invalid request |
| 404 | Resource not found |
| 500 | Internal server error |



#### Endpoints

- **GET /weapon-types** – returns all weapon types  
- **GET /weapon-types/{weaponTypeId}/weapons** – returns weapons by type  
- **GET /weapons/{weaponId}/skins** – returns skins for a weapon  
- **GET /skins/{skinId}/wear-tiers** – returns wear tiers  
- **GET /skins/{skinId}/patterns** – returns pattern IDs  
- **GET /stickers?q=&limit=** – searches stickers  



### Prediction API Documentation

#### Overview

The Prediction API is the main entry point for generating a unified CS2 skin price prediction.
It aggregates metadata, validates input, selects the appropriate ML model, invokes the ML API,
and returns a structured prediction result.

All responses are returned in **JSON** format.

**Base URL:** `/api/v1`  
**Authentication:** Not required


#### POST /predict

Performs a complete price prediction workflow.

##### Responsibilities

- Input validation  
- Metadata resolution  
- ML model selection  
- ML API invocation  
- Response aggregation  


### AI Explanation API Documentation

#### Overview

The AI Explanation API provides natural-language explanations describing how different attributes
influence the predicted price.

It improves transparency and interpretability of machine learning predictions.

**Base URL:** `/api/v1/ai`  
**Authentication:** Not required


#### Endpoints

- **POST /explain** – concise explanation  
- **POST /explain-v2** – extended explanation  


### Admin API (Summary)

#### Purpose

The Admin API provides administrative and maintenance functionality such as configuration
management and system monitoring.

This API is internal and requires authentication.

**Full documentation is available in the project repository.**


### ML API (Summary)

#### Purpose

The ML API is an internal machine learning microservice responsible for executing
model inference for trained models.

It is consumed exclusively by the Prediction API.

**Full documentation is available in the project repository.**


