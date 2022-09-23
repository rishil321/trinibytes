using trinibytes.Models;
using trinibytes.Scrapers;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Net;
using System.Xml.Linq;
using Serilog;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace trinibytes.Controllers
{
    [ApiController]
    [Route("BlogPosts")]
    public class BlogPostController : ControllerBase
    {
        private readonly MyDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public BlogPostController(MyDbContext context, IConfiguration configuration,
            BackgroundTasks.ICustomServiceStopper stopper, BackgroundTasks.ICustomServiceStarter starter)
        {
            _dbContext = context;
            _configuration = configuration;
        }

        [Route("UploadNewImage")]
        [HttpPost]
        public async Task<ActionResult<UploadedImage>> UploadNewImage(UploadedImage? uploadedImage)
        {
            try
            {
                if (uploadedImage == null)
                    return BadRequest();

                _dbContext.UploadedImages?.Add(uploadedImage);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetUploadedImage),
                    new {id = uploadedImage.Id},
                    uploadedImage);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new blog post");
            }
        }

        [Route("GetUploadedImage/{id:int}")]
        [HttpGet]
        public async Task<ActionResult<UploadedImage>> GetUploadedImage(int id)
        {
            if (_dbContext.UploadedImages != null)
            {
                var uploadedImage = await _dbContext.UploadedImages.FindAsync(id);

                if (uploadedImage == null)
                {
                    return NotFound();
                }

                return uploadedImage;
            }
            else
            {
                return NotFound();
            }
        }

        [Route("CreateNewBlogPost")]
        [HttpPost]
        public async Task<ActionResult<BlogPost>> CreateNewBlogPost(BlogPost? blogPost)
        {
            try
            {
                if (blogPost == null)
                    return BadRequest();

                _dbContext.BlogPosts?.Add(blogPost);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(
                    nameof(GetBlogPost),
                    new {id = blogPost.Id},
                    blogPost);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Error creating new blog post");
            }
        }


        [Route("GetBlogPost/{id:int}")]
        [HttpGet]
        public async Task<ActionResult<BlogPost>> GetBlogPost(int id)
        {
            if (_dbContext.BlogPosts != null)
            {
                var blogPost = await _dbContext.BlogPosts.FindAsync(id);

                if (blogPost == null)
                {
                    return NotFound();
                }

                return blogPost;
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("GetLatestBlogPosts")]
        public async Task<ActionResult<List<BlogPost>>> GetLatestBlogPosts()
        {
            if (_dbContext.BlogPosts != null)
            {
                var blogPosts = await _dbContext.BlogPosts.OrderByDescending(blogPost => blogPost.LastModifiedDate)
                    .Take(5)
                    .ToListAsync();

                if (blogPosts.Capacity == 0)
                {
                    return NotFound();
                }

                return blogPosts;
            }
            else
            {
                return NotFound();
            }
        }
    }
}