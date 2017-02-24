using System;
using System.Web.Http;
using CompressorService.Contracts;
using ImageCompressor;
using CompressorService.Models;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using CompressorService.Enums;

namespace CompressorService.Controllers
{
    public class CompressorController : ApiController
    {
        /// <summary>
        /// Function to validate URL.
        /// Currently supported schemes Http and Https
        /// </summary>
        /// <param name="imageURL">URL to be validated</param>
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
        /// <param name="URL">URL to be validated</param>
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
        /// </summary>
        /// <param name="image">Image to be compressed</param>
        /// <returns>Compressed Image URL</returns>
        private string Compress(Image image)
        {
            string extension = Path.GetExtension(image.imageURL);
            string compressedImageURL = "";

            if (validateURL(image.imageURL) &&
                validateExtension(image.imageURL,typeof(imageFormat)))
            {
                try
                {
                    ImageCompressor.ImageCompressor img = new ImageCompressor.ImageCompressor();

                    //extension.Remove(0,1) removes . from the extension.
                    compressedImageURL = img.CompressImage(image.imageURL, extension.Remove(0, 1));
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
        /// <param name="image">Image to be compressed</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult CompressImage([FromBody]Image image)
        {
            try
            {
                string compressedImagePath = Compress(image);
                return Ok(new Image { imageId = 1, imageURL = compressedImagePath });
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
        /// <param name="images">List of images to be compressed</param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult CompressImages([FromBody]List<Image> images)
        {
            if (images!=null && images.Count > 0)
            {
                List<Image> compressedImages = new List<Image>();

                for (int i = 0; i < images.Count; i++)
                {

                    string compressedImagePath = "";
                    try
                    {
                        compressedImagePath = Compress(images[i]);
                    }
                    catch(Exception ex)
                    {
                        compressedImagePath = ex.Message;
                    }

                    compressedImages.Add(new Image { imageId = images[i].imageId, imageURL = compressedImagePath });

                }
                return Ok(compressedImages);
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
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult CompressImageFromFile([FromBody]string fileURL)
        {
            if (validateExtension(fileURL, typeof(fileType)))
            {
                try
                {
                    List<Image> images = FileParser.ParseFile(fileURL);
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


    }
}
