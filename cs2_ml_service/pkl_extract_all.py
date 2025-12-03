import pickle
import json
from pathlib import Path

# ===========================
# 1. СТАБ-КЛАССЫ ДЛЯ PKL-ОБЪЕКТОВ
# ===========================
class DopplerKnivesPriceModel:
    pass


class CaseHardenedKnivesPriceModel:
    pass


class FadeKnivesPriceModel:
    pass


# ===========================
# 2. ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ
# ===========================
def inspect_pkl(pkl_path: Path):
    print(f"\n===== {pkl_path.name} =====")
    with open(pkl_path, "rb") as f:
        obj = pickle.load(f)

    print("Тип объекта:", type(obj))

    if isinstance(obj, dict):
        print("Ключи dict:", list(obj.keys()))
    else:
        print("Атрибуты объекта:", list(obj.__dict__.keys()))

    return obj


def export_wrapper(obj, short_name: str):
    export_dir = Path("extracted") / short_name
    export_dir.mkdir(parents=True, exist_ok=True)

    # ---------- СЛУЧАЙ 1: объект — dict ----------
    if isinstance(obj, dict):
        for key, val in obj.items():
            # модель, если вдруг лежит внутри дикта
            if key == "model":
                model_path = export_dir / f"{short_name}_model.cbm"
                try:
                    val.save_model(model_path)
                    print(f"[{short_name}] Модель сохранена в {model_path}")
                except Exception as e:
                    print(f"[{short_name}] Не удалось сохранить model из dict: {e}")
                continue

            # пробуем сохранить как JSON
            try:
                with open(export_dir / f"{key}.json", "w", encoding="utf-8") as f:
                    json.dump(val, f, indent=2)
                print(f"[{short_name}] Сохранён ключ dict -> {key}.json")
            except TypeError:
                print(f"[{short_name}] Не могу сериализовать ключ {key} в JSON, пропускаю.")

        print(f"[{short_name}] Готово! Всё сохранено в {export_dir}")
        return

    # ---------- СЛУЧАЙ 2: объект — экземпляр класса ----------
    # Общие поля, которые мы можем ожидать
    fields_to_save = [
        "numeric_features",
        "categorical_features",
        "feature_cols",
        "feature_columns",
        "num_features",
        "cat_features",
        "cat_indices",
        "default_values",
    ]

    for field in fields_to_save:
        if hasattr(obj, field):
            value = getattr(obj, field)
            filename = field + ".json"
            with open(export_dir / filename, "w", encoding="utf-8") as f:
                json.dump(value, f, indent=2)
            print(f"[{short_name}] Сохранено поле {field} -> {filename}")

    # Модель
    if hasattr(obj, "model"):
        model_path = export_dir / f"{short_name}_model.cbm"
        try:
            obj.model.save_model(model_path)
            print(f"[{short_name}] Модель сохранена в {model_path}")
        except Exception as e:
            print(f"[{short_name}] Не удалось сохранить модель из wrapper'а: {e}")

    print(f"[{short_name}] Готово! Всё сохранено в {export_dir}")


# ===========================
# 3. ОСНОВНОЙ КОД
# ===========================
if __name__ == "__main__":
    ROOT = Path(__file__).resolve().parent
    MODELS_DIR = ROOT / "models"

    wrappers = [
        ("doppler_knives_wrapper.pkl", "doppler"),
        ("case_hardened_catboost_wrapper.pkl", "case_hardened"),
        ("ch_guns_wrapper.pkl", "ch_guns"),
        ("fade_guns_wrapper.pkl", "fade_guns"),
        ("fade_knives_wrapper.pkl", "fade_knives"),
        ("float_sensitive_guns_wrapper.pkl", "float_sensitive_guns"),
    ]

    for filename, short_name in wrappers:
        pkl_path = MODELS_DIR / filename
        print(f"\n=== Обработка {filename} ===")
        try:
            obj = inspect_pkl(pkl_path)
            export_wrapper(obj, short_name)
        except FileNotFoundError:
            print(f"[{short_name}] Файл {pkl_path} не найден, пропускаю.")
        except Exception as e:
            print(f"[{short_name}] Неизвестная ошибка: {e}")
