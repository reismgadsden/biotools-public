using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.RegularExpressions;

namespace dotnetcoresample.Pages;

public class LoginModel : PageModel {
    // [HttpPost]

    public string? username { get; set; }

    public string? password { get; set; }

    public bool passFail { get; set; } = false;

    public bool userFail { get; set; } = false;

    public void validateLogin() {
        RedirectToPage("./Login", new {userFail = false, passFail = false});
    }

    public void OnGet(bool user, bool pass) {
        userFail = user;
        passFail = pass;
    }

    public ActionResult OnPost() {

        Regex userRegex = new Regex("^[a-zA-Z0-9]+$");
        username = Request.Form["username"];
        bool userFailed = userRegex.IsMatch(username);

        Regex passRegex = new Regex("^[a-zA-Z0-9~`!@#$%^&*?]+$");
        password = Request.Form["password"];
        bool passFailed = passRegex.IsMatch(password);

        if (!userFailed || !passFailed) {
            return RedirectToPage("./Login", new {user = !userFailed, pass = !passFailed});
        }

        //this.validateLogin();
        return RedirectToPage("./Login", new {userFail = userFailed, passFail = passFailed});
        // TODO: go to next page

    }
}