# AI Explanation Service – Technical Documentation

AI Skin Explanation Service

1. Introduction

This document provides a complete technical and architectural overview of the AI Skin Explanation Service, a component of the CS2 price-prediction platform.
The service integrates with an internal ML system, retrieves metadata from the database, constructs structured prompts, and generates clear natural-language explanations for skin prices using OpenAI models.


## This document explains

architectural decisions

prompt engineering strategy

fallback logic for multi-model LLM routing

sanitization and safety rules

API behavior

data-processing pipeline

The goal is to provide an engineering-level understanding of the service and justify its design.

2. System Purpose and Responsibilities

The service converts raw prediction data (float, wear, pattern, rarity, stickers) into a human-readable explanation, based entirely on provided numerical values.


## Key responsibilities

Integrate CS2 prediction model output with LLMs

Retrieve metadata (weapon, skin, tier, pattern info)

Construct structured prompts for different item categories

Enforce strict rules preventing hallucinations (e.g., no sticker names invention)

Support multi-model fallback logic

Format results into a clean explanation for frontend consumption

Users never interact with prompts directly — they simply receive an explanation of a predicted price.

3. High-Level Architecture


## The service is part of an ASP.NET Core backend and interacts with

Internal ML Service

Provides predicted numeric price

Does not produce explanations

AppDbContext

Skins

Wear tiers

Sticker metadata

Pattern-related tables (CH, Fade, Doppler, etc.)

AI Components

AiPromptFactory — builds prompts for each category

AiStickerService — resolves sticker prices & names

AiExplanationService — high-level orchestration

LlmFailoverService — manages model selection & failover

OpenAiClient — low-level HTTP client for OpenAI Responses API

Controllers

/api/ai/explain (primary → gpt-4o-mini)

/api/ai/explain-v2 (primary → gpt-4.1-mini)

The service functions purely as a computation and orchestration layer, never storing user data.

4. Data Flow

4.1 Input Sources


## Requests to the service contain

PredictedPrice — ML model output

SkinId, WearTierId, Pattern, FloatValue

Stickers[] — list of sticker IDs (ignored for knives)

IsStattrak

LlmPriority — determines fallback direction

All metadata is validated through the database.

4.2 Processing Steps

Validate request

Skin and wear must exist

Wear must be valid for the skin

Pattern must exist for this skin type

Determine item category

ch_knife, ch_gun, fade_gun, fade_knife, doppler_knife, float_gun

Sticker resolution

Guns → full sticker processing

Knives → stickers are forcibly ignored

Construct prompt

AiPromptFactory builds scenario-specific prompt

Includes strict behavioral instructions

Pass prompt to LLM with fallback

OpenAiClient generates plain-text explanation

LlmFailoverService handles failures

Return formatted explanation
Output is always a single clean text block without markdown, emojis, or hallucinated names.

5. Prompt Design and Structure

5.1 System Prompt Responsibilities


## The system prompt is embedded in OpenAiClient and enforces

English-only output

No markdown, no lists, no bullets

No hallucinations of sticker names

No invented historical data

Explanation must be based only on numeric values provided

Only plain sentences

Forget stickers entirely if item is a knife

It defines global AI behavior and ensures consistent output regardless of user input.

5.2 Item-Specific Prompts


## AiPromptFactory generates 6 scenarios

Case Hardened Knife

Uses color distribution (blue/gold/purple), float, wear.

 Case Hardened Gun

Adds sticker block and blue score tiering.

Doppler Knife

Uses phase name and float.

Fade Gun

Includes fade percentage and ranking.

Fade Knife

Fade % and fade rank; no stickers.

Float-Sensitive Guns

Only float + wear + stickers.

Each prompt ends with a footer describing how to interpret the factors.

6. Sanitization Layer


## The service prevents unsafe or malformed content from reaching the model

Stickers on knives are replaced with 0 values

Pattern mismatch triggers validation errors

Wear tiers are validated against allowed values

No user-controlled strings are injected directly into prompts
(all metadata comes from controlled DB tables)

Therefore, input sanitization is predefined by controlled sources.

7. Model Selection Strategy

7.1 Available Models

Primary: gpt-4o-mini (fast, cheap)

Secondary: gpt-4.1-mini (more consistent output)

7.2 Priority Types


## LlmPriority enum contains

MiniThenGpt41 — used by standard /explain endpoint

Gpt41ThenMini — used by /explain-v2 endpoint

7.3 Multi-Model Failover Logic


## LlmFailoverService implements

Try primary model

On exception → try fallback model

If both fail → propagate error to controller

This guarantees that the AI explanation always works even if one model is temporarily unavailable.

8. Error Handling and Logging


## The service logs

OpenAI request failures

Fallback model activation

Pattern/wear validation failures

Unexpected empty outputs from LLM

Sensitive content such as prompts or user-provided values is never logged.

9. Security Considerations


## To ensure safety

API keys stored only in environment variables

Prompts prohibit hallucination

Prompts prohibit mentioning stickers for knives

Only controlled DB data is injected into prompts

No user prompt influence is allowed

No execution of user instructions

Validation layer prevents invalid patterns/wear tiers

The service behaves predictably and resists injection attempts by design.

10. Justification of Design Choices

Strict prompt engineering

Prevents hallucinations & inconsistencies.

Multi-model fallback

Ensures reliability under outages.

Modular architecture

Easily extendable for new skin types.

Category-specific prompts


## Important because

CH logic differs greatly from Fade logic

Doppler phases require structured handling

Knives vs guns differ in sticker logic

Pure orchestration

The service does not store user data or predictions — only processes and explains.

11. Conclusion

The CS2 AI Explanation Microservice is a robust, modular, and reliable system that converts numeric ML predictions into natural-language explanations using OpenAI models.


## It ensures

strong safety through controlled metadata

no hallucination through strict prompts

fallback reliability across models

extensibility for new item categories

clean architecture with clearly separated responsibilities

This implementation adheres to modern standards of LLM integration in production-grade systems.
