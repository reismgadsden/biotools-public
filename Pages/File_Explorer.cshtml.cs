using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;

namespace dotnetcoresample.Pages;

public class File_ExplorerModel : PageModel {
    public string? username { get; set; }

    public BlobServiceClient? bsc { get; set; }

    private string conn_str { get; set; } = "";

    public List<BlobContainerItem> bcis { get; set; } = new List<BlobContainerItem>();

    public void OnGet(string user) {
        username = user;
        bsc = new BlobServiceClient(conn_str);
        ListContainers(bsc, "");
    }

    public void ListContainers (BlobServiceClient blobsc, string prefix) {
        try {
            Azure.Pageable<BlobContainerItem> result = blobsc.GetBlobContainers(BlobContainerTraits.Metadata, prefix, default);

            foreach (BlobContainerItem containerPage in result) {
                //containerPage.ListBlob
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

    public string ContainerSize(string cont_ref) {
        float totalSize = 0;
        BlobContainerClient container = new BlobContainerClient(conn_str, cont_ref);

        foreach (BlobItem blob in container.GetBlobs()) {
            totalSize += blob.Properties.ContentLength.GetValueOrDefault();
        }
        
        return PrettySize(totalSize);
    }

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