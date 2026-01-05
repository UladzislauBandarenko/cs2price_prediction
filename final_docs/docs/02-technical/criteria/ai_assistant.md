## AI Assistant Technical Documentation

### Introduction

This document describes the architecture and functionality of the **AI Skin Explanation Service**, a core component of the CS2 skin price prediction platform.

The service integrates machine learning price predictions with structured market metadata and generates concise, human-readable explanations using OpenAI language models. The goal of this documentation is to outline the system design and justify key architectural decisions.


### Scope

This document covers:
- system architecture,
- prompt engineering approach,
- LLM model routing and fallback logic,
- data validation and safety mechanisms,
- API behavior and end-to-end data flow.


### System Purpose

The AI Skin Explanation Service converts numeric prediction outputs (price, float, wear, pattern, rarity, stickers) into clear natural-language explanations. All generated explanations are based exclusively on validated numerical and categorical inputs.

Key responsibilities include:
- integration with ML prediction services,
- retrieval and validation of skin metadata from the database,
- construction of structured, category-specific prompts,
- enforcement of anti-hallucination rules,
- orchestration of LLM calls with fallback support.

Users interact only with final explanations; prompt logic is fully internal.


### Architecture Overview

The service operates within an **ASP.NET Core backend** and interacts with:
- an internal ML service (price prediction only),
- a relational database (skins, wear tiers, patterns, stickers),
- AI components for prompt construction, explanation generation, and model failover,
- REST API controllers exposing explanation endpoints.

The service acts as a stateless orchestration layer and does not store user data.


### Data Flow Summary

**Input:**  
Predicted price, skin identifiers, wear tier, float value, pattern data, optional stickers, and LLM priority.

**Processing steps:**
1. Metadata validation against the database
2. Item category detection (e.g. Case Hardened, Fade, Doppler)
3. Sticker resolution (ignored for knives)
4. Structured prompt construction
5. LLM execution with automatic fallback
6. Plain-text explanation formatting


### Prompt Strategy

A global system prompt enforces strict behavior:
- English-only output
- no markdown or lists
- no hallucinated facts
- reliance solely on provided data
- stickers forbidden for knives

Category-specific prompts reflect different pricing logic for Case Hardened, Fade, Doppler, and float-sensitive items.


### Safety and Validation

The service enforces strong safeguards:
- all prompt data originates from controlled database sources,
- invalid patterns or wear tiers are rejected,
- stickers are excluded from knife explanations,
- user-provided strings are never injected into prompts.


### Model Selection and Reliability

Supported models:
- **Primary:** `gpt-4o-mini`
- **Fallback:** `gpt-4.1-mini`

Automatic failover ensures high availability in case of model or network failures.


### Error Handling and Security

The system logs only technical errors and never stores prompts or user data. API keys are managed via environment variables.


### Conclusion

The AI Skin Explanation Service is a modular and reliable component that transforms numeric ML predictions into clear and explainable natural-language outputs.

The implementation provides:
- strong safety guarantees,
- zero hallucination via controlled prompts,
- high reliability through model fallback,
- a clean and extensible architecture.

The service follows modern best practices for production-grade LLM integration.
