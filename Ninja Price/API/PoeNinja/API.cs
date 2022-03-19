﻿using System.Net.Http;
using System.Threading.Tasks;

namespace Ninja_Price.API.PoeNinja
{
    public class Api
    {
        public static async Task<string> DownloadFromUrl(string url)
        {
            using var handler = new HttpClientHandler { UseCookies = false };
            using var client = new HttpClient(handler);
            return await client.GetStringAsync(url).ConfigureAwait(false);
        }
    }
}