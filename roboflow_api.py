""" from fastapi import FastAPI, File, UploadFile
from fastapi.responses import JSONResponse
from inference_sdk import InferenceHTTPClient
import shutil
import os

app = FastAPI()

CLIENT = InferenceHTTPClient(
    api_url="https://serverless.roboflow.com",
    api_key="1nP8NxJMP9QsCHjudTOy"
)

@app.get("/")
async def root():
    return {"message": "Roboflow API is live!"}


@app.post("/predict/")
async def predict(file: UploadFile = File(...)):
    try:
        # Geçici olarak resmi kaydet
        file_path = f"temp_{file.filename}"
        with open(file_path, "wb") as buffer:
            shutil.copyfileobj(file.file, buffer)

        # Tahmin yap
        result = CLIENT.infer(file_path, model_id="kaskikamizelkibudowa/4")

        # Geçici resmi sil
        os.remove(file_path)

        return JSONResponse(content=result)

    except Exception as e:
        return JSONResponse(status_code=500, content={"error": str(e)})
 """
""" from fastapi import FastAPI, UploadFile, File
from inference_sdk import InferenceHTTPClient
import shutil


app = FastAPI()

@app.get("/")
async def root():
    return {"message": "Roboflow API is live!"}


# Roboflow API bilgileri
CLIENT = InferenceHTTPClient(
    api_url="https://serverless.roboflow.com",
    api_key="1nP8NxJMP9QsCHjudTOy"
)

@app.post("/analyze/")
async def analyze_image(file: UploadFile = File(...)):
    # Dosyayı kaydet
    with open(file.filename, "wb") as buffer:
        shutil.copyfileobj(file.file, buffer)

    # Roboflow ile analiz et
    result = CLIENT.infer(file.filename, model_id="kaskikamizelkibudowa/4") 
    result = CLIENT.infer(file.filename, model_id="personal-protective-equipment-combined-model/8")

    return result
 """

""" from fastapi import FastAPI, UploadFile, File
from inference_sdk import InferenceHTTPClient
import shutil
import os

# 🔽 BURASI: Roboflow istemcisi oluşturuluyor
CLIENT = InferenceHTTPClient(
    api_url="https://serverless.roboflow.com",
    api_key="1nP8NxJMP9QsCHjudTOy"
)

app = FastAPI()

@app.post("/analyze/")
async def analyze_image(file: UploadFile = File(...)):
    # Dosya geçici olarak kaydediliyor
    file_path = f"temp_{file.filename}"
    with open(file_path, "wb") as buffer:
        shutil.copyfileobj(file.file, buffer)

    # 🔽 BURASI: Modelden tahmin alınan kısım
    result = CLIENT.infer(file_path, model_id="personal-protective-equipment-combined-model/8")

    # Geçici dosya siliniyor
    os.remove(file_path)

    # Sınıflar sayılıyor
    classes = [pred["class"] for pred in result["predictions"]]
    hardhats = classes.count("Hardhat")
    vests = classes.count("Vest")
    goggles = classes.count("Goggles")
    boots = classes.count("Boots")
  # Cevap dönülüyor

    result = model.predict(image_path, confidence=40, overlap=30).json()
    preds = result.get("predictions", [])

    return {
    "total_predictions": len(preds),
    "hardhats": sum(1 for p in preds if p["class"] == "Hardhat"),
    "vests": sum(1 for p in preds if p["class"] == "Safety Vest"),
    "goggles": sum(1 for p in preds if p["class"] == "Goggles"),
    "masks": sum(1 for p in preds if p["class"] == "Mask"),
   }
 
 """
from fastapi import FastAPI, UploadFile, File
from fastapi.responses import JSONResponse
from inference_sdk import InferenceHTTPClient
import shutil, os

app = FastAPI()

# ► Roboflow istemcin
CLIENT = InferenceHTTPClient(
    api_url="https://serverless.roboflow.com",
    api_key="1nP8NxJMP9QsCHjudTOy"
)

MODEL_ID = "personal-protective-equipment-combined-model/8"   # *** tek satırda ayarla ***

@app.post("/analyze/")
async def analyze_image(file: UploadFile = File(...)):
    # 1️⃣ – Dosyayı diske kaydet
    temp_path = f"temp_{file.filename}"
    with open(temp_path, "wb") as buf:
        shutil.copyfileobj(file.file, buf)

    try:
        # 2️⃣ – Roboflow tahmini
        result = CLIENT.infer(temp_path, model_id=MODEL_ID)
        preds = result.get("predictions", [])

        # 3️⃣ – Sınıfları say
        def count(cls):  # küçük yardımcı
            return sum(1 for p in preds if p["class"].lower() == cls)

        payload = {
            "total_predictions": len(preds),
            "hardhats":  count("hardhat"),
            "vests":     count("safety vest"),     # modelde “Safety Vest”
            "goggles":   count("goggles"),
            "masks":     count("mask")
        }

        return JSONResponse(content=payload)

    except Exception as ex:
        return JSONResponse(status_code=500, content={"error": str(ex)})

    finally:
        # 4️⃣ – Geçici dosyayı sil
        if os.path.exists(temp_path):
            os.remove(temp_path)
