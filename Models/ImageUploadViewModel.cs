using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ISGKkdTakip.Models.ViewModels
{
    // Görsel yükleme sayfasında kullanılacak verileri taşıyan ViewModel sınıfıve bu sınıf, hem kullanıcıdan yüklenecek görseli hem de seçim listesi verilerini (mekanlar gibi) bir araya getirir.
    
    public class ImageUploadViewModel
    {
        public IFormFile File { get; set; }// Yüklediğimiz görsel dosyasının temsili gibi birşey

        public int SelectedMekanId { get; set; } // Kullanıcının açılır listeden seçtiği mekanın ID'sini tutar.

        public List<SelectListItem> MekanList { get; set; }//Mekanları içeren bir SelectListItem listesi arayüzdeki dropdown menü) doldurmak için kullanılır.
    }
}
