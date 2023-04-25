using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

public class File_Explorer_ContainerModel : PageModel {
    public string? username { get; set; }

    public string path {get; set;} = "";

    public string last_path {get; set;} = "~";

    public string container { get; set; } = "";

    private string conn_str { get; set; } = "";

    public BlobServiceClient? bsc { get; set; }

    public List<BlobItem> blobs { get; set; } = new List<BlobItem>();

    public void OnGet(string user, string rel_path, string l_path, string cont) {
        username = user;

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

        public void ListBlobs () {

        List<BlobItem> folders = new List<BlobItem>();
                
        string loc_path = path + "/";
        BlobContainerClient b_container = new BlobContainerClient(conn_str, this.container);

        foreach (BlobItem blob in b_container.GetBlobs()) {
            if (loc_path == "/") {
                if (blob.Name.Split("/").Length == 1) blobs.Add(blob);
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
            blobs.Sort((x1, x2) => x1.Name.Replace(path + "/", string.Empty).CompareTo(x2.Name.Replace(path + "/", string.Empty)));
        }
        for (int i = 0; i < folders.Count; i++) blobs.Insert(i, folders[i]);
    }

    public string FolderDate(string loc_path) {
        if (loc_path.Substring(0, 1) == "/") loc_path = loc_path.Substring(1);

        DateTimeOffset d_max = DateTimeOffset.UnixEpoch;
        BlobContainerClient b_container = new BlobContainerClient(conn_str, this.container);

        foreach(BlobItem b in b_container.GetBlobs()) {
            if (b.Name.Contains(loc_path) && b.Properties != null) {
                if (b.Properties.LastModified.GetValueOrDefault() > d_max) d_max = b.Properties.LastModified.GetValueOrDefault();
            }
        }

        if (d_max == DateTimeOffset.UnixEpoch) return "";
        else return d_max.ToString("g");
    }

    public string FolderSize(string loc_path) {
        if (loc_path.Substring(0, 1) == "/") loc_path = loc_path.Substring(1);

        float folderSize = 0;
        BlobContainerClient b_container = new BlobContainerClient(conn_str, this.container);

        foreach(BlobItem b in b_container.GetBlobs()) {
            if (b.Name.Contains(loc_path) && b.Properties != null) {
                folderSize += b.Properties.ContentLength.GetValueOrDefault();
            }
        }

        return PrettySize(folderSize);
    }

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