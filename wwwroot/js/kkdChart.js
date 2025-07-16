document.addEventListener('DOMContentLoaded', function() {
    // ViewBag'den gelen veriler, HTML tarafından doğrudan buraya enjekte edilecek.
    // Bu JavaScript dosyası, Result.cshtml içinde çağrıldığı için ViewBag değerlerine erişebilir.
    // Ancak, direkt olarak @ViewBag.Hardhats şeklinde erişemez.
    // Result.cshtml'nin bu değerleri bir HTML elementi aracılığıyla veya global JS değişkenlerine atayarak aktarması gerekir.
    // En iyi yöntem, Result.cshtml içinde bu değerleri bir script bloğunda tanımlayıp global yapmaktır.
    // Alternatif olarak, bu JS dosyasını Result.cshtml içinde, ViewBag değerlerini kullanarak çağırılan bir fonksiyon olarak da düşünebiliriz.

    // Result.cshtml'den gelen global değişkenleri burada kullanıyoruz.
    // Result.cshtml'nin içinde bu değişkenlerin tanımlandığından emin olun:
    // <script>
    //     const hardhats = @Html.Raw(Json.Serialize((int)(ViewBag.Hardhats ?? 0)));
    //     const vests = @Html.Raw(Json.Serialize((int)(ViewBag.Vests ?? 0)));
    //     const goggles = @Html.Raw(Json.Serialize((int)(ViewBag.Goggles ?? 0)));
    //     const masks = @Html.Raw(Json.Serialize((int)(ViewBag.Masks ?? 0)));
    // </script>
    // Bu yaklaşım, JS dosyasını daha taşınabilir yapar.

    // Eğer Result.cshtml'de global olarak tanımlanmışlarsa, doğrudan kullanabiliriz.
    // Aksi takdirde, bu kod çalışmayacaktır.
    // Güvenli bir yol, bu değerleri bir fonksiyon aracılığıyla almak veya HTML'deki data-attribute'lardan okumaktır.
    // Şimdilik, Result.cshtml'de global değişkenlerin tanımlandığını varsayalım.

    const hardhats = typeof window.hardhats !== 'undefined' ? window.hardhats : 0;
    const vests = typeof window.vests !== 'undefined' ? window.vests : 0;
    const goggles = typeof window.goggles !== 'undefined' ? window.goggles : 0;
    const masks = typeof window.masks !== 'undefined' ? window.masks : 0;

    // Hata ayıklama için konsola yazdırın
    console.log('KKD Sayıları (kkdChart.js):', {
        hardhats: hardhats,
        vests: vests,
        goggles: goggles,
        masks: masks
    });

    // Eğer tüm sayılar 0 ise grafiği gizleyebilir veya özel bir mesaj gösterebilirsiniz.
    if (hardhats === 0 && vests === 0 && goggles === 0 && masks === 0) {
        console.warn('Tüm KKD sayıları sıfır. Grafik boş görünebilir.');
        const chartContainer = document.getElementById('kkdDonutChart'); // Canvas'ın ID'si
        if (chartContainer) {
            const parentCard = chartContainer.closest('.dashboard-card');
            if (parentCard) {
                parentCard.innerHTML = '<h2 style="color: #e84393;">📈 KKE Dağılımı</h2><p class="text-muted mt-3">Grafik verisi bulunamadı.</p>';
            }
        }
        return; // Grafik oluşturmadan çık
    }

    const ctx = document.getElementById('kkdDonutChart').getContext('2d'); // Canvas ID'sinin 'kkdDonutChart' olduğundan emin ol

    const kkdDonutChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Kask', 'Yelek', 'Gözlük', 'Maske'],
            datasets: [{
                label: 'KKD Dağılımı',
                data: [hardhats, vests, goggles, masks],
                backgroundColor: [
                    '#e84393', /* Kask - Tatlı Pembe */
                    '#6a5acd', /* Yelek - Mor tonu */
                    '#ffc107', /* Gözlük - Sarı */
                    '#20c997'  /* Maske - Turkuaz yeşil */
                ],
                borderColor: '#fff',
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '60%',
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: {
                        font: {
                            size: 14
                        }
                    }
                },
                title: {
                    display: true,
                    text: 'Kişisel Koruyucu Donanım Dağılımı',
                    font: {
                        size: 18,
                        weight: 'bold'
                    },
                    padding: {
                        top: 10,
                        bottom: 20
                    }
                }
            }
        }
    });
});
