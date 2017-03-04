
namespace CompressorService.Models
{
    public class Image
    {
        public int imageId { get; set; }
        public string imageURL { get; set; }

        override public string ToString()
        {
            return imageId + "," + imageURL;
        }
        
    }
}