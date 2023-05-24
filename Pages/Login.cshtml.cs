using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace dotnetcoresample.Pages;

public class LoginModel : PageModel {

    // holds current user
    public string? username { get; set; }

    // holds password that is entered
    public string? password { get; set; }

    // holds whether the password was valid
    public bool passFail { get; set; } = false;

    // holds whether the username was valid
    public bool userFail { get; set; } = false;

    // helper function for hasing password
    public byte[] GetHash() {
        if (password == null) return Array.Empty<byte>();
        using (HashAlgorithm alg = SHA256.Create())
            return alg.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    // computes the hash of the password
    public String hashPass() {
        if (password == null) return "";

        StringBuilder sb = new StringBuilder();
        foreach (byte b in GetHash()) sb.Append(b.ToString("X2"));

        return sb.ToString();
    }

    
    /*
        What happens when page is called, essentially a constructor.

        @param user: whether or not last username was invalid
        @param pass: whether or not last password is invalid
    */
    public void OnGet(bool user, bool pass) {
        userFail = user;
        passFail = pass;
    }

    // checks whether the hashed password and username match with those stored in SQL database
    public bool CheckSQL(string user, string hashPass) {
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
        builder.ConnectionString = "";

        using (SqlConnection connection = new SqlConnection(builder.ConnectionString)) {
            connection.Open();

            // need to add logic to prevent sql injections
            String sql = "SELECT UserID, HashedPass FROM dbo.WHITELIST WHERE UserID='" + user + "'";

            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        if (hashPass == reader.GetString(1)) return true;
                    }
                }
            }
        }

        return false;
    }

    /*
        What happens when a POST request on the page is made, i.e. form submission.
    */
    public ActionResult OnPost() {

        // regex pattern for usernames
        Regex userRegex = new Regex("^[a-zA-Z0-9]+$");

        // get username from form and validate it
        username = Request.Form["username"];
        bool userFailed = userRegex.IsMatch(username);

        // password regex pattern
        Regex passRegex = new Regex("^[a-zA-Z0-9~`!@#$%^&*?]+$");

        // get password from request
        password = Request.Form["password"];
        bool passFailed = passRegex.IsMatch(password);

        // if either paterns dont match get return back to login page with errors
        if (!userFailed || !passFailed) {
            return RedirectToPage("./Login", new {user = !userFailed, pass = !passFailed});
        }
        
        // has the password and compare to value in SQL database
        string hashedPass = this.hashPass();
        if (CheckSQL(username, hashedPass)) return RedirectToPage("./Login_Home", new {user = username});
        else return RedirectToPage("./Login", new {user = !userFailed, pass = true});

    }
}