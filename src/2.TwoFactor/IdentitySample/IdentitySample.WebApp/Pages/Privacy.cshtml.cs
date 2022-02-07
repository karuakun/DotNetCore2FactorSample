using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentitySample.WebApp.Pages;

[Authorize]
public class PrivacyModel : PageModel
{

    public PrivacyModel()
    {
    }

    public void OnGet()
    {
    }
}

