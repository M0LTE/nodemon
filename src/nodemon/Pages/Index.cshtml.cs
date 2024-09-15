using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using nodemon.Configuration;
using nodemon.Services;

namespace nodemon.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public List<SelectListItem> Modes { get; set; } = [new SelectListItem("", ""), .. NinoMode.All.Select(m => new SelectListItem($"{m.Display} - {m.Remark}", m.Id.ToString()))];

        public IList<SelectListItem> PortChannels2m { get; set; } = [];

        public IndexModel(ILogger<IndexModel> logger, IOptions<NodeMonConfig> options)
        {
            _logger = logger;

            foreach (var port in options.Value.Ports.Where(p=>p.Id == "2m"))
            {
                PortChannels2m = port.Channels?.Select(c => new SelectListItem($"{c.Id} - {c.MHz} - {c.W}W", c.Id.ToString())).ToList() ?? [];
            }
        }

        public void OnGet()
        {

        }
    }
}
