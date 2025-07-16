using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ISGKkdTakip.Models.ViewModels
{
    public class ImageUploadViewModel
    {
        public IFormFile File { get; set; }

        public int SelectedMekanId { get; set; }

        public List<SelectListItem> MekanList { get; set; }
    }
}
