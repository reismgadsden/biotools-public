using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace dotnetcoresample.Pages;

public class Login_HomeModel : PageModel {
    public string? username { get; set; }

    public void OnGet(string user) {
        username = user;
    }
}