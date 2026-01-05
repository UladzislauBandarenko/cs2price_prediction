# API Documentation Rules and Strategy

## 1. Purpose of This Document

This document describes the API documentation strategy used in the diploma project.
It defines the tools, standards, conventions, and rules applied when documenting all APIs in the system.

The goal of this document is to ensure that the API documentation is:
- clear and consistent;
- technically accurate;
- easy to maintain and extend;
- compliant with diploma project evaluation requirements.

---

## 2. Documentation Scope

The documentation covers the following API groups:
- Meta API (reference metadata);
- Prediction API (price prediction workflow);
- AI Explanation API (model explainability);
- Admin API (internal reference data management);
- ML Service API (internal ML inference service).

Both public and internal APIs are documented. Internal APIs are explicitly marked as such.

---

## 3. Tools Used

The following tools and technologies are used for API documentation:

- **Markdown** — main format for human-readable documentation;
- **OpenAPI 3.x** — formal machine-readable API specification;
- **Swagger UI / Swagger Editor** — schema validation and interactive visualization;
- **Spectral / Redocly CLI** — OpenAPI schema linting and validation;
- **Git** — version control for all documentation artifacts.

---

## 4. Standards Followed

The documentation follows these standards and best practices:

- RESTful API design principles;
- OpenAPI Specification 3.x;
- HTTP/1.1 semantics and standard HTTP status codes;
- JSON as the primary data exchange format;
- Clear separation between public, internal, and administrative APIs.

---

## 5. Naming Conventions

The following naming conventions are applied consistently:

- URL paths: **kebab-case**  
  Example: `/api/v1/meta/weapon-types`

- JSON fields: **camelCase**  
  Example: `predictedPrice`, `wearTierId`

- Error codes: **UPPER_SNAKE_CASE**  
  Example: `SKIN_NOT_FOUND`, `VALIDATION_ERROR`

- Resource names: plural nouns  
  Example: `/skins`, `/weapons`, `/stickers`

---

## 6. Versioning Strategy

The API uses URL-based versioning:

```
/api/v1/...
```

Rules:
- Breaking changes require a new major version;
- Older versions remain documented until deprecated;
- Version changes are reflected in both Markdown documentation and OpenAPI schemas.

---

## 7. Formatting Rules

All documentation follows these formatting rules:

- Markdown is used for all textual documentation;
- Each API has a dedicated reference file;
- Sections are organized using consistent headings;
- Code blocks are used for all request and response examples;
- Tables are used for parameters and field descriptions.

Recommended directory structure:

```
/docs
  API_DOCUMENTATION_RULES.md
  META_API_DOCUMENTATION.md
  PREDICTION_API_DOCUMENTATION.md
  AI_EXPLANATION_API_DOCUMENTATION.md
  ADMIN_API_DOCUMENTATION.md
  ML_SERVICE_API_DOCUMENTATION.md
```

---

## 8. Error Handling Documentation Rules

All APIs document error handling explicitly.

Rules:
- Error responses use a unified structure where applicable;
- HTTP status codes are documented for each endpoint;
- Validation errors, authorization errors, and not-found errors are described separately.

---

## 9. Authentication and Authorization Documentation

Authentication requirements are documented per API group:

- Public APIs explicitly state that no authentication is required;
- Admin API documents API key authorization;
- Internal APIs are marked as internal and not exposed to end users.

---

## 10. OpenAPI Specification Rules

The OpenAPI specification:
- serves as the single source of truth for API contracts;
- is stored as a version-controlled YAML file;
- must pass validation using OpenAPI tooling;
- reflects the same structure and behavior as the Markdown documentation.

---

## 11. Known Gaps and Limitations

The following documentation limitations are explicitly acknowledged:

- Internal ML Service APIs do not document business validation logic;
- Some validation rules are enforced upstream and therefore not repeated in internal APIs;
- ML prediction values are returned as strings due to model output constraints;
- Some endpoints are intended for internal use only and are not publicly accessible.

These limitations are intentional design decisions and are documented for transparency.

---

## 12. Conclusion

This documentation strategy ensures a clear separation between API behavior, validation responsibilities,
and architectural concerns. It provides both human-readable documentation and machine-readable specifications,
meeting industry standards and diploma project evaluation requirements.
