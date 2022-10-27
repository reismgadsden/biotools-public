using static UserAuth;
using User;

public class LoginModel : PageModel {
    public bool hashMatch (string p, string c) {
        int hash = UserAuth.hash(s);

        
    }

    public LoginModel(string user, string pass) {
        User user = new User();
        
        StreamReader r = new StreamReader("Shared/whitelist.json");
        string json = r.ReadToEnd();
        JsonTextReader whitelist = new JsonTextReader(new StringReader(json));

        bool found = false;

        while (whitelist.Read()) {
            if (    )
        }
    }
}