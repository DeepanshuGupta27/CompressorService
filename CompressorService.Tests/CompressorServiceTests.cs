using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CompressorService.Controllers;
using CompressorService.Models;
using ImageCompressor;
using System.Web.Http.Results;
using System.IO;
using System.Collections.Generic;

namespace CompressorService.Tests
{
    [TestClass]
    public class CompressorServiceTests
    {
        private string ImageURL_JPEG = "https://upload.wikimedia.org/wikipedia/commons/8/86/Cactus_Flower_(Easy-Macro).jpeg";
        private string ImageURL_JPG = "http://www.uaex.edu/environment-nature/water/quality/images/clearrunningstreamSmall.jpg";
        private string ImageURL_PNG = "http://vignette4.wikia.nocookie.net/pokemon/images/7/75/Pikachu_(Pokk%C3%A9n_Tournament).png";

        [TestMethod]
        public void Test_ValidImageURL_CompressImage()
        {
            CompressorController compressor = new CompressorController();
            string imageURL = "htt://vignette4.wikia.nocookie.net/pokemon/images/7/75/Pikachu_(Pokk%C3%A9n_Tournament)";
            
            Image image = new Image { imageId = 1, imageURL = imageURL };
            var result = compressor.CompressImage(image);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void Test_ValidImageExtension_CompressImage()
        {
            CompressorController compressor = new CompressorController();
            string imageURL = "http://vignette4.wikia.nocookie.net/pokemon/images/7/75/Pikachu_(Pokk%C3%A9n_Tournament)";
            
            Image image = new Image { imageId = 1, imageURL = imageURL };
            var result = compressor.CompressImage(image);

            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void Test_UndefinedImageCompressImage()
        {
            CompressorController compressor = new CompressorController();
            var result = compressor.CompressImage(null);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
        }

        [TestMethod]
        public void Test_JpgCompressImage()
        {
            CompressorController compressor = new CompressorController();
            string imageURL = ImageURL_JPG;
            string fileName = Path.GetFileName(imageURL);

            string compressImageURL = Constants.BASEPATH + fileName;

            Image image = new Image { imageId = 1, imageURL = imageURL };
            var result = compressor.CompressImage(image) as OkNegotiatedContentResult<Image>;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content.imageURL, compressImageURL);

        }

        [TestMethod]
        public void Test_JpegCompressImage()
        {
            CompressorController compressor = new CompressorController();
            string imageURL = ImageURL_JPEG;
            string fileName = Path.GetFileName(imageURL);

            string compressImageURL = Constants.BASEPATH + fileName;

            Image image = new Image { imageId = 1, imageURL = imageURL };
            var result = compressor.CompressImage(image) as OkNegotiatedContentResult<Image>;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content.imageURL, compressImageURL);

        }

        [TestMethod]
        public void Test_PngCompressImage()
        {
            CompressorController compressor = new CompressorController();
            string imageURL = ImageURL_PNG;
            string fileName = Path.GetFileName(imageURL);

            string compressImageURL = Constants.BASEPATH + fileName;

            Image image = new Image { imageId = 1, imageURL = imageURL };
            var result = compressor.CompressImage(image) as OkNegotiatedContentResult<Image>;

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content.imageURL, compressImageURL);

        }

        [TestMethod]
        public void Test_EmptyImageList_CompressImages()
        {
            CompressorController compressor = new CompressorController();

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
            CompressorController compressor = new CompressorController();
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
        public void Test_ValidFileExtension_CompressImageFromFile()
        {
            string invalidCsvFileName = @"C:\Users\gdeepanshu\Documents\Book2.cs";

            //this file contains 2 image url.
            string validFileName = @"C:\Users\gdeepanshu\Documents\Book2.csv";

            CompressorController compressor = new CompressorController();
            var result = compressor.CompressImageFromFile(invalidCsvFileName);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));

            var result1 = compressor.CompressImageFromFile(validFileName) as OkNegotiatedContentResult<List<Image>>;
            Assert.IsNotNull(result1);
            Assert.AreEqual(result1.Content.Count, 2);
        }

    }
}
