using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using System.Collections.Generic;
using Renci.SshNet;
using System.Threading;
using Microsoft.Data.SqlClient;

public class TrinityModel : PageModel {

    // holds username
    public string? username { get; set; }
    
    //holds the current error string
    public string? err { get; set; }

    /*
        What happens when page is loaded, essentially a constructor

        @param user: current username
        @param e: current error string
    */
    public void OnGet(string user, string e) {
        username = user;
        if (e != null && e != "") {
            err = e;
        }
    }

    /*
        Gets the email of the current user so it can be passed in to VM powershell as parameter
    */
    public string EmailFromSQL(string user) {
        SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
        builder.ConnectionString = "";

        string email = "";

        using (SqlConnection connection = new SqlConnection(builder.ConnectionString)) {
            connection.Open();
        
            // need to add logic to prevent sql injections
            String sql = "SELECT UserID, Email FROM dbo.WHITELIST WHERE UserID='" + user + "'";

            using (SqlCommand command = new SqlCommand(sql, connection)) {
                using (SqlDataReader reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        email = reader.GetString(1);
                    }
                }
            }
        }

            return email;
    }

    /*
        Connects to our VM and runs the powershell script with the input params
    */
    public void RunTrinty(string seqType, string left, string right, string output, string user, string email){

        // command to run on vm
        string command = "pwsh -command './RunTrinity.ps1 -seqType \"." + seqType + "\" -Left " + left + " -Right " + right + " -Output \"" + output +"\" -User \"" + user + "\" -Email \"" + email + "\"'";
        // using (StreamWriter sw = new StreamWriter("./log.txt", true)) {
        //     sw.WriteLine(command);
        // }

        // Connect to VM via SSH
        using (var client = new SshClient("", "", "")) {
            client.Connect();
            var cmd = client.CreateCommand(command);
            var res = cmd.Execute();

            // can be included to debug shell
            // using (StreamWriter sw = new StreamWriter("./log.txt", true)) {
            //     sw.WriteLine(res);
            //     var reader = new StreamReader(cmd.ExtendedOutputStream);
            //     sw.WriteLine("Debug:");
            //     sw.WriteLine(reader.ReadToEnd());
            // }               

            client.Disconnect();
        }
    }

    /*
        What happens on POST request i.e. form submission
    */
    public ActionResult OnPost(){

        // get all values from the form
        var leftReads = Request.Form["left[]"];
        var rightReads = Request.Form["right[]"];
        var radio = Request.Form["seq_type"];
        var usern = Request.Form["user"];
        var out_dir = Request.Form["output"];

        // using (StreamWriter sw = new StreamWriter("./log.txt", true)) {
        //     sw.WriteLine(username);
        // }

        // validates that a sequence type is selected
        if (radio != "fq" && radio != "fa") return RedirectToPage("./Trinity", new {user = usern, e = "No file type was selected!"});

        // checks to make sure a file names are valid
        for (int i = 0; i < leftReads.Count; i++) {
            if (leftReads[i] == "" || rightReads[i] == "") return RedirectToPage("./Trinity", new {user = usern, e = "You must provide an equal number of left and right reads!"});
            if (!leftReads[i].Contains("." + radio) || !rightReads[i].Contains("." + radio)) return RedirectToPage("./Trinity", new {user = usern, e = "You selected the wrong file type!"});
        }

        // concats array of files into one string
        string l_reads = string.Join(", ", leftReads.Select(x => string.Format("\"{0}\"", x)));
        string r_reads = string.Join(", ", rightReads.Select(x => string.Format("\"{0}\"", x)));

        // gets user email
        string email = EmailFromSQL(usern);

        // start the trinity run in a new thread so that it continues to execute even after leaving page
        Thread t = new Thread(() => RunTrinty(radio, l_reads, r_reads, out_dir, usern, email));
        t.Start();

        // redirects back to form page with err *, this prints a success message
        return RedirectToPage("./Trinity", new {user = usern, e = "*"});
    }
}