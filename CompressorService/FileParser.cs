using CompressorService.Models;
using LinqToExcel;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CompressorService.Enums;
using System;

namespace CompressorService
{
    public static class FileParser
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
    }
}