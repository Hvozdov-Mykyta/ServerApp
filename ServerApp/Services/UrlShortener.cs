using Microsoft.EntityFrameworkCore;
using ServerApp.Models;
using System.Text;

namespace ServerApp.Services
{
    public class UrlShortener
    {
        private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private const int ShortUrlLength = 8;

        public async Task<string> ShortenUrlAsync(DBContext db, string originalUrl, int userId)
        {
            var random = new Random();
            StringBuilder stringBuilder = new StringBuilder(ShortUrlLength);
            do
            {
                stringBuilder.Clear();
                for (int i = 0; i < ShortUrlLength; i++)
                {
                    stringBuilder.Append(Characters[random.Next(Characters.Length)]);
                }
            } 
            while (await db.ShortenedLinks.AnyAsync(record => record.shortenedUrl == stringBuilder.ToString()));

            return stringBuilder.ToString();
        }
    }
}