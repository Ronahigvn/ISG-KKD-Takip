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
""" 
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
 """
""" from inference_sdk import InferenceHTTPClient
import supervision as sv
import cv2
import sys
import os
import numpy as np


if len(sys.argv) != 3:
    print("Kullanım: python draw_predictions.py <giriş_yolu> <çıkış_yolu>")
    sys.exit(1)

input_path = sys.argv[1]
output_path = sys.argv[2]

if not os.path.exists(input_path):
    print("❌ Giriş dosyası bulunamadı:", input_path)
    sys.exit(1)

image_path = sys.argv[1]
output_path = sys.argv[2]

if not os.path.exists(image_path):
    print("Dosya bulunamadı →", image_path)
    sys.exit(1)

CLIENT = InferenceHTTPClient(
    api_url="https://serverless.roboflow.com",
    api_key="1nP8NxJMP9QsCHjudTOy"
)

MODEL_ID = "personal-protective-equipment-combined-model/8"

result = CLIENT.infer(image_path, model_id=MODEL_ID)
preds = result["predictions"]

xyxy = []
conf = []
class_ids = []
labels = []

CLASS_MAP = {
    "Hardhat": 0,
    "Safety Vest": 1,
    "Goggles": 2,
    "Mask": 3
}

for p in preds:
    x, y = p["x"], p["y"]
    w, h = p["width"], p["height"]
    x1, y1 = x - w / 2, y - h / 2
    x2, y2 = x + w / 2, y + h / 2
    xyxy.append([x1, y1, x2, y2])
    conf.append(p["confidence"])
    class_ids.append(CLASS_MAP.get(p["class"], -1))
    labels.append(p["class"])

detections = sv.Detections(
    xyxy=np.array(xyxy, dtype=float),
    confidence=np.array(conf, dtype=float),
    class_id=np.array(class_ids, dtype=int)
)

image = cv2.imread(image_path)
box_annotator = sv.BoxAnnotator(color=sv.Color.RED, thickness=2)
annotated     = box_annotator.annotate(image.copy(), detections)
cv2.imwrite(output_path, annotated)
print("✅ Kutulu görsel kaydedildi →", output_path)


 """

""" import cv2
import supervision as sv
from inference_sdk import InferenceHTTPClient
import os
import sys # Komut satırı argümanları için gerekli
import numpy as np # NumPy array işlemleri için gerekli

# --- Roboflow Client Tanımlaması ---
# Kendi API anahtarınızı ve model ID'nizi buraya girin.
# Bu değerler Roboflow projenizden alınmalıdır.
CLIENT = InferenceHTTPClient(
    api_url="https://serverless.roboflow.com", # <<< KULLANICININ BELİRTTİĞİ DOĞRU URL >>>
    api_key="1nP8NxJMP9QsCHjudTOy" # <<< KULLANICININ BELİRTTİĞİ DOĞRU API ANAHTARI >>>
)
MODEL_ID = "personal-protective-equipment-combined-model/8" # <<< KULLANICININ BELİRTTİĞİ DOĞRU MODEL ID'Sİ >>>

# --- Komut Satırı Argümanlarını İşleme ---
# Betik, iki argüman bekler: giriş görsel yolu ve çıkış görsel yolu.
if len(sys.argv) != 3:
    print("Kullanım: python draw_predictions.py <giriş_yolu> <çıkış_yolu>", file=sys.stderr)
    sys.exit(1) # Hata kodu ile çıkış yap

input_image_path = sys.argv[1]
output_image_path = sys.argv[2]

# Giriş görsel dosyasının varlığını kontrol et
if not os.path.exists(input_image_path):
    print(f"HATA: Giriş görseli bulunamadı: {input_image_path}. Lütfen yolu kontrol edin.", file=sys.stderr)
    sys.exit(1)

# --- Görseli Yükle ---
# OpenCV kullanarak görseli oku.
image = cv2.imread(input_image_path)
if image is None:
    print(f"HATA: Görsel yüklenemedi: {input_image_path}", file=sys.stderr)
    sys.exit(1)

# --- Roboflow API'den Tahminleri Al ---
# Roboflow Inference API'sine görseli gönder ve algılama tahminlerini al.
try:
    # serverless.roboflow.com doğrudan bir Python sözlüğü (dict) döndürür.
    # Bu yüzden .json() metodunu çağırmıyoruz.
    raw_predictions = CLIENT.infer(input_image_path, model_id=MODEL_ID)
    
    # Tahminler listesini "predictions" anahtarından al.
    # Eğer "predictions" anahtarı yoksa boş bir liste döndürür.
    preds = raw_predictions.get("predictions", []) 
    
    # API'den hata döndüyse ve bu bir sözlükse, burada kontrol edebiliriz.
    if "error" in raw_predictions or "message" in raw_predictions and not preds:
        print(f"HATA: Roboflow API'den hata yanıtı alındı: {raw_predictions.get('error', raw_predictions.get('message', 'Bilinmeyen hata'))}", file=sys.stderr)
        sys.exit(1)

except Exception as e:
    print(f"HATA: Roboflow API'den tahmin alınırken beklenmedik bir hata oluştu: {e}", file=sys.stderr)
    sys.exit(1)

# --- KKE Sayımlarını Başlatma ---
# Her bir KKE türü için başlangıç sayım değerlerini sıfırla.
# Bu anahtarlar, C# tarafındaki Dictionary anahtarlarıyla uyumlu olmalıdır.
ppe_counts = {
    "hardhat": 0,
    "safety vest": 0,
    "goggles": 0,
    "mask": 0
}

# --- Tahminleri supervision için NumPy Dizilerine Dönüştürme ve Sayım Yapma ---
xyxy = []        # Sınır kutusu koordinatları (x1, y1, x2, y2)
conf = []        # Güven skorları
class_ids = []   # Sınıf ID'leri (tam sayı)
# supervision.Detections objesi için sınıf isimleri listesi
# Bu liste, class_id'leri sınıf isimlerine dönüştürmek için kullanılacak.
sv_class_names = [] 

# Roboflow'dan gelen tüm benzersiz sınıf isimlerini topla ve sırala.
# Bu liste, supervision'ın dahili olarak kullanacağı sınıf isimleri haritasını oluşturmak içindir.
all_model_class_names = sorted(list(set([p["class"] for p in preds])))
# Sınıf isminden ID'ye harita oluştur
class_name_to_id = {name: i for i, name in enumerate(all_model_class_names)}
sv_class_names = all_model_class_names # supervision Detections objesi için sınıf isimleri listesi

for p in preds:
    # Sınır kutusu koordinatlarını hesapla (merkezden köşelere)
    x, y = p["x"], p["y"]
    w, h = p["width"], p["height"]
    x1, y1 = x - w / 2, y - h / 2
    x2, y2 = x + w / 2, y + h / 2
    xyxy.append([x1, y1, x2, y2])
    conf.append(p["confidence"])
    
    # Modelin döndürdüğü orijinal sınıf adını al
    class_name_from_model = p["class"]
    
    # KKE Sayımlarını yap
    # C# tarafının beklediği anahtarlarla uyumlu hale getir.
    # Büyük/küçük harf ve boşluk farklılıklarını gidermek için normalizasyon yapıyoruz.
    normalized_for_count = class_name_from_model.lower().replace(" ", "")
    
    # Özel durumlar için düzeltmeler (eğer modeliniz 'Safety Vest' döndürüp sizin anahtarınız 'safety vest' ise)
    if normalized_for_count == "safetyvest":
        normalized_for_count = "safety vest"
    elif normalized_for_count == "hardhat":
        normalized_for_count = "hardhat"
    # Diğerleri zaten küçük harf ve boşluksuz ise doğrudan eşleşir

    if normalized_for_count in ppe_counts:
        ppe_counts[normalized_for_count] += 1

    # supervision için sınıf ID'sini al
    # Eğer sınıf adı haritada yoksa (beklenmeyen bir sınıf), -1 atarız.
    class_ids.append(class_name_to_id.get(class_name_from_model, -1))

# --- supervision Detections objesini oluşturma ---
# Detections sınıfının constructor'ı 'class_name_map' argümanını almaz.
# Bunun yerine, sınıf isimleri listesini 'class_name' özelliğine atarız.
detections = sv.Detections(
    xyxy=np.array(xyxy, dtype=float),
    confidence=np.array(conf, dtype=float),
    class_id=np.array(class_ids, dtype=int),
)
# Detections objesine sınıf isimleri listesini atama
detections.class_name = sv_class_names 

# --- Görsel Üzerine Çizim İçin Annotator'ları Tanımlama ---
# Sınır kutuları için annotator
box_annotator = sv.BoxAnnotator(thickness=2)
# Etiketler (sınıf adı ve güven) için annotator
label_annotator = sv.LabelAnnotator(
    text_thickness=2,
    text_scale=0.8,
    text_color=sv.Color.WHITE # Etiket metin rengi
)

annotated_image = image.copy() # Orijinal görselin kopyası üzerinde çalış

# --- Tahminleri Döngüye Al ve Çizim Uygula (Renkli Kutular ve Etiketler) ---
# Her bir tespit edilen nesne (detection) için işlem yapıyoruz.
for i in range(len(detections)):
    class_id = detections.class_id[i]
    class_name = detections.class_name[class_id]
    confidence = detections.confidence[i]
    box = detections.xyxy[i]

    display_label = f"{class_name.replace('_', ' ').title()}: {confidence:.2f}"

    color_for_box = sv.Color.GREEN
    if class_name == "Hardhat":
        color_for_box = sv.Color.RED
        display_label = f"Kask: {confidence:.2f}"
    elif class_name == "Safety Vest":
        color_for_box = sv.Color.BLUE
    elif class_name == "Goggles":
        color_for_box = sv.Color.YELLOW
    elif class_name == "Mask":
        color_for_box = sv.Color.PURPLE

    single_detection = sv.Detections(
        xyxy=box.reshape(1, -1),
        class_id=np.array([class_id]),
        confidence=np.array([confidence]),
    )

    annotated_image = box_annotator.annotate(
        scene=annotated_image,
        detections=single_detection,
        color=color_for_box
    )

    annotated_image = label_annotator.annotate(
        scene=annotated_image,
        detections=single_detection,
        labels=[display_label],
        color=color_for_box
    )


# --- İşlenmiş Görseli Kaydet ---
# İşlenmiş görseli belirtilen çıkış yoluna kaydet.
cv2.imwrite(output_image_path, annotated_image)

# --- KKE Sayım Sonuçlarını C# tarafının okuyabileceği formatta yazdır ---
# Bu satırlar C# tarafından yakalanacak ve parse edilecek.
# Her bir KKE türü için ayrı bir satırda "TÜR: SAYI" formatında yazdırılır.
for ppe_type, count in ppe_counts.items():
    print(f"{ppe_type}: {count}")


 

# Görselin başarıyla kaydedildiğini belirten son mesajı yazdır.
print(f"✅ Kutulu görsel kaydedildi → {output_image_path}")

# --- İsteğe Bağlı: Görseli Bir Pencerede Göster (Sadece Geliştirme/Test Amaçlı) ---
# Bu kısım, betiği doğrudan çalıştırdığınızda görseli bir pencerede göstermek içindir.
# Web uygulamanızda kullanılıyorsa genellikle buna gerek yoktur.
# cv2.imshow("Algılanan KKE'ler", annotated_image)
# cv2.waitKey(0) # Kullanıcı bir tuşa basana kadar pencereyi açık tut
# cv2.destroyAllWindows() # Tüm OpenCV pencerelerini kapat
 """

import cv2
import supervision as sv
from inference_sdk import InferenceHTTPClient
import os
import sys
import numpy as np

CLIENT = InferenceHTTPClient(
    api_url="https://serverless.roboflow.com",
    api_key="1nP8NxJMP9QsCHjudTOy"
)
MODEL_ID = "personal-protective-equipment-combined-model/8"

if len(sys.argv) != 3:
    print("Kullanım: python draw_predictions.py <giriş_yolu> <çıkış_yolu>", file=sys.stderr)
    sys.exit(1)

input_image_path = sys.argv[1]
output_image_path = sys.argv[2]

if not os.path.exists(input_image_path):
    print(f"HATA: Giriş görseli bulunamadı: {input_image_path}", file=sys.stderr)
    sys.exit(1)

image = cv2.imread(input_image_path)
if image is None:
    print(f"HATA: Görsel yüklenemedi: {input_image_path}", file=sys.stderr)
    sys.exit(1)

try:
    raw_predictions = CLIENT.infer(input_image_path, model_id=MODEL_ID)
    preds = raw_predictions.get("predictions", [])

    if ("error" in raw_predictions or "message" in raw_predictions) and not preds:
        print(f"HATA: Roboflow API hatası: {raw_predictions.get('error', raw_predictions.get('message', 'Bilinmeyen hata'))}", file=sys.stderr)
        sys.exit(1)
except Exception as e:
    print(f"HATA: Roboflow API hatası: {e}", file=sys.stderr)
    sys.exit(1)

ppe_counts = {
    "hardhat": 0,
    "safety vest": 0,
    "goggles": 0,
    "mask": 0
}

xyxy = []
conf = []
class_ids = []
sv_class_names = []

all_model_class_names = sorted(list(set([p["class"] for p in preds])))
class_name_to_id = {name: i for i, name in enumerate(all_model_class_names)}
sv_class_names = all_model_class_names

for p in preds:
    x, y = p["x"], p["y"]
    w, h = p["width"], p["height"]
    x1, y1 = x - w / 2, y - h / 2
    x2, y2 = x + w / 2, y + h / 2
    xyxy.append([x1, y1, x2, y2])
    conf.append(p["confidence"])
    class_name = p["class"]

    norm_name = class_name.lower().replace(" ", "")
    if norm_name == "safetyvest":
        norm_name = "safety vest"
    elif norm_name == "hardhat":
        norm_name = "hardhat"

    if norm_name in ppe_counts:
        ppe_counts[norm_name] += 1

    class_ids.append(class_name_to_id.get(class_name, -1))


detections = sv.Detections(
    xyxy=np.array(xyxy, dtype=float),
    confidence=np.array(conf, dtype=float),
    class_id=np.array(class_ids, dtype=int),
)
detections.class_name = sv_class_names

box_annotator = sv.BoxAnnotator(thickness=2)
label_annotator = sv.LabelAnnotator(
    text_thickness=2,
    text_scale=0.8,
    text_color=sv.Color.WHITE
)

annotated_image = image.copy()

for i in range(len(detections)):
    class_id = detections.class_id[i]
    class_name = detections.class_name[class_id]
    confidence = detections.confidence[i]
    box = detections.xyxy[i]

    display_label = f"{class_name.replace('_', ' ').title()}: {confidence:.2f}"

    single_detection = sv.Detections(
        xyxy=box.reshape(1, -1),
        class_id=np.array([class_id]),
        confidence=np.array([confidence]),
    )

    annotated_image = box_annotator.annotate(
        scene=annotated_image,
        detections=single_detection
    )

    annotated_image = label_annotator.annotate(
        scene=annotated_image,
        detections=single_detection,
        labels=[display_label]
    )

cv2.imwrite(output_image_path, annotated_image)

# --- KKE ve Kişi Sayım Sonuçlarını C# tarafının okuyabileceği formatta yazdır ---
print("\n--- KKE ve Kişi Sayım Sonuçları ---")
for ppe_type, count in ppe_counts.items():
    display_name = ppe_type.replace('_', ' ').title() # Örneğin "hardhat" -> "Hardhat"
    print(f"{display_name}: {count}")
""" 
# Toplam kişi sayısını hesapla (basit toplama)
toplam_kisi = sum(ppe_counts.values())
print(f"toplamkisi: {toplam_kisi}") """


print(f"✅ Kutulu görsel kaydedildi → {output_image_path}")






   