using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;

namespace ImageCompressor
{
    public class ImageCompressor
    {
        /// <summary>
        /// Saves the image on required path with required quality.
        /// </summary>
        /// <param name="path">Path where image need to be stored</param>
        /// <param name="img">Image to be stored</param>
        /// <param name="extension">Extension of the image</param>
        /// <param name="quality">Quality of the compressed image.</param>
        private void Save(string path, Image img, string extension, int quality)
        {
            if (quality < 0 || quality > 100)
                throw new ImageCompressorException("quality must be between 0 and 100.");

            string mimeType = "image/" + extension;

            // Encoder parameter for image quality 
            EncoderParameter qualityParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
            
            // Extension image codec 
            ImageCodecInfo extensionCodec = GetEncoderInfo(mimeType);
            EncoderParameters encoderParams = new EncoderParameters(1);
            encoderParams.Param[0] = qualityParam;

            try
            {
                img.Save(path, extensionCodec, encoderParams);
            }
            catch (Exception ex)
            {
                throw new ImageCompressorException("Exception while saving compressed image.", ex);
            }
        }

        /// <summary>
        /// Get the image encoders for required mimeType.
        /// </summary>
        /// <param name="mimeType">MimeType of image.</param>
        /// <returns></returns>
        private ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            //In case of jpg mimetype return jpeg encoder.
            if (mimeType.Equals("image/jpg"))
                mimeType = "image/jpeg";

            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec 
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];

            return null;
        }

        /// <summary>
        /// Creates image from byte array.
        /// </summary>
        /// <param name="rawImage">Raw image byte array.</param>
        /// <param name="fileName">File name as an output paramter.</param>
        /// <param name="extension">Image extension.</param>
        /// <returns></returns>
        private Image CreateImageFromRawBytes(byte[] rawImage,out string fileName,out string extension)
        {
            try
            {
                fileName = DateTime.Now.Ticks.ToString();
                Image image = Image.FromStream(new MemoryStream(rawImage));

                if (ImageFormat.Png.Equals(image.RawFormat))
                    extension = "png";
                else if (ImageFormat.Jpeg.Equals(image.RawFormat))
                    extension = "jpeg";
                else
                    throw new ImageCompressorException("Please provide valid Extension.");

                fileName = fileName + '.' + extension;
                return image;
            }
            catch (Exception ex)
            {
                throw new ImageCompressorException("Error while creating image from raw image data.");
            }
        }

        /// <summary>
        /// Downloads the image from url and creates an Image object for downloaded image.
        /// </summary>
        /// <param name="imageURL">URL from where image will be downloaded</param>
        /// <returns></returns>
        private Image CreateImageFromImageURL(string imageURL)
        {
            try
            {
                WebClient wc = new WebClient();
                byte[] bytes = wc.DownloadData(imageURL);
                MemoryStream ms = new MemoryStream(bytes);
                System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                return img;
            }
            catch (Exception ex)
            {
                throw new ImageCompressorException("Exception while downloading image from url.", ex);
            }
        }

        /// <summary>
        /// Given the image url and its extension this function will 
        /// download and stores the compressed image on the disk with given quality.
        /// </summary>
        /// <param name="imageURL">URL from where image will be downloaded.</param>
        /// <param name="extension">Extension of the image.</param>
        /// <param name="quality">Compression quality.</param>
        /// <param name="compressedImageFilePath">Compressed image file path.</param>
        /// <returns></returns>
        public string CompressImage(string imageURL, string extension,int quality,string compressedImageFilePath)
        {

            string fileName = Path.GetFileName(imageURL);
            compressedImageFilePath = compressedImageFilePath + fileName;
            Image img = CreateImageFromImageURL(imageURL);
            Save(compressedImageFilePath, img, extension, 90);
            return fileName;

        }

        /// <summary>
        /// Given the raw image this function will 
        /// creates and stores the compressed image on the disk with given quality.
        /// </summary>
        /// <param name="rawImage">Raw image byte array.</param>
        /// <param name="quality">Compression quality.</param>
        /// <param name="compressedImageFilePath">Compressed image file path.</param>
        /// <returns></returns>
        public string CompressImage(byte[] rawImage,int quality,string compressedImageFilePath)
        {
            string fileName;
            string extension;
            Image img = CreateImageFromRawBytes(rawImage,out fileName, out extension);
            compressedImageFilePath = compressedImageFilePath + fileName;
            Save(compressedImageFilePath, img, extension, quality);
            return fileName;
        }
    }
}
