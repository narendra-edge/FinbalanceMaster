namespace FbEmailService.Api.Models
{
    public class EmailAttachments
    {
        public IFormFile ImageFile { get; set; }
        public string ToEmail { get; set; }
    }
}
