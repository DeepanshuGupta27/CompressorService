using CompressorService.Models;
using LinqToExcel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CompressorService.Enums;
using System;
using System.Text;

namespace CompressorService
{
    public static class FileOperations
    {
        /// <summary>
        /// Decides which parser to use based on file extension and returns list of images.
        /// </summary>
        /// <param name="fileURL">URL of the file.Presently using local system path.</param>
        /// <returns></returns>
        public static List<Image> ParseFile(string fileURL)
        {
            string extension = Path.GetExtension(fileURL);
            extension = extension.Remove(0, 1);
            fileType result;

            if (Enum.TryParse<fileType>(extension, out result) && result == fileType.csv)
                return ParseCSV(fileURL);
            else
                return ParseXLS(fileURL);

        }

        /// <summary>
        /// Parses csv file and populates list of image objects.
        /// Below is the expected format of csv file.
        /// ImageId,ImageURL
        /// 1,http://imageURL.com/
        /// </summary>
        /// <param name="fileURL">URL of the file.Presently using local system path.</param>
        /// <returns></returns>
        private static List<Image> ParseCSV(string fileURL)
        {
            char[] delimiters = new char[] { ',' };
            string[] lines = File.ReadAllLines(fileURL);
            List<Image> images = new List<Image>();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] imageDetails = lines[i].Split(delimiters);
                images.Add(new Image { imageId = int.Parse(imageDetails[0]), imageURL = imageDetails[1] });
            }

            return images;
        }

        /// <summary>
        /// Parses xlsx file and populates list of image objects.
        /// Below is the expected format of xlsx file.
        /// ImageId,ImageURL
        /// 1,http://imageURL.com/
        /// </summary>
        /// <param name="fileURL">URL of the file.Presently using local system path.</param>
        /// <returns></returns>
        private static List<Image> ParseXLS(string fileURL)
        {
            List<Image> images = new List<Image>();

            var excel = new ExcelQueryFactory(fileURL);
            excel.AddMapping<Image>(x => x.imageId, "ImageId"); //maps the "State" property to the "Providence" column
            excel.AddMapping<Image>(x => x.imageURL, "ImageURL");
           
            images.AddRange(from c in excel.Worksheet<Image>()
                            select new Image { imageId = c.imageId, imageURL = c.imageURL });

            return images;
        }

        /// <summary>
        /// Creates csv file.
        /// </summary>
        /// <param name="content">CSV file content.</param>
        /// <param name="csvResultFilePath">CSV file path.</param>
        /// <returns></returns>
        private static string createCSVFile(string content,string csvResultFilePath)
        {
            string fileName = DateTime.Now.Ticks.ToString()+".csv";
            File.AppendAllText(csvResultFilePath + fileName, content);
            return fileName;
        }


        /// <summary>
        /// Function creates csv file content based on type of object.
        /// </summary>
        /// <param name="image">Compressed images or list of compressed images.</param>
        /// <param name="csvResultFilePath">CSV file path.</param>
        /// <returns></returns>
        public static string createCSVFile(object image,string csvResultFilePath)
        {
            StringBuilder fileContent = new StringBuilder();
            fileContent.AppendLine("ImageId,CompressedImageURL");
            if (image is Image)
            {
                Image img = image as Image;
                fileContent.AppendLine(img.ToString());
            }
            else
            {
                List<Image> images = image as List<Image>;
                foreach (Image img in images)
                {
                    fileContent.AppendLine(img.ToString());
                }
            }

            return createCSVFile(fileContent.ToString(), csvResultFilePath);
        }

        /// <summary>
        /// Parses raw csv file and returns list of images to compress.
        /// </summary>
        /// <param name="rawCSV">Raw csv byte array.</param>
        /// <param name="filePath">CSV file path.</param>
        /// <returns></returns>
        public static List<Image> ParseRawCSV(byte[] rawCSV,string filePath)
        {
            string fileName = DateTime.Now.Ticks.ToString() + ".csv";
            filePath = filePath + fileName;
            File.WriteAllBytes(filePath, rawCSV);
            return ParseCSV(filePath);
        }
    }
}