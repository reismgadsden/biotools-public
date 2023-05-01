using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;
using System.Collections.Generic;
using Renci.SshNet;
using System.Threading;
using Microsoft.Data.SqlClient;

public class TrinityModel : PageModel {

    public string? username { get; set; }
    
    public string? err { get; set; }

    public void OnGet(string user, string e) {
        username = user;
        if (e != null && e != "") {
            err = e;
        }
    }

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

    public void RunTrinty(string seqType, string left, string right, string output, string user, string email){
        string command = "pwsh -command './RunTrinity.ps1 -seqType \"." + seqType + "\" -Left " + left + " -Right " + right + " -Output \"" + output +"\" -User \"" + user + "\" -Email \"" + email + "\"'";
        // using (StreamWriter sw = new StreamWriter("./log.txt", true)) {
        //     sw.WriteLine(command);
        // }

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

    public ActionResult OnPost(){
        var leftReads = Request.Form["left[]"];
        var rightReads = Request.Form["right[]"];
        var radio = Request.Form["seq_type"];
        var usern = Request.Form["user"];
        var out_dir = Request.Form["output"];

        // using (StreamWriter sw = new StreamWriter("./log.txt", true)) {
        //     sw.WriteLine(username);
        // }

        if (radio != "fq" && radio != "fa") return RedirectToPage("./Trinity", new {user = usern, e = "No file type was selected!"});

        for (int i = 0; i < leftReads.Count; i++) {
            if (leftReads[i] == "" || rightReads[i] == "") return RedirectToPage("./Trinity", new {user = usern, e = "You must provide an equal number of left and right reads!"});
            if (!leftReads[i].Contains("." + radio) || !rightReads[i].Contains("." + radio)) return RedirectToPage("./Trinity", new {user = usern, e = "You selected the wrong file type!"});
        }

        string l_reads = string.Join(", ", leftReads.Select(x => string.Format("\"{0}\"", x)));
        string r_reads = string.Join(", ", rightReads.Select(x => string.Format("\"{0}\"", x)));

        string email = EmailFromSQL(usern);

        // Thread t = new Thread(() => RunTrinty(radio, l_reads, r_reads, out_dir, usern, email));
        // t.Start();

        // RunTrinty(radio, l_reads, r_reads, usern, email);

        return RedirectToPage("./Trinity", new {user = usern, e = "*"});
    }
}