from fastapi import FastAPI, UploadFile, File
from fastapi.responses import JSONResponse, FileResponse
from fastapi.middleware.cors import CORSMiddleware
from inference_sdk import InferenceHTTPClient
import shutil, os

# FastAPI uygulamasını başlatır
app = FastAPI()


# --- CORS Ayarları ---
# CORS middleware'i ekler. Bu, farklı bir alan adından (örneğin bir frontend uygulamasından) API'ye istek göndermenizi sağlar.
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# --- Roboflow API İstemcisi Ayarları ---
CLIENT = InferenceHTTPClient(
    api_url="https://serverless.roboflow.com",
    api_key="1nP8NxJMP9QsCHjudTOy"
)

MODEL_ID = "personal-protective-equipment-combined-model/8"


# --- GÖRSEL ANALİZ ENDPOİNT'İ ---
# `/analyze/` yoluna gelen POST isteklerini işleyen asenkron fonksiyon
# Bir görsel dosyası yüklemesini bekler
@app.post("/analyze/")
async def analyze_image(file: UploadFile = File(...)):
    temp_path = f"temp_{file.filename}"
    with open(temp_path, "wb") as buf:
        shutil.copyfileobj(file.file, buf)

    try:
        # Roboflow API'ye kaydedilen görseli göndererek nesne tespiti tahminlerini alır
        result = CLIENT.infer(temp_path, model_id=MODEL_ID)
         # Gelen yanıttan 'predictions' anahtarını alır, yoksa boş liste döndürür
        preds = result.get("predictions", [])

        # Her bir KKD türünün sayısını hesaplamak için yardımcı bir iç fonksiyon tanımlar
        def count(cls): return sum(1 for p in preds if p["class"].lower() == cls)
        # Yanıt olarak döndürülecek veri yükünü (payload) oluşturur
        payload = {
            "total_predictions": len(preds),
            "hardhats":  count("hardhat"),
            "vests":     count("safety vest"),
            "goggles":   count("goggles"),
            "masks":     count("mask")
        }
          # JSON formatında başarılı bir yanıt döndürür
        return JSONResponse(content=payload)

    except Exception as ex:
        return JSONResponse(status_code=500, content={"error": str(ex)})

    finally:
        if os.path.exists(temp_path):
            os.remove(temp_path)
# --- ETİKETLENMİŞ GÖRSELİ DÖNDÜRME ENDPOİNT'İ ---
# `/annotated-image/` yoluna gelen GET isteklerini işleyen asenkron fonksiyon
# (Not: Bu endpoint, tahminlerin çizildiği bir görselin varlığını varsayar.
#  Normalde bu görselin, `analyze_image` endpoint'i içinde veya ayrı bir işlemle
#  oluşturulup diske kaydedilmesi gerekir. Şu anki `analyze_image` bu kaydetme işlemini yapmıyor.)
@app.get("/annotated-image/")
async def get_annotated_image():
    return FileResponse("annotated_output.jpeg", media_type="image/jpeg")
