using System.ComponentModel.DataAnnotations.Schema;

namespace trinibytes.Models;

using Microsoft.EntityFrameworkCore;

public class UploadedImage
{
    public int Id { get; set; }
    public string RelativePath { get; set; }
    public string ImageName { get; set; }
    public DateTime CreationDate { get; set; }

    public UploadedImage()
    {
    }

    public UploadedImage(string relativePath, string imageName)
    {
        this.RelativePath = relativePath;
        this.ImageName = imageName;
        this.CreationDate = DateTime.Today;
    }
}