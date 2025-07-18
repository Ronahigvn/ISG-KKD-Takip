""" import cv2 #Görsel işleme görevleri için (görsel okuma, yazma, çizim)
import supervision as sv  #Nesne tespiti sonuçları (kutular, etiketler) için
from inference_sdk import InferenceHTTPClient
import os
import sys
import numpy as np

# --- Roboflow API Ayarları ---
CLIENT = InferenceHTTPClient(
    api_url="https://serverless.roboflow.com",
    api_key="1nP8NxJMP9QsCHjudTOy"
)
MODEL_ID = "personal-protective-equipment-combined-model/8"

if len(sys.argv) != 3:
    print("Kullanım: python draw_predictions.py <giriş_yolu> <çıkış_yolu>", file=sys.stderr)
    sys.exit(1)

# Komut satırı argümanlarından giriş ve çıkış görsel yollarını alır
input_image_path = sys.argv[1]
output_image_path = sys.argv[2]


# Giriş görsel dosyasının gerçekten var olup olmadığını kontrol eder
if not os.path.exists(input_image_path):
    print(f"HATA: Giriş görseli bulunamadı: {input_image_path}", file=sys.stderr)
    sys.exit(1)

image = cv2.imread(input_image_path)
if image is None:
    print(f"HATA: Görsel yüklenemedi: {input_image_path}", file=sys.stderr)
    sys.exit(1)

# --- Roboflow API ile Tahminleri Alma ---
try:
    raw_predictions = CLIENT.infer(input_image_path, model_id=MODEL_ID)
    preds = raw_predictions.get("predictions", [])

    if ("error" in raw_predictions or "message" in raw_predictions) and not preds:
        print(f"HATA: Roboflow API hatası: {raw_predictions.get('error', raw_predictions.get('message', 'Bilinmeyen hata'))}", file=sys.stderr)
        sys.exit(1)
except Exception as e:
    print(f"HATA: Roboflow API hatası: {e}", file=sys.stderr)
    sys.exit(1)


# Tespit edilen her bir KKD türünün sayısını tutan sözlük 
ppe_counts = {
    "hardhat": 0,
    "safety vest": 0,
    "goggles": 0,
    "mask": 0
}



# --- Tespit Sonuçlarını Supervision Kütüphanesi Formatına Hazırlama ---
# Sınırlayıcı kutu koordinatlarını (x1, y1, x2, y2) saklamak için liste

xyxy = []
# Güvenilirlik skorlarını saklamak için liste
conf = []
# Sınıf kimliklerini (id) saklamak için liste
class_ids = []
# Sınıf isimlerini saklamak için liste (Supervision kütüphanesi için)
sv_class_names = []
# Roboflow'dan gelen tüm sınıf isimlerini alır, tekrar edenleri kaldırır ve alfabetik sıralar
all_model_class_names = sorted(list(set([p["class"] for p in preds])))
# Sınıf isimlerini sayısal kimliklere eşleyen bir sözlük oluşturur
class_name_to_id = {name: i for i, name in enumerate(all_model_class_names)}
# Supervision için sınıf isimlerini atar
sv_class_names = all_model_class_names

# Her bir tahmini (prediction) döngüye alır
for p in preds:
    x, y = p["x"], p["y"]
    w, h = p["width"], p["height"]
    x1, y1 = x - w / 2, y - h / 2
    x2, y2 = x + w / 2, y + h / 2
    xyxy.append([x1, y1, x2, y2])
    conf.append(p["confidence"])
    class_name = p["class"]
    
    if class_name.lower() == "person":
        person_count += 1

 # Roboflow modelinden gelebilecek farklı isimlendirmeleri standartlaştırır
    norm_name = class_name.lower().replace(" ", "")
    if norm_name == "safetyvest":
        norm_name = "safety vest"
    elif norm_name == "hardhat":
        norm_name = "hardhat"

    if norm_name in ppe_counts:
        ppe_counts[norm_name] += 1

    class_ids.append(class_name_to_id.get(class_name, -1))
    # 💥 Tahmin yoksa (boşsa) hata vermeden güvenli çık
if len(xyxy) == 0:
    print("\n--- KKE ve Kişi Sayım Sonuçları ---")
    for ppe_type in ppe_counts:
        display_name = ppe_type.replace('_', ' ').title()
        print(f"{display_name}: 0")
    
    # Boş görseli aynen kaydet
    cv2.imwrite(output_image_path, image)
    print("⚠️ Hiçbir nesne tespit edilmedi. Boş görsel kaydedildi.")
    sys.exit(0)


# --- Supervision Detections Nesnesini Oluşturma ---
detections = sv.Detections(
    xyxy=np.array(xyxy, dtype=float),
    confidence=np.array(conf, dtype=float),
    class_id=np.array(class_ids, dtype=int),
)
detections.class_name = sv_class_names
# --- Kutulayıcı ve Etiketleyici Annotator'ları Tanımlama ---
# Sınırlayıcı kutuları çizmek için BoxAnnotator nesnesi oluşturur
box_annotator = sv.BoxAnnotator(thickness=2)
label_annotator = sv.LabelAnnotator(
    text_thickness=2,
    text_scale=0.8,
    text_color=sv.Color.WHITE
)
# Orijinal görselin bir kopyasını alır (üzerine çizim yapmak için)
annotated_image = image.copy()

# --- Tahminleri Görsel Üzerine Çizme ---
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

  
# İşlenmiş (kutulu ve etiketli) görseli belirtilen çıkış yoluna kaydeder
cv2.imwrite(output_image_path, annotated_image)
# Sonuçları JSON olarak döndür
print(json.dumps({
    "output_image_path": output_image_path,
    "ppe_counts": ppe_counts
}))

# --- KKE ve Kişi Sayım Sonuçlarını C# tarafının okuyabileceği formatta yazdır ---
print("\n--- KKE ve Kişi Sayım Sonuçları ---")
for ppe_type, count in ppe_counts.items():
    display_name = ppe_type.replace('_', ' ').title() # Örneğin "hardhat" -> "Hardhat"
    print(f"{display_name}: {count}")








# Toplam kişi sayısını hesapla (basit toplama)
toplam_kisi = sum(ppe_counts.values())
print(f"toplamkisi: {toplam_kisi}")









toplam_kisi = sum(ppe_counts.values())
print(f"Toplam Kişi: {toplam_kisi}")


print(f"✅ Kutulu görsel kaydedildi → {output_image_path}")






    """
""" 
import cv2
import supervision as sv
from inference_sdk import InferenceHTTPClient
import os
import sys
import numpy as np
import json  # ✅ JSON çıktısı için eklendi

# --- Roboflow API Ayarları ---
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

person_count = 0  # ✅ EKSİK OLAN DEĞİŞKEN EKLENDİ

for p in preds:
    x, y = p["x"], p["y"]
    w, h = p["width"], p["height"]
    x1, y1 = x - w / 2, y - h / 2
    x2, y2 = x + w / 2, y + h / 2
    xyxy.append([x1, y1, x2, y2])
    conf.append(p["confidence"])
    class_name = p["class"]

    if class_name.lower() == "person":
        person_count += 1  # ✅ DOĞRU YERE EKLENDİ

    norm_name = class_name.lower().replace(" ", "")
    if norm_name == "safetyvest":
        norm_name = "safety vest"
    elif norm_name == "hardhat":
        norm_name = "hardhat"

    if norm_name in ppe_counts:
        ppe_counts[norm_name] += 1

    class_ids.append(class_name_to_id.get(class_name, -1))

if len(xyxy) == 0:
    print("\n--- KKE ve Kişi Sayım Sonuçları ---")
    for ppe_type in ppe_counts:
        display_name = ppe_type.replace('_', ' ').title()
        print(f"{display_name}: 0")
    
    cv2.imwrite(output_image_path, image)
    print("⚠️ Hiçbir nesne tespit edilmedi. Boş görsel kaydedildi.")
    sys.exit(0)

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

print("\n--- KKE ve Kişi Sayım Sonuçları ---")
for ppe_type, count in ppe_counts.items():
    display_name = ppe_type.replace('_', ' ').title()
    print(f"{display_name}: {count}")

# ✅ TOPLAM KİŞİ ARTIK "person" sınıfından hesaplanıyor
print(f"Toplam Kişi: {person_count}")

# ✅ C# backend için JSON çıktı da ekleniyor
print(json.dumps({
    "output_image_path": output_image_path,
    "ppe_counts": ppe_counts,
    "total_person_count": person_count
}))
print(f"✅ Kutulu görsel kaydedildi → {output_image_path}")


 """

import cv2 
import supervision as sv  
from inference_sdk import InferenceHTTPClient
import os
import sys
import numpy as np
import json  # JSON çıktısı için eklendi

# --- Roboflow API Ayarları ---
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

# --- Roboflow API ile Tahminleri Alma ---
try:
    raw_predictions = CLIENT.infer(input_image_path, model_id=MODEL_ID)
    preds = raw_predictions.get("predictions", [])

    if ("error" in raw_predictions or "message" in raw_predictions) and not preds:
        print(f"HATA: Roboflow API hatası: {raw_predictions.get('error', raw_predictions.get('message', 'Bilinmeyen hata'))}", file=sys.stderr)
        sys.exit(1)
except Exception as e:
    print(f"HATA: Roboflow API hatası: {e}", file=sys.stderr)
    sys.exit(1)

# Tespit edilen her bir KKD türünün sayısını tutan sözlük
# 🎉 "person" sınıfını buraya ekleyerek tüm sayımları tek bir sözlükte topluyoruz.
ppe_counts = {
    "hardhat": 0,
    "safety vest": 0,
    "goggles": 0,
    "mask": 0,
    "person": 0 # 🎉 Kişi sayımı için başlangıç değeri eklendi
}

# --- Tespit Sonuçlarını Supervision Kütüphanesi Formatına Hazırlama ---
xyxy = []
conf = []
class_ids = []
sv_class_names = []

all_model_class_names = sorted(list(set([p["class"] for p in preds])))
class_name_to_id = {name: i for i, name in enumerate(all_model_class_names)}
sv_class_names = all_model_class_names

# Her bir tahmini (prediction) döngüye alır
for p in preds:
    x, y = p["x"], p["y"]
    w, h = p["width"], p["height"]
    x1, y1 = x - w / 2, y - h / 2
    x2, y2 = x + w / 2, y + h / 2
    xyxy.append([x1, y1, x2, y2])
    conf.append(p["confidence"])
    class_name = p["class"]
    
    # 🎉 norm_name'i her p için hesapla ve doğrudan ppe_counts'a ekle
    norm_name = class_name.lower().replace(" ", "")
    if norm_name == "safetyvest":
        norm_name = "safety vest"
    elif norm_name == "hardhat":
        norm_name = "hardhat"
    # Eğer modeliniz 'person' sınıfını döndürüyorsa, bu da otomatik olarak ppe_counts'a eklenecek.
    # Bu yüzden ayrı bir 'person_count' değişkenine artık gerek kalmadı, hepsi ppe_counts içinde.

    if norm_name in ppe_counts: # Eğer normalize edilmiş isim ppe_counts'ta varsa
        ppe_counts[norm_name] += 1 # Sayısını artır
    else:
        # 🎉 ppe_counts'ta olmayan yeni bir sınıf tespit edilirse de ekle (opsiyonel ama iyi bir pratik)
        ppe_counts[norm_name] = 1 
        # Yeni sınıf için bir ID atamasını da güncelle (opsiyonel ama tutarlılık için)
        if norm_name not in class_name_to_id:
            class_name_to_id[norm_name] = len(all_model_class_names) # Yeni ID
            all_model_class_names.append(norm_name) # Yeni sınıf adını listeye ekle
            sv_class_names = all_model_class_names # sv_class_names'ı güncelle

    class_ids.append(class_name_to_id.get(class_name, -1))

# 💥 Tahmin yoksa (boşsa) hata vermeden güvenli çık
if len(xyxy) == 0:
    print("\n--- KKE ve Kişi Sayım Sonuçları ---")
    # 🎉 Tüm PPE ve kişi sayımlarını 0 olarak yazdır
    for ppe_type, count in ppe_counts.items():
        display_name = ppe_type.replace('_', ' ').title()
        print(f"{display_name}: 0")
    
    # Boş görseli aynen kaydet
    cv2.imwrite(output_image_path, image)
    print("⚠️ Hiçbir nesne tespit edilmedi. Boş görsel kaydedildi.")
    # 🎉 JSON çıktısını da boş olarak döndürüyoruz
    print(json.dumps({
        "output_image_path": output_image_path,
        "ppe_counts": ppe_counts, # Boş sayımlar ile
        "total_person_count": 0 # Kişi sayısı 0
    }))
    sys.exit(0)


# --- Supervision Detections Nesnesini Oluşturma ---
detections = sv.Detections(
    xyxy=np.array(xyxy, dtype=float),
    confidence=np.array(conf, dtype=float),
    class_id=np.array(class_ids, dtype=int),
)
detections.class_name = sv_class_names

# --- Kutulayıcı ve Etiketleyici Annotator'ları Tanımlama ---
box_annotator = sv.BoxAnnotator(thickness=2)
label_annotator = sv.LabelAnnotator(
    text_thickness=2,
    text_scale=0.8,
    text_color=sv.Color.WHITE
)

annotated_image = image.copy()

# --- Tahminleri Görsel Üzerine Çizme ---
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

# İşlenmiş (kutulu ve etiketli) görseli belirtilen çıkış yoluna kaydeder
cv2.imwrite(output_image_path, annotated_image)

# --- KKE ve Kişi Sayım Sonuçlarını C# tarafının okuyabileceği formatta yazdır ---
print("\n--- KKE ve Kişi Sayım Sonuçları ---")
for ppe_type, count in ppe_counts.items():
    display_name = ppe_type.replace('_', ' ').title()
    print(f"{display_name}: {count}")

# 🎉 Toplam kişi sayısını doğrudan ppe_counts'tan alıyoruz
# Eğer 'person' sınıfı tespit edilmediyse 0 olacaktır
total_person_count = ppe_counts.get("person", 0) 
print(f"Toplam Kişi: {total_person_count}") # Python çıktısında görünecek

# 🎉 C# backend için JSON çıktı, tüm bilgileri içeriyor
print(json.dumps({
    "output_image_path": output_image_path,
    "ppe_counts": ppe_counts, # Tüm KKD ve kişi sayımlarını içerir
    "total_person_count": total_person_count # Toplam kişi sayısını da ayrıca ekledik
}))

print(f"✅ Kutulu görsel kaydedildi → {output_image_path}")