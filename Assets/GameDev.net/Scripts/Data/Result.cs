using System;

/// <summary>
/// Data holder for search results.
/// </summary>
namespace gamedev.net.data
{
    public class Result
    {
        public string title;
        public string content;
        public string itemUrl;
        public string objectUrl;
        public string started;
        public string updated;
        public int comments;
        public string author;
        public string authorPhotoThumbnail;
        public int reputation;

        public string GetDateFormatted()
        {
            DateTime dt = DateTime.Parse(started);
            //return dt.ToString("HH:mm MM/dd/yyyy");
            return dt.ToString("MMM dd, yyyy");
        }

        public Result()
        {

        }
    }
}