# Project Overview

This section provides a comprehensive business-oriented overview of the diploma project dedicated to the analysis, monitoring, visualization, and prediction of Counter-Strike 2 (CS2) skin prices. The document defines the business context, motivation, system boundaries, stakeholders, and key functional and non-functional requirements.

The project is positioned at the intersection of digital economies, data analytics, and machine learning. It addresses the growing demand for transparent and explainable tools in virtual item trading environments, where financial decisions are increasingly driven by data-driven insights.

The proposed solution is a modular web-based analytical platform that integrates a frontend user interface, a backend API, a machine learning service, a relational database, and external trading platform APIs.

## Purpose of the Project

The primary purpose of this diploma project is to design and implement an analytical system that enables users to evaluate CS2 skin prices more accurately by considering both historical market trends and unique item-specific characteristics. Unlike traditional pricing tools, the system emphasizes explainability, automation, and adaptability to market dynamics.

The project also serves an academic purpose by demonstrating the practical application of:
- business analysis methodologies,
- machine learning techniques,
- web system architecture design,
- and data-driven decision support systems.

## Contents

- [Problem Statement & Goals](problem-and-goals.md)
- [Stakeholders & Users](stakeholders.md)
- [Scope](scope.md)
- [Features](features.md)


## Scope of This Document

This Project Overview focuses on the **Business Analysis (BA) perspective** and does not describe low-level implementation details. Technical architecture and implementation aspects are addressed only at a conceptual level to support business requirements.

Detailed system design, data models, and implementation strategies are covered in subsequent diploma chapters.

## Contents

- Problem context and motivation
- Business goals and success criteria
- Stakeholder identification and analysis
- System scope and constraints
- High-level feature definition and requirements

## Executive Summary

The Counter-Strike 2 skin market represents a mature virtual economy with real monetary value, high trading volumes, and strong price volatility. Prices are influenced by numerous factors, including rarity, wear level (float), pattern index, applied stickers, StatTrak status, and overall market sentiment.

Existing tools typically provide aggregated or simplified price estimates, failing to capture the multidimensional nature of item valuation. Moreover, most platforms do not explain why a specific price is suggested, which reduces user trust and increases financial risk.

This diploma project proposes an interactive web-based system that allows users to input detailed skin parameters, receive machine learning–based price predictions, and explore historical price trends and factor influence through visual analytics. Market data is automatically synchronized with external trading platforms such as Steam Market, Buff, and Skinport, ensuring data relevance and reliability.

## Key Highlights

| Aspect | Description |
|------|-------------|
| Problem Domain | CS2 virtual item market analytics |
| Core Problem | Inaccurate and non-transparent skin valuation |
| Proposed Solution | ML-based analytical web platform |
| Target Users | CS2 players, traders, trading platforms |
| Key Capabilities | Prediction, visualization, automation |
| Architecture | Frontend – Backend API – ML Service – Database |
| Academic Focus | Business analysis, ML, data visualization |
