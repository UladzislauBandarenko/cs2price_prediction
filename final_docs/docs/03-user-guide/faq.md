## FAQ & Troubleshooting Documentation

### Frequently Asked Questions

#### General

**Q: Where do the prices come from?**

A: All predictions are based on real completed market sales.
Listings and asking prices are intentionally not used.


**Q: Why does the predicted price differ from current listings?**

A: Listings often reflect seller expectations, not actual market value.
The system predicts realistic sale prices.


**Q: Is this an official Valve or Steam service?**

A: No. This is an independent academic and technical project
based on publicly observable market behavior.


#### Accuracy & Models

**Q: Why does the same skin sometimes give different prices?**

A: Small changes in float, wear, or attributes can significantly affect value,
especially for rare or float-sensitive items.


**Q: Why are there multiple ML models?**

A: Different skin categories have fundamentally different pricing logic.
Specialized models improve accuracy and stability.


#### Troubleshooting

| Problem | Possible Cause | Solution |
|-------|---------------|----------|
| Prediction seems too low | Missing attributes | Add float, stickers, or pattern info |
| Prediction seems too high | Rare pattern detected | Verify entered pattern or attributes |
| Page not responding | Network issue | Refresh page and retry |
| Unexpected value | Incorrect wear/float | Double-check inputs |


#### Error Messages

| Message | Meaning | Resolution |
|------|--------|------------|
| `Invalid input` | Missing or malformed data | Review input fields |
| `Prediction unavailable` | Model routing issue | Retry request |
| `Timeout` | Network latency | Refresh and try again |
