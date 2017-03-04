using System;
using System.Web.Http;
using CompressorService.Contracts;
using ImageCompressor;
using CompressorService.Models;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using CompressorService.Enums;
using System.Web;

namespace CompressorService.Controllers
{
    public class CompressorController : ApiController
    {
        private string baseURL = HttpContext.Current.Request.ServerVariables["HTTP_HOST"];

        private int compressImageQuality = Int16.Parse(ConfigurationManager.AppSettings["defaultCompressionQuality"]);
        private static string compressedImageRelativePath = ConfigurationManager.AppSettings["compressedImageRelativePath"];
        private static string csvResultFileRelativePath = ConfigurationManager.AppSettings["csvResultFileRelativePath"];
        private static string csvDownloadFileRelativePath = ConfigurationManager.AppSettings["csvDownloadFileRelativePath"];
        private string compressedImageFilePath;
        private string csvResultFilePath;
        private string csvDownloadFilePath;


        public CompressorController()
        {
            compressedImageFilePath = HttpRuntime.AppDomainAppPath + compressedImageRelativePath;
            csvResultFilePath = HttpRuntime.AppDomainAppPath + csvResultFileRelativePath;
            csvDownloadFilePath = HttpRuntime.AppDomainAppPath + csvDownloadFileRelativePath;
            
            new FileInfo(compressedImageFilePath).Directory.Create();
            new FileInfo(csvResultFilePath).Directory.Create();
            new FileInfo(csvDownloadFilePath).Directory.Create();
        }

        public CompressorController(string defaultTestPath)
        {
            compressedImageFilePath = defaultTestPath + compressedImageRelativePath;
            csvResultFilePath = defaultTestPath + csvResultFileRelativePath;
            csvDownloadFilePath = defaultTestPath + csvDownloadFileRelativePath;
        }

        /// <summary>
        /// Function to validate URL.
        /// Currently supported schemes Http and Https
        /// </summary>
        /// <param name="imageURL">URL to be validated.</param>
        /// <returns></returns>
        private bool validateURL(string URL)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(URL, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            return result;
        }

        /// <summary>
        /// Function to validate extension.
        /// Refer to Enums folder for supported extensions.
        /// </summary>
        /// <param name="URL">URL to be validated.</param>
        /// <param name="enumType">Supported Enum type.</param>
        /// <returns></returns>
        private bool validateExtension(string URL, Type enumType)
        {
            string extension = Path.GetExtension(URL);
            if (!extension.Equals(String.Empty) && Enum.IsDefined(enumType, extension.Remove(0, 1)))
                return true;
            return false;
        }

        /// <summary>
        /// Service welcome message.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult HelloCompressor()
        {
            return Ok("Welcome to Compressor Service.");
        }

        /// <summary>
        /// Function which validates URL and Extensions.
        /// If validated compresses the image using ImageCompressor Library.
        /// We can have single instance of Image Compressor.
        /// </summary>
        /// <param name="image">Image to be compressed.</param>
        /// <returns>Compressed Image URL.</returns>
        private string Compress(Image image)
        {
            string compressedImageURL = "";
            if (validateURL(image.imageURL) &&
                validateExtension(image.imageURL,typeof(imageFormat)))
            {
                string extension = Path.GetExtension(image.imageURL);
                try
                {
                    ImageCompressor.ImageCompressor img = new ImageCompressor.ImageCompressor();
                    string fileName = img.CompressImage(image.imageURL, extension.Remove(0, 1),compressImageQuality,compressedImageFilePath);
                    compressedImageURL = baseURL + "\\" + compressedImageRelativePath + fileName;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            else
            {
                throw new ImageCompressorException("Please provide valid URL or Extension.");
            }
            return compressedImageURL;
        }

        /// <summary>
        /// Function exposed through API.
        /// Get an image from the request body.
        /// Uses compress function for compressing image.
        /// </summary>
        /// <param name="image">Image to be compressed.</param>
        /// <param name="quality">Image compression quality.</param>
        /// <param name="output">Response output.</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult CompressImage([FromBody]Image image,int? quality = null,string output = null)
        {
            try
            {
                compressImageQuality = quality != null ? quality.Value : compressImageQuality;
                string compressedImageURL = Compress(image);
                Image compressedImage = new Image { imageId = image.imageId, imageURL = compressedImageURL };
                IHttpActionResult response = GenerateOutput(compressedImage,output);
                return response;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Function exposed through API.
        /// Get list of images from the request body.
        /// Uses compress function for compressing list of images.
        /// </summary>
        /// <param name="images">List of images to be compressed.</param>
        /// <param name="quality">Image compression quality.</param>
        /// <param name="output">Response output.</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult CompressImages([FromBody]List<Image> images, int? quality = null, string output = null)
        {
            if (images!=null && images.Count > 0)
            {
                compressImageQuality = quality != null ? quality.Value : compressImageQuality;
                List<Image> compressedImages = new List<Image>();

                for (int i = 0; i < images.Count; i++)
                {

                    string compressedImageURL = "";
                    try
                    {
                        compressedImageURL = Compress(images[i]);
                    }
                    catch(Exception ex)
                    {
                        compressedImageURL = ex.Message;
                    }

                    compressedImages.Add(new Image { imageId = images[i].imageId, imageURL = compressedImageURL });

                }
                return GenerateOutput(compressedImages,output);
            }
            else
            {
                return BadRequest("No image to compress in the list.");
            }
            
        }


        /// <summary>
        /// Function exposed through API.
        /// Get fileURL from the request body.
        /// Validates if file type is supported.
        /// Uses file parser for parsing list of images from the file.
        /// Uses compress function for compressing list of images.
        /// </summary>
        /// <param name="fileURL">URL of the file.</param>
        /// <param name="quality">Image compression quality.</param>
        /// <param name="output">Response output.</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult CompressImageFromFile([FromBody]string fileURL, int? quality = null, string output = null)
        {
            if (fileURL!=null && validateExtension(fileURL, typeof(fileType)))
            {
                try
                {
                    compressImageQuality = quality != null ? quality.Value : compressImageQuality;
                    List<Image> images = FileOperations.ParseFile(fileURL);
                    return CompressImages(images);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return BadRequest("File format not supported.");
            }
        }

        /// <summary>
        /// Function for compressing raw image.
        /// </summary>
        /// <param name="rawImage">Raw image byte array.</param>
        /// <returns></returns>
        private string CompressImage(byte[] rawImage)
        {
            ImageCompressor.ImageCompressor compressor = new ImageCompressor.ImageCompressor();
            string compressedImageURL = compressor.CompressImage(rawImage,compressImageQuality, compressedImageFilePath);
            return compressedImageURL;
        }

        /// <summary>
        /// Function exposed through API.
        /// Function for compressing raw image.
        /// </summary>
        /// <param name="rawImage">Raw image byte array.</param>
        /// <param name="quality">Image compression quality.</param>
        /// <param name="output">Response output.</param>
        /// <returns></returns>
        public IHttpActionResult CompressRawImage([FromBody]byte[] rawImage, int? quality = null, string output = null)
        {
            try
            {
                compressImageQuality = quality != null ? quality.Value : compressImageQuality;
                string fileName = CompressImage(rawImage);
                string compressedImageURL = baseURL + "\\" + compressedImageRelativePath + fileName;
                Image compressedImage = new Image { imageId = 1, imageURL = compressedImageURL };
                return GenerateOutput(compressedImage,output);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        
        }

        /// <summary>
        /// Function exposed through API.
        /// Function for compressing list of raw images.
        /// </summary>
        /// <param name="rawImages">List of raw image byte array.</param>
        /// <param name="quality">Image compression quality.</param>
        /// <param name="output">Response output.</param>
        /// <returns></returns>
        public IHttpActionResult CompressRawImages([FromBody]List<byte[]> rawImages, int? quality = null, string output = null)
        {
            if (rawImages != null && rawImages.Count > 0)
            {
                compressImageQuality = quality != null ? quality.Value : compressImageQuality;
                List<Image> compressedImages = new List<Image>();

                for (int i = 0; i < rawImages.Count; i++)
                {

                    string compressedImageURL = "";
                    try
                    {
                        compressedImageURL = CompressImage(rawImages[i]);
                    }
                    catch (Exception ex)
                    {
                        compressedImageURL = ex.Message;
                    }

                    compressedImages.Add(new Image { imageId = i+1, imageURL = compressedImageURL });

                }
                return GenerateOutput(compressedImages,output);
            }
            else
            {
                return BadRequest("No image to compress in the list.");
            }

        }

        /// <summary>
        /// Function exposed through API.
        /// Function for compressing images in raw csv.
        /// </summary>
        /// <param name="rawImage">Raw csv byte array.</param>
        /// <param name="quality">Image compression quality.</param>
        /// <param name="output">Response output.</param>
        /// <returns></returns>
        public IHttpActionResult CompressRawCSV([FromBody]byte[] rawCSV, int? quality = null, string output = null)
        {
            try
            {
                compressImageQuality = quality != null ? quality.Value : compressImageQuality;
                List<Image> compressedImages = FileOperations.ParseRawCSV(rawCSV, csvDownloadFilePath);
                return CompressImages(compressedImages);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        /// <summary>
        /// Function for generating output based on output parameter.
        /// </summary>
        /// <param name="image">Compressed image or list of compressed images.</param>
        /// <param name="output">Response output.</param>
        /// <returns></returns>
        private IHttpActionResult GenerateOutput(object image, string output)
        {
            if (output == null)
            {
                if (image is Image)
                    return Ok(image as Image);
                else
                    return Ok(image as List<Image>);
            }
            else if (output.Equals("csv", StringComparison.InvariantCultureIgnoreCase))
            {
                string csvFileName = FileOperations.createCSVFile(image, csvResultFilePath);
                return Ok(baseURL + "\\" + csvResultFileRelativePath + csvFileName);
            }
            else
                return BadRequest("Output is not supported");
        }
    }
}
