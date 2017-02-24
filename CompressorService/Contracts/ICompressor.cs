using System.Web.Http;

namespace CompressorService.Contracts
{
    interface ICompressor
    {
        IHttpActionResult CompressImage(string imageURL);
       
    }
}
