using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CompressorService.Controllers;
using CompressorService.Models;
using ImageCompressor;
using System.Web.Http.Results;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Configuration;

namespace CompressorService.Tests
{
    [TestClass]
    public class CompressorServiceTests
    {
        private string ImageURL_JPEG;
        private string ImageURL_JPG;
        private string ImageURL_PNG;
        private string projectDirectory;
        private string testCSVFilePath;
        private string negativeTestCSVFilePath;
        private string compressedImageRelativePath = @"ServiceFiles\CompressedImages\";
        private string csvResultFileRelativePath = @"ServiceFiles\CSVFiles\Results\";
        private string csvDownloadFileRelativePath = @"ServiceFiles\CSVFiles\Downloads\";
        private string defaultCompressionQuality = "90";
        private string baseURL;
        CompressorController compressor;

        [TestInitialize]
        public void TestInitialize()
        {
            HttpContext.Current = new HttpContext(
                                  new HttpRequest("", "http://tempuri.org", ""),
                                  new HttpResponse(new StringWriter())
                                  );
            ImageURL_JPEG = "https://upload.wikimedia.org/wikipedia/commons/8/86/Cactus_Flower_(Easy-Macro).jpeg";
            ImageURL_JPG = "http://www.uaex.edu/environment-nature/water/quality/images/clearrunningstreamSmall.jpg";
            ImageURL_PNG = "http://vignette4.wikia.nocookie.net/pokemon/images/7/75/Pikachu_(Pokk%C3%A9n_Tournament).png";

            projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\";
            testCSVFilePath = projectDirectory + @"\TestData\Images.csv";
            negativeTestCSVFilePath = projectDirectory + @"\TestData\Ima.csv";
            ConfigurationManager.AppSettings["compressedImageRelativePath"] = compressedImageRelativePath;
            ConfigurationManager.AppSettings["csvResultFileRelativePath"] = csvResultFileRelativePath;
            ConfigurationManager.AppSettings["csvDownloadFileRelativePath"] = csvDownloadFileRelativePath; 
            ConfigurationManager.AppSettings["defaultCompressionQuality"] = defaultCompressionQuality;
            new FileInfo(projectDirectory + compressedImageRelativePath).Directory.Create();
            new FileInfo(projectDirectory + csvResultFileRelativePath).Directory.Create();
            new FileInfo(projectDirectory + csvDownloadFileRelativePath).Directory.Create();
            baseURL = HttpContext.Current.Request.ServerVariables["HTTP_HOST"];
            compressor = new CompressorController(projectDirectory);

        }
        
        [TestMethod]
        public void Test_ValidImageURL_CompressImage()
        {
      
            string imageURL = "htt://vignette4.wikia.nocookie.net/pokemon/images/7/75/Pikachu_(Pokk%C3%A9n_Tournament)";
            
            Image image = new Image { imageId = 1, imageURL = imageURL };
            var result = compressor.CompressImage(image);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void Test_ValidImageExtension_CompressImage()
        {
            string imageURL = "http://vignette4.wikia.nocookie.net/pokemon/images/7/75/Pikachu_(Pokk%C3%A9n_Tournament)";
            
            Image image = new Image { imageId = 1, imageURL = imageURL };
            var result = compressor.CompressImage(image);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void Test_UndefinedImageCompressImage()
        {
            var result = compressor.CompressImage(null);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void Test_JpgCompressImage()
        {
            string imageURL = ImageURL_JPG;
            string fileName = Path.GetFileName(imageURL);

            string compressImageURL = baseURL + "\\" + compressedImageRelativePath + fileName;

            Image image = new Image { imageId = 1, imageURL = imageURL };
            var result = compressor.CompressImage(image) as OkNegotiatedContentResult<Image>;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content.imageURL, compressImageURL);

        }

        [TestMethod]
        public void Test_JpegCompressImage()
        {
            string imageURL = ImageURL_JPEG;
            string fileName = Path.GetFileName(imageURL);

            string compressImageURL = baseURL + "\\" + compressedImageRelativePath + fileName;

            Image image = new Image { imageId = 1, imageURL = imageURL };
            var result = compressor.CompressImage(image) as OkNegotiatedContentResult<Image>;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content.imageURL, compressImageURL);

        }

        [TestMethod]
        public void Test_PngCompressImage()
        {
            string imageURL = ImageURL_PNG;
            string fileName = Path.GetFileName(imageURL);

            string compressImageURL = baseURL + "\\" + compressedImageRelativePath + fileName;

            Image image = new Image { imageId = 1, imageURL = imageURL };
            var result = compressor.CompressImage(image) as OkNegotiatedContentResult<Image>;
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content.imageURL, compressImageURL);

            //Testing with output csv
            var result1 = compressor.CompressImage(image,null,"csv") as OkNegotiatedContentResult<string>;
            Assert.IsNotNull(result1);

        }

        [TestMethod]
        public void Test_EmptyImageList_CompressImages()
        {
            var result = compressor.CompressImages(null);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));


            result = compressor.CompressImages(new List<Image>());
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void Test_MultipleImages_CompressImage()
        {
            List<Image> images = new List<Image>();

            string imageURL = ImageURL_PNG;
            images.Add(new Image { imageId = 1, imageURL = imageURL });

            imageURL = ImageURL_JPEG;
            images.Add(new Image { imageId = 2, imageURL = imageURL });

            imageURL = "Invalid URL.";
            images.Add(new Image { imageId = 2, imageURL = imageURL });
            
            var result = compressor.CompressImages(images) as OkNegotiatedContentResult<List<Image>>;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content.Count, images.Count);

        }

        [TestMethod]
        public void Test_CompressImageFromFile()
        {
            var result = compressor.CompressImageFromFile(negativeTestCSVFilePath);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));

            var result1 = compressor.CompressImageFromFile(testCSVFilePath) as OkNegotiatedContentResult<List<Image>>;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Content.Count, 2);
        }

        [TestMethod]
        public void Test_CompressRawImage()
        {
            WebClient wc = new WebClient();
            byte[] rawImage = wc.DownloadData(ImageURL_JPG);
            var result = compressor.CompressRawImage(rawImage) as OkNegotiatedContentResult<Image>;

            Assert.IsNotNull(result);
            
            Assert.AreEqual(result.Content.imageId, 1);
        }

        [TestMethod]
        public void Test_CompressRawImages()
        {
            WebClient wc = new WebClient();
            List<byte[]> rawImages = new List<byte[]>();
            rawImages.Add(wc.DownloadData(ImageURL_JPG));
            rawImages.Add(wc.DownloadData(ImageURL_PNG));
            var result = compressor.CompressRawImages(rawImages) as OkNegotiatedContentResult<List<Image>>;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content.Count, 2);
        }

    }
}
