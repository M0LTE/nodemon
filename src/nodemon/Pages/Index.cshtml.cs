using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using nodemon.Services;

namespace nodemon.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public List<SelectListItem> Modes { get; set; } = [new SelectListItem("", ""), .. NinoMode.All.Select(m => new SelectListItem(m.Display, m.Id.ToString()))];

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}
