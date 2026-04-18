using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace FE.Pages.Gacha
{
    // Yêu cầu user phải đăng nhập mới được vào trang Gacha
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            // Mọi dữ liệu (Gems, Pity, Banner) sẽ được load qua JS Fetch API
        }
    }
}