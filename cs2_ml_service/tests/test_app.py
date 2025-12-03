import sys
from pathlib import Path

# Ensure project root is on sys.path so "import app" works
PROJECT_ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(PROJECT_ROOT))

import pytest
from fastapi.testclient import TestClient

import app as app_module


@pytest.fixture(autouse=True)
def mock_models_and_loader(monkeypatch):
    """
    - Patch load_catboost_model to return DummyModel (no real .cbm files needed).
    - Clear models dict before each test.
    - Save original load_catboost_model as RealLoadCatboostModel
      so we can test the "file not found" branch.
    """
    # Save real function once
    if not hasattr(app_module, "RealLoadCatboostModel"):
        app_module.RealLoadCatboostModel = app_module.load_catboost_model

    class DummyModel:
        def __init__(self, value: float = 100.0, fail: bool = False):
            self.value = value
            self.fail = fail

        def predict(self, df):
            if self.fail:
                raise RuntimeError("boom")
            return [self.value]

    def fake_loader(filename: str):
        return DummyModel()

    monkeypatch.setattr(app_module, "load_catboost_model", fake_loader)
    app_module.DummyModel = DummyModel
    app_module.models.clear()

    yield


@pytest.fixture
def client():
    """
    TestClient fixture that ensures startup events are executed
    (so models dict is filled using the patched loader).
    """
    with TestClient(app_module.app) as c:
        yield c


# ===== helper payload builders =====

def _base_case_hardened_payload():
    return {
        "float": 0.05,
        "pattern": 123,
        "stattrak": 1,
        "backside_blue": 0.3,
        "backside_purple": 0.2,
        "backside_gold": 0.1,
        "playside_blue": 0.4,
        "playside_purple": 0.3,
        "playside_gold": 0.2,
        "weapon": "AK-47",
        "skin": "Case Hardened",
        "wear": "Factory New",
    }


def _base_ch_guns_payload():
    return {
        "weapon": "AK-47",
        "skin": "Case Hardened",
        "wear": "Factory New",
        "pattern_style": "blue",
        "float": 0.05,
        "pattern": 123,
        "stattrak": 1,
        "backside_blue": 0.3,
        "playside_blue": 0.4,
        "stickers_count": 4,
        "stickers_total_value": 100.0,
        "stickers_avg_value": 25.0,
        "stickers_max_value": 40.0,
        "slot0_price": 10.0,
        "slot1_price": 20.0,
        "slot2_price": 30.0,
        "slot3_price": 40.0,
        "blue_score": 0.8,
        "blue_tier": 3,
    }


def _base_doppler_payload():
    return {
        "weapon": "Karambit",
        "skin": "Doppler",
        "wear": "Factory New",
        "phase": "Phase 2",
        "float": 0.01,
        "stattrak": 0,
    }


def _base_fade_guns_payload():
    return {
        "float": 0.02,
        "pattern": 42,
        "stattrak": 0,
        "fade_percentage": 0.95,
        "fade_rank": 1,
        "stickers_count": 2,
        "stickers_total_value": 50.0,
        "stickers_avg_value": 25.0,
        "stickers_max_value": 30.0,
        "slot0_price": 5.0,
        "slot1_price": 10.0,
        "slot2_price": 15.0,
        "slot3_price": 20.0,
        "weapon": "Glock-18",
        "skin": "Fade",
        "wear": "Factory New",
    }


def _base_fade_knives_payload():
    return {
        "float": 0.01,
        "pattern": 7,
        "stattrak": 1,
        "fade_percentage": 0.98,
        "fade_rank": 1,
        "weapon": "Bayonet",
        "skin": "Fade",
        "wear": "Factory New",
    }


def _base_float_sensitive_payload():
    return {
        "float": 0.06,
        "stattrak": 0,
        "stickers_count": 3,
        "stickers_total_value": 30.0,
        "stickers_avg_value": 10.0,
        "stickers_max_value": 15.0,
        "slot0_price": 1.0,
        "slot1_price": 2.0,
        "slot2_price": 3.0,
        "slot3_price": 4.0,
        "weapon": "AWP",
        "skin": "Wildfire",
        "wear": "Minimal Wear",
    }


# ===== success tests =====

def test_predict_case_hardened_success(client):
    payload = _base_case_hardened_payload()
    resp = client.post("/predict/case-hardened", json=payload)
    assert resp.status_code == 200
    data = resp.json()
    assert "predicted_price" in data
    assert isinstance(data["predicted_price"], float)


def test_predict_ch_guns_success(client):
    payload = _base_ch_guns_payload()
    resp = client.post("/predict/ch-guns", json=payload)
    assert resp.status_code == 200
    data = resp.json()
    assert "predicted_price" in data
    assert "stickers_features" in data
    stickers = data["stickers_features"]
    assert stickers["stickers_count"] == payload["stickers_count"]
    assert stickers["stickers_total_value"] == payload["stickers_total_value"]


def test_predict_doppler_success(client):
    payload = _base_doppler_payload()
    resp = client.post("/predict/doppler", json=payload)
    assert resp.status_code == 200
    data = resp.json()
    assert "predicted_price" in data
    assert isinstance(data["predicted_price"], float)


def test_predict_fade_guns_success(client):
    payload = _base_fade_guns_payload()
    resp = client.post("/predict/fade-guns", json=payload)
    assert resp.status_code == 200
    data = resp.json()
    assert "predicted_price" in data
    assert "stickers_features" in data
    assert data["stickers_features"]["stickers_count"] == payload["stickers_count"]


def test_predict_fade_knives_success(client):
    payload = _base_fade_knives_payload()
    resp = client.post("/predict/fade-knives", json=payload)
    assert resp.status_code == 200
    data = resp.json()
    assert "predicted_price" in data


def test_predict_float_sensitive_guns_success(client):
    payload = _base_float_sensitive_payload()
    resp = client.post("/predict/float-sensitive-guns", json=payload)
    assert resp.status_code == 200
    data = resp.json()
    assert "predicted_price" in data
    assert "stickers_features" in data
    assert data["stickers_features"]["stickers_count"] == payload["stickers_count"]


# ===== error tests =====

def test_doppler_model_not_loaded_returns_500(client):
    # Break model on purpose
    app_module.models["doppler"] = None

    resp = client.post("/predict/doppler", json=_base_doppler_payload())
    assert resp.status_code == 500
    data = resp.json()
    assert data["detail"] == "doppler model not loaded"


def test_float_sensitive_prediction_error_returns_500(client):
    # Replace model by one that raises in predict
    app_module.models["float_sensitive_guns"] = app_module.DummyModel(fail=True)

    resp = client.post("/predict/float-sensitive-guns", json=_base_float_sensitive_payload())
    assert resp.status_code == 500
    data = resp.json()
    assert "Prediction error" in data["detail"]


# ===== load_catboost_model "file not found" branch =====

def test_load_catboost_model_raises_if_file_missing(tmp_path, monkeypatch):
    """
    Check that the real load_catboost_model raises RuntimeError
    when model file does not exist.
    """
    monkeypatch.setattr(app_module, "MODELS_DIR", tmp_path)

    missing_file = "nonexistent_model.cbm"

    with pytest.raises(RuntimeError) as exc_info:
        app_module.RealLoadCatboostModel(missing_file)

    assert "Model file not found" in str(exc_info.value)
