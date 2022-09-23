using System.ComponentModel.DataAnnotations.Schema;

namespace trinibytes.Models;

using Microsoft.EntityFrameworkCore;

public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public DateTime CreationDate { get; set; }

    public DateTime LastModifiedDate { get; set; }

    public BlogPost(string title, string body)
    {
        this.Title = title;
        this.Body = body;
        this.CreationDate = DateTime.Today;
        this.LastModifiedDate = DateTime.Today;
    }
}