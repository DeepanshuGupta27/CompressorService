using System;

namespace ImageCompressor
{
    public class ImageCompressorException : Exception
    {
        public ImageCompressorException(string message) : base(message)
        {
        }

        public ImageCompressorException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
