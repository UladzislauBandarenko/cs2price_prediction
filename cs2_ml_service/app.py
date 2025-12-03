import os
from typing import Dict

import pandas as pd
import uvicorn
from catboost import CatBoostRegressor
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel

# ==== Пути ====
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
MODELS_DIR = os.path.join(BASE_DIR, "models")

print("BASE_DIR:", BASE_DIR)
print("MODELS_DIR:", MODELS_DIR)

app = FastAPI(title="CS2 Pricing ML Service")

models: Dict[str, CatBoostRegressor] = {}


# ========= 1. Pydantic-схемы ПОД ГОТОВЫЕ ФИЧИ (то, что шлёт C#) =========

# 1) CASE HARDENED KNIVES
# feature_cols (НОВАЯ СХЕМА, БЕЗ market_hash_name):
# ["float","pattern","stattrak",
#  "backside_blue","backside_purple","backside_gold",
#  "playside_blue","playside_purple","playside_gold",
#  "weapon","skin","wear"]
class CaseHardenedRequest(BaseModel):
    float: float
    pattern: int
    stattrak: int

    backside_blue: float
    backside_purple: float
    backside_gold: float
    playside_blue: float
    playside_purple: float
    playside_gold: float

    weapon: str
    skin: str
    wear: str


# 2) CH_GUNS (без изменений)
# num_features:
# ["float","pattern","stattrak",
#  "backside_blue","backside_purple","backside_gold",
#  "playside_blue","playside_purple","playside_gold",
#  "stickers_count","stickers_total_value","stickers_avg_value","stickers_max_value",
#  "slot0_price","slot1_price","slot2_price","slot3_price",
#  "blue_score","gold_score","blue_tier","gold_tier"]
# cat_features:
# ["weapon","skin","wear","pattern_style"]
class ChGunsRequest(BaseModel):
    # категориальные
    weapon: str
    skin: str
    wear: str
    pattern_style: str

    # базовые числовые (base_num_features)
    float: float
    pattern: int
    stattrak: int

    backside_blue: float
    playside_blue: float

    stickers_count: int
    stickers_total_value: float
    stickers_avg_value: float
    stickers_max_value: float

    slot0_price: float
    slot1_price: float
    slot2_price: float
    slot3_price: float

    # engineered_num_features
    blue_score: float
    blue_tier: int


# 3) DOPPLER KNIVES (уже без pattern, как ты сделал)
# feature_cols:
# ["float","stattrak","weapon","skin","wear","phase"]
class DopplerRequest(BaseModel):
    weapon: str
    skin: str
    wear: str
    phase: str

    float: float
    stattrak: int


# 4) FADE GUNS
# НОВАЯ СХЕМА (БЕЗ market_hash_name):
# all_features:
# ["float","pattern","stattrak",
#  "fade_percentage","fade_rank",
#  "stickers_count","stickers_total_value","stickers_avg_value","stickers_max_value",
#  "slot0_price","slot1_price","slot2_price","slot3_price",
#  "weapon","skin","wear"]
class FadeGunsRequest(BaseModel):
    float: float
    pattern: int
    stattrak: int

    fade_percentage: float
    fade_rank: float

    stickers_count: int
    stickers_total_value: float
    stickers_avg_value: float
    stickers_max_value: float

    slot0_price: float
    slot1_price: float
    slot2_price: float
    slot3_price: float

    weapon: str
    skin: str
    wear: str


# 5) FADE KNIVES
# НОВАЯ СХЕМА (БЕЗ market_hash_name):
# feature_cols:
# ["float","pattern","stattrak",
#  "fade_percentage","fade_rank",
#  "weapon","skin","wear"]
class FadeKnivesRequest(BaseModel):
    float: float
    pattern: int
    stattrak: int

    fade_percentage: float
    fade_rank: float

    weapon: str
    skin: str
    wear: str


# 6) FLOAT-SENSITIVE GUNS
# НОВАЯ СХЕМА (БЕЗ market_hash_name):
# all_features:
# ["float","stattrak",
#  "stickers_count","stickers_total_value","stickers_avg_value","stickers_max_value",
#  "slot0_price","slot1_price","slot2_price","slot3_price",
#  "weapon","skin","wear"]
class FloatSensitiveGunsRequest(BaseModel):
    float: float
    stattrak: int

    stickers_count: int
    stickers_total_value: float
    stickers_avg_value: float
    stickers_max_value: float

    slot0_price: float
    slot1_price: float
    slot2_price: float
    slot3_price: float

    weapon: str
    skin: str
    wear: str


# ========= 2. Загрузка CatBoost-моделей =========

def load_catboost_model(filename: str) -> CatBoostRegressor:
    path = os.path.join(MODELS_DIR, filename)
    print("Trying to load model:", path)
    if not os.path.exists(path):
        raise RuntimeError(f"Model file not found: {path}")
    m = CatBoostRegressor()
    m.load_model(path)
    return m


@app.on_event("startup")
def load_all_models():
    global models
    models["case_hardened"] = load_catboost_model("case_hardened_catboost_price_model.cbm")
    models["ch_guns"] = load_catboost_model("ch_guns_model.cbm")
    models["doppler"] = load_catboost_model("doppler_knives_price_model.cbm")
    models["fade_guns"] = load_catboost_model("fade_guns_model.cbm")
    models["fade_knives"] = load_catboost_model("fade_knives_model.cbm")
    models["float_sensitive_guns"] = load_catboost_model("float_sensitive_guns_model.cbm")

    print("[startup] Models loaded:", list(models.keys()))


# ========= 3. Endpoints =========

@app.post("/predict/case-hardened")
def predict_case_hardened(payload: CaseHardenedRequest):
    model = models.get("case_hardened")
    if model is None:
        raise HTTPException(status_code=500, detail="case_hardened model not loaded")

    feature_cols = [
        "float",
        "pattern",
        "stattrak",
        "backside_blue",
        "backside_purple",
        "backside_gold",
        "playside_blue",
        "playside_purple",
        "playside_gold",
        "weapon",
        "skin",
        "wear",
    ]

    row = payload.dict()
    df = pd.DataFrame([[row[c] for c in feature_cols]], columns=feature_cols)

    try:
        price = float(model.predict(df)[0])
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Prediction error: {e}")

    return {"predicted_price": price}


@app.post("/predict/ch-guns")
def predict_ch_guns(payload: ChGunsRequest):
    model = models.get("ch_guns")
    if model is None:
        raise HTTPException(status_code=500, detail="ch_guns model not loaded")

    base_num_features = [
        "float",
        "pattern",
        "stattrak",
        "backside_blue",
        "playside_blue",
        "stickers_count",
        "stickers_total_value",
        "stickers_avg_value",
        "stickers_max_value",
        "slot0_price",
        "slot1_price",
        "slot2_price",
        "slot3_price",
    ]

    engineered_num_features = [
        "blue_score",
        "blue_tier",
    ]

    cat_features = [
        "weapon",
        "skin",
        "wear",
        "pattern_style",
    ]

    feature_cols = base_num_features + engineered_num_features + cat_features

    row = payload.dict()
    df = pd.DataFrame([[row[c] for c in feature_cols]], columns=feature_cols)

    try:
        price = float(model.predict(df)[0])
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Prediction error: {e}")

    stickers_features = {
        "stickers_count": payload.stickers_count,
        "stickers_total_value": payload.stickers_total_value,
        "stickers_avg_value": payload.stickers_avg_value,
        "stickers_max_value": payload.stickers_max_value,
    }

    return {
        "predicted_price": price,
        "stickers_features": stickers_features,
    }


@app.post("/predict/doppler")
def predict_doppler(payload: DopplerRequest):
    model = models.get("doppler")
    if model is None:
        raise HTTPException(status_code=500, detail="doppler model not loaded")

    feature_cols = [
        "float",
        "stattrak",
        "weapon",
        "skin",
        "wear",
        "phase",
    ]

    row = payload.dict()
    df = pd.DataFrame([[row[c] for c in feature_cols]], columns=feature_cols)

    try:
        price = float(model.predict(df)[0])
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Prediction error: {e}")

    return {"predicted_price": price}


@app.post("/predict/fade-guns")
def predict_fade_guns(payload: FadeGunsRequest):
    model = models.get("fade_guns")
    if model is None:
        raise HTTPException(status_code=500, detail="fade_guns model not loaded")

    feature_cols = [
        "float",
        "pattern",
        "stattrak",
        "fade_percentage",
        "fade_rank",
        "stickers_count",
        "stickers_total_value",
        "stickers_avg_value",
        "stickers_max_value",
        "slot0_price",
        "slot1_price",
        "slot2_price",
        "slot3_price",
        "weapon",
        "skin",
        "wear",
    ]

    row = payload.dict()
    df = pd.DataFrame([[row[c] for c in feature_cols]], columns=feature_cols)

    try:
        price = float(model.predict(df)[0])
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Prediction error: {e}")

    stickers_features = {
        "stickers_count": payload.stickers_count,
        "stickers_total_value": payload.stickers_total_value,
        "stickers_avg_value": payload.stickers_avg_value,
        "stickers_max_value": payload.stickers_max_value,
    }

    return {"predicted_price": price, "stickers_features": stickers_features}


@app.post("/predict/fade-knives")
def predict_fade_knives(payload: FadeKnivesRequest):
    model = models.get("fade_knives")
    if model is None:
        raise HTTPException(status_code=500, detail="fade_knives model not loaded")

    feature_cols = [
        "float",
        "pattern",
        "stattrak",
        "fade_percentage",
        "fade_rank",
        "weapon",
        "skin",
        "wear",
    ]

    row = payload.dict()
    df = pd.DataFrame([[row[c] for c in feature_cols]], columns=feature_cols)

    try:
        price = float(model.predict(df)[0])
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Prediction error: {e}")

    return {"predicted_price": price}


@app.post("/predict/float-sensitive-guns")
def predict_float_sensitive_guns(payload: FloatSensitiveGunsRequest):
    model = models.get("float_sensitive_guns")
    if model is None:
        raise HTTPException(status_code=500, detail="float_sensitive_guns model not loaded")

    feature_cols = [
        "float",
        "stattrak",
        "stickers_count",
        "stickers_total_value",
        "stickers_avg_value",
        "stickers_max_value",
        "slot0_price",
        "slot1_price",
        "slot2_price",
        "slot3_price",
        "weapon",
        "skin",
        "wear",
    ]

    row = payload.dict()
    df = pd.DataFrame([[row[c] for c in feature_cols]], columns=feature_cols)

    try:
        price = float(model.predict(df)[0])
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Prediction error: {e}")

    stickers_features = {
        "stickers_count": payload.stickers_count,
        "stickers_total_value": payload.stickers_total_value,
        "stickers_avg_value": payload.stickers_avg_value,
        "stickers_max_value": payload.stickers_max_value,
    }

    return {"predicted_price": price, "stickers_features": stickers_features}


if __name__ == "__main__":
    uvicorn.run("app:app", host="0.0.0.0", port=8000, reload=True)
