""" from roboflow import Roboflow
import supervision as sv
import cv2
import numpy as np

# === Görsel yolu ===
image_path = "wwwroot/uploads/uploaded_image.jpg"  # ARTIK YÜKLENEN GÖRSELİ OKUYOR 💡


# === Roboflow modeli ===
print("loading Roboflow workspace...")
rf = Roboflow(api_key="1nP8NxJMP9QsCHjudTOy")
project = rf.workspace().project("personal-protective-equipment-combined-model")
model = project.version(8).model

# === Tahmin yap ===
result_json = model.predict(image_path, confidence=40, overlap=30).json()
predictions = result_json.get("predictions", [])

# === Tespit yoksa uyar
if not predictions:
    print("⚠️ Görselde hiçbir nesne tespit edilmedi.")
    exit()

# === Görseli yükle
image = cv2.imread(image_path)

# === Prediction bilgilerini toparla
xyxy = []
confidences = []
class_ids = []
labels = []

for i, pred in enumerate(predictions):
    x1 = pred["x"] - pred["width"] / 2
    y1 = pred["y"] - pred["height"] / 2
    x2 = pred["x"] + pred["width"] / 2
    y2 = pred["y"] + pred["height"] / 2

    xyxy.append([x1, y1, x2, y2])
    confidences.append(pred.get("confidence", 0))
    class_ids.append(i)
    labels.append(pred.get("class", "Unknown"))

# === NumPy array’lere dönüştür
xyxy = np.array(xyxy)
confidences = np.array(confidences)
class_ids = np.array(class_ids)

# === Detections nesnesi
detections = sv.Detections(
    xyxy=xyxy,
    confidence=confidences,
    class_id=class_ids
)

# === Annotatorlar
box_annotator = sv.BoxAnnotator()
label_annotator = sv.LabelAnnotator()

annotated_image = box_annotator.annotate(scene=image, detections=detections)
annotated_image = label_annotator.annotate(scene=annotated_image, detections=detections, labels=labels)

# === Görseli kaydet
output_path = "annotated_output.jpeg"
cv2.imwrite(output_path, annotated_image)
print(f"✅ Görsel başarıyla kaydedildi: {output_path}")
 """
""" import cv2
import numpy as np
from roboflow import Roboflow
import supervision as sv

# === Yüklenen görsel yolu ===
image_path = "wwwroot/uploads/uploaded_image.jpg"

# ✅ Yeni SDK yapısı (workspace yok!)
rf = Roboflow(api_key="SENİN_API_KEYİN")  # 🔁 BURAYA KENDİ API KEY’İNİ YAZ

# ✅ Doğrudan proje adıyla yükleniyor
model = rf.load_model("personal-protective-equipment-combined-model/8")

# === Görseli oku ===
image = cv2.imread(image_path)
if image is None:
    raise Exception("Görsel yüklenemedi, path yanlış olabilir: " + image_path)

# === Tahmin yap ===
result = model.predict(image_path, confidence=40, overlap=30).json()

# === Kutu koordinatlarını oku ===
xyxy = []
confidences = []
class_ids = []
class_labels = []
label_map = {}
label_index = 0

for prediction in result["predictions"]:
    x, y, w, h = prediction["x"], prediction["y"], prediction["width"], prediction["height"]
    conf = prediction["confidence"]
    label = prediction["class"]

    x1 = x - w / 2
    y1 = y - h / 2
    x2 = x + w / 2
    y2 = y + h / 2

    xyxy.append([x1, y1, x2, y2])
    confidences.append(conf)
    class_ids.append(label_index)
    class_labels.append(label)
    label_map[label_index] = label
    label_index += 1

# === supervision kutu çizimi ===
detections = sv.Detections(
    xyxy=np.array(xyxy),
    confidence=np.array(confidences),
    class_id=np.array(class_ids)
)

annotator = sv.BoxAnnotator()
labels = [f"{label_map[c]} {conf:.0%}" for c, conf in zip(detections.class_id, detections.confidence)]
annotated = annotator.annotate(image.copy(), detections, labels)

# === Kaydet ===
cv2.imwrite("wwwroot/annotated_output.jpeg", annotated)
print("✅ Görsel başarıyla işlendi ve kutular çizildi.")
 """
# draw_predictions.py  (projenin kökünde kalsın)

from inference_sdk import InferenceHTTPClient
import supervision as sv
import cv2
import sys
import os

# ---- 1) Komut satırından dosya yolunu al
if len(sys.argv) != 2:
    print("Kullanım  : python draw_predictions.py <resim_yolu>")
    sys.exit(1)

image_path = sys.argv[1]
if not os.path.exists(image_path):
    print("Dosya bulunamadı →", image_path)
    sys.exit(1)

# ---- 2) Roboflow istemcisi
CLIENT = InferenceHTTPClient(
    api_url="https://serverless.roboflow.com",
    api_key="1nP8NxJMP9QsCHjudTOy"
)

MODEL_ID = "personal-protective-equipment-combined-model/8"

# ---- 3) Tahmin al
result = CLIENT.infer(image_path, model_id=MODEL_ID)
preds  = result["predictions"]

# ---- 4) supervision 0.25+ ile elle detections oluştur
xyxy      = []
conf      = []
class_ids = []
labels    = []

CLASS_MAP = {
    "Hardhat": 0,
    "Safety Vest": 1,
    "Goggles": 2,
    "Mask": 3
}

for p in preds:
    x      = p["x"]; y = p["y"]
    w      = p["width"]; h = p["height"]
    x1, y1 = x - w/2, y - h/2
    x2, y2 = x + w/2, y + h/2
    xyxy.append([x1, y1, x2, y2])
    conf.append(p["confidence"])
    class_ids.append(CLASS_MAP.get(p["class"], -1))
    labels.append(p["class"])

import numpy as np
detections = sv.Detections(
    xyxy      = np.array(xyxy, dtype=float),
    confidence= np.array(conf, dtype=float),
    class_id  = np.array(class_ids, dtype=int)
)

# ---- 5) Annotate
box_annotator   = sv.BoxAnnotator(color=sv.Color.red(), thickness=2)
label_annotator = sv.LabelAnnotator()

image           = cv2.imread(image_path)
annotated       = box_annotator.annotate(image.copy(), detections)
annotated       = label_annotator.annotate(annotated, detections, labels)

out_path = "wwwroot/uploads/annotated_image.jpg"
cv2.imwrite(out_path, annotated)
print("✅ Kutulu görsel kaydedildi →", out_path)
