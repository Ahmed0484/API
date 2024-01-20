using API.Data;
using API.Models.Domain;
using API.Repositories.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories.Implementation
{
    public class ImageRepository:IImageRepository
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContext;
        private readonly AppDbContext _context;

        public ImageRepository(IWebHostEnvironment env,
           IHttpContextAccessor httpContext,
           AppDbContext context)
        {
            _env = env;
            _httpContext = httpContext;
            _context = context;
        }

        public async Task<BlogImage> Upload(IFormFile file, BlogImage blogImage)
        {
            // 1- Upload the Image to API/Images
            var localPath = Path.Combine(_env.ContentRootPath, "Images", $"{blogImage.FileName}{blogImage.FileExtension}");
            using var stream = new FileStream(localPath, FileMode.Create);
            await file.CopyToAsync(stream);

            // 2-Update the database
            // https://codepulse.com/images/somefilename.jpg
            var httpRequest = _httpContext.HttpContext?.Request;
            var urlPath = $"{httpRequest?.Scheme}://{httpRequest?.Host}{httpRequest?.PathBase}/Images/{blogImage.FileName}{blogImage.FileExtension}";

            blogImage.Url = urlPath;

            await _context.BlogImages.AddAsync(blogImage);
            await _context.SaveChangesAsync();

            return blogImage;
        }
        public async Task<IEnumerable<BlogImage>> GetAll()
        {
            return await _context.BlogImages.ToListAsync();
        }
    }
}
