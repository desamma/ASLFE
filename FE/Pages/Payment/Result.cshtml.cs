using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FE.Pages.Payment
{
    [Authorize]
    public class ResultModel : PageModel
    {
        public void OnGet() { }
    }
}