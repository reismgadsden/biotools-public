using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace dotnetcoresample.Pages;

public class File_ExplorerModel : PageModel {

    // holds the current user name
    public string? username { get; set; }

    // holds the current Azure storage API object
    public BlobServiceClient? bsc { get; set; }

    // connection string to the stroage account
    private string conn_str { get; set; } = "";

    // holds containers in storage account
    public List<BlobContainerItem> bcis { get; set; } = new List<BlobContainerItem>();

    /*
        What happens when the page is called, essentially a constructor

        @param user: the current user
    */
    public void OnGet(string user) {
        username = user;
        bsc = new BlobServiceClient(conn_str);
        ListContainers(bsc, "");
    }

    /*
        Gets a list of all containers within a storage account
    */
    public void ListContainers (BlobServiceClient blobsc, string prefix) {
        try {
            Azure.Pageable<BlobContainerItem> result = blobsc.GetBlobContainers(BlobContainerTraits.Metadata, prefix, default);

            foreach (BlobContainerItem containerPage in result) {
                bcis.Add(containerPage);
            }
        }
        catch (Azure.RequestFailedException e) {
            using (StreamWriter writer = new StreamWriter("./log.txt")) {
                writer.WriteLine("Exception Occured in ListContainers " + e.ToString());
            }
            return;
        }
    }

    /*
        Computes the total size of a container

        @param cont_ref: the name of the container to calculate for
    */
    public string ContainerSize(string cont_ref) {
        float totalSize = 0;
        BlobContainerClient container = new BlobContainerClient(conn_str, cont_ref);

        foreach (BlobItem blob in container.GetBlobs()) {
            totalSize += blob.Properties.ContentLength.GetValueOrDefault();
        }
        
        // call to pretty formatting
        return PrettySize(totalSize);
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

        if (i == 0) return totalSize.ToString() + " Bytes";
        else if (i == 1) return totalSize.ToString("0.00") + " KB";
        else if (i == 2) return totalSize.ToString("0.00") + " MB";
        else if (i == 3) return totalSize.ToString("0.00") + " GB";
        else return "This cant handle files of this size, tf?";
    }
}