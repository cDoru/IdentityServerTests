﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Thinktecture.IdentityModel.Client;

namespace MRTutorial.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync();
            Console.ReadLine();
        }

        private static async void RunAsync()
        {
            var response = await GetTokenAsync();
            var result = await CallApi(response.AccessToken);
        }

        private static async Task<TokenResponse> GetTokenAsync()
        {
            var client = new OAuth2Client(
                new Uri("https://localhost:44303/identity/connect/token"),  //MRTutorial.IdentityServer
                "client",
                "secret");

            return await client.RequestClientCredentialsAsync("sampleApi");
        }
        private static async Task<string> CallApi(string token)
        {
            var client = new HttpClient();
            client.SetBearerToken(token);

            return await client.GetStringAsync("https://localhost:44305/api/test");  //MRTutorial.ProtectedApi
        }
    }
}
