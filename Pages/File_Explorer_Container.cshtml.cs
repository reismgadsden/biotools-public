using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public class File_Explorer_ContainerModel : PageModel {

    // This holds our user name
    public string? username { get; set; }

    // this hold the current path that we are accessing
    // the empty value "" corresonds to the ~ of a container
    public string path {get; set;} = "";

    // this holds the previous path
    // ~ is equivalent to the root (/)
    public string last_path {get; set;} = "~";

    // holds the value of the current container we are accessing
    public string container { get; set; } = "";

    // connection string to storage account
    private string conn_str { get; set; } = "";

    // holds our Blob service account object
    // this is the API object for Azure storage account blobs
    public BlobServiceClient? bsc { get; set; }

    // this holds all our blob items that are within the current path
    public List<BlobItem> blobs { get; set; } = new List<BlobItem>();

    /*
        What happens when the page is loaded, essentially a constructor.

        @param user: current username
        @param rel_path: the current path
        @param l_path: the last path
        @param cont: the current container
    */
    public void OnGet(string user, string rel_path, string l_path, string cont) {
        username = user;

        // replace all ~ in path with / for ease in matching with blob names
        if (rel_path == "~") path = "";
        else {
            if (rel_path.Substring(0, 1) == "~") path = rel_path.Substring(1).Replace("~", "/");
            else path = rel_path.Replace("~", "/");
        }
        if (l_path == "~") last_path = "~";
        else {
            if (l_path.Substring(0, 1) == "~") last_path = l_path.Substring(1).Replace("~", "/");
            else last_path = l_path.Replace("~", "/");
        }
        container = cont;
        bsc = new BlobServiceClient(conn_str);

        ListBlobs();
    }


        /*
            Gathers a list of blobs within the current path. Interprets subdirectories by creating
            a fake blob with no values except its name.
        */
        public void ListBlobs () {

        List<BlobItem> folders = new List<BlobItem>();
                
        string loc_path = path + "/";
        BlobContainerClient b_container = new BlobContainerClient(conn_str, this.container);

        foreach (BlobItem blob in b_container.GetBlobs()) {
            // case: we are in the root
            if (loc_path == "/") {
                
                // add to path if not a subdirectory
                if (blob.Name.Split("/").Length == 1) blobs.Add(blob);

                // create fake item for subdirectories
                else {
                    var b_test = BlobsModelFactory.BlobItem(blob.Name.Split("/")[0]);
                    bool b_bool = false;
                    foreach(BlobItem b in folders) {
                        if (b_test.Name == b.Name) {
                            b_bool = true;
                            break;
                        }
                    }

                    if (!b_bool) folders.Add(b_test);
                }
            }

            // case: we are not in the root
            else {
                if (blob.Name.Replace(loc_path, string.Empty).Split("/").Length == 1 && blob.Name.Contains(loc_path)) blobs.Add(blob);
                else if (blob.Name.Contains(loc_path)) {
                    var b_test = BlobsModelFactory.BlobItem(blob.Name.Replace(loc_path, string.Empty).Split("/")[0]);
                    bool b_bool = false;
                    foreach(BlobItem b in folders) {
                        if (b_test.Name == b.Name) {
                            b_bool = true;
                            break;
                        }
                    }

                    if (!b_bool) folders.Add(b_test);
                }
            }

            // sort or blobs alphabetically, so they render in order
            blobs.Sort((x1, x2) => x1.Name.Replace(path + "/", string.Empty).CompareTo(x2.Name.Replace(path + "/", string.Empty)));
        }

        // append all fake subdirectory objects to original list
        for (int i = 0; i < folders.Count; i++) blobs.Insert(i, folders[i]);
    }

    /*
        Returns a pretty date formatting for a folder. Gets the most recent date of all blobs in that subdirecotry.

        @params loc_path current path
    */
    public string FolderDate(string loc_path) {

        if (loc_path.Substring(0, 1) == "/") loc_path = loc_path.Substring(1);

        // set d_max inital to be the unix epoch
        DateTimeOffset d_max = DateTimeOffset.UnixEpoch;
        BlobContainerClient b_container = new BlobContainerClient(conn_str, this.container);

        // iterate over blobs and set the latest date to d_max
        foreach(BlobItem b in b_container.GetBlobs()) {
            if (b.Name.Contains(loc_path) && b.Properties != null) {
                if (b.Properties.LastModified.GetValueOrDefault() > d_max) d_max = b.Properties.LastModified.GetValueOrDefault();
            }
        }

        // returns empty string if there are no files in the directory
        if (d_max == DateTimeOffset.UnixEpoch) return "";
        else return d_max.ToString("g");
    }

    /*
        Calculates the total size of all blobs within a specified direcotory
    */
    public string FolderSize(string loc_path) {
        if (loc_path.Substring(0, 1) == "/") loc_path = loc_path.Substring(1);

        float folderSize = 0;
        BlobContainerClient b_container = new BlobContainerClient(conn_str, this.container);

        foreach(BlobItem b in b_container.GetBlobs()) {
            if (b.Name.Contains(loc_path) && b.Properties != null) {
                folderSize += b.Properties.ContentLength.GetValueOrDefault();
            }
        }

        // call to a format opotion for size
        return PrettySize(folderSize);
    }

    /*
        Prints a better formatted size value

        @param totalSize: total size in bytes
    */
    public string PrettySize(float totalSize) {
        int i = 0;
        while (totalSize > 1024) {
            totalSize /= 1024;
            i += 1;
        }

        if (i == 0) {
            if (totalSize == 0) return "";
            return totalSize.ToString() + " Bytes";
        } 
        else if (i == 1) return totalSize.ToString("0.00") + " KB";
        else if (i == 2) return totalSize.ToString("0.00") + " MB";
        else if (i == 3) return totalSize.ToString("0.00") + " GB";
        else return "This cant handle files of this size, tf?";
    }
}