# Problem Statement & Goals

## Background and Market Context

The trading of virtual items in modern online games has evolved into a complex digital economy with real-world financial implications. Counter-Strike 2 (CS2) represents one of the most prominent examples of such an economy, where in-game cosmetic items, known as skins, are actively traded on both official and third-party marketplaces.

Skin prices are influenced by a combination of:
- intrinsic item properties (rarity, float value, pattern),
- cosmetic enhancements (stickers, StatTrak),
- market supply and demand,
- temporal trends and external events.

Due to this complexity, accurate price estimation requires advanced analytical approaches that go beyond basic averaging or historical comparison.

## Problem Description

Despite the high economic value of CS2 skins, existing tools suffer from several limitations:
- incomplete consideration of item-specific parameters,
- lack of explainability in price estimation,
- delayed or manual data updates,
- insufficient visualization of price dynamics.

These limitations lead to suboptimal decision-making and financial losses for users.

## Problem Statement

**Who:**  
CS2 players, traders, and digital trading platforms.

**What:**  
Users lack access to accurate, transparent, and explainable tools for determining the true market value of CS2 skins.

**Why:**  
Inaccurate valuation increases financial risk, reduces market efficiency, and undermines user trust in existing analytical solutions.

## Business Goals

| Goal | Description | KPI |
|----|------------|-----|
| Pricing Accuracy | Improve correctness of price estimates | ≥ 90% accuracy |
| Risk Reduction | Support informed trading decisions | ≥ 80% user satisfaction |
| Transparency | Explain pricing factors | SHAP visualizations |
| Automation | Reduce manual data handling | ≤ 24h data refresh |
| Reliability | Ensure stable system behavior | ≥ 99% uptime |

## Project Objectives

- Design a unified and user-friendly data input mechanism.
- Integrate ensemble machine learning models (XGBoost, Random Forest, LSTM).
- Provide interactive data visualizations for price trends.
- Implement automated market data synchronization.
- Ensure compliance with GDPR and security best practices.

## Success Criteria

- High prediction accuracy
- Low system response time
- High test coverage
- Positive user feedback
- Stable and automated data updates

## Non-Goals

The project explicitly does not aim to:
- facilitate direct trading transactions,
- provide financial or investment advice,
- replace existing marketplaces.
