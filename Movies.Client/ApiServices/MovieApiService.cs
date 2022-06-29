using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Movies.Client.Models;
using Newtonsoft.Json;

namespace Movies.Client.ApiServices
{
    public class MovieApiService : IMovieApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MovieApiService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        public async Task<UserInfoViewModel> GetUserInfo()
        {
            var idpClient = _httpClientFactory.CreateClient("IDPClient");
            var metaDataResponse = await idpClient.GetDiscoveryDocumentAsync();
            if (metaDataResponse.IsError)
            {
                throw new HttpRequestException("Something went wrong while requesting the access token");
            }

            var accesToken =
                await _httpContextAccessor.HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var userInfoResponse = await idpClient.GetUserInfoAsync(
                new UserInfoRequest()
                {
                    Address = metaDataResponse.UserInfoEndpoint,
                    Token = accesToken
                });

            if (userInfoResponse.IsError)
            {
                throw new HttpRequestException("Something went wrong while getting user info");
            }

            var userinfoDictionary = new Dictionary<string, string>();
            
            foreach (var claim in userInfoResponse.Claims)
            {
                userinfoDictionary.Add(claim.Type,claim.Value);
            }

            return new UserInfoViewModel(userinfoDictionary);
        }

        public async Task<IEnumerable<Movie>> GetMovies()
        {
            // WAY 1

            var httpClient = _httpClientFactory.CreateClient("MovieAPIClient");

            var request = new HttpRequestMessage(HttpMethod.Get, "/Movies");

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var movieList = JsonConvert.DeserializeObject<List<Movie>>(content);
            return movieList;

            // WAY 2

            //// 1. Get token from identity server 
            //// 2. Send request to protected API
            //// 3. Deserialize object to MovieList

            //// 1. "retrieve" our api credentials. This must be registered on Identity Server!

            //var apiClientCredentials = new ClientCredentialsTokenRequest
            //{
            //    Address = "https://localhost:5005/connect/token",

            //    ClientId = "movieClient",
            //    ClientSecret = "secret",

            //    // This is the cope our Protected API requires.
            //    Scope = "movieAPI"
            //};
            //// creartes a new HttpClient to talk to our IdenityServer (localhost:5005)
            //var client = new HttpClient();

            //// Just check if we can reach the Discovery document. not 100% needed but..
            //var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5005");
            //if (disco.IsError)
            //{
            //    return null;
            //}

            //// 2. Authenticates and get a n access token from Identity Server
            //var tokenResponse = await client.RequestClientCredentialsTokenAsync(apiClientCredentials);
            //if (tokenResponse.IsError)
            //{
            //    return null;
            //}

            //// 2. send Request to Protected API

            //// Another HttpClient for talking now with our Protected API
            //var apiClient= new HttpClient();

            //// 3. Set the access_token in the request Authorizatoin: Bearer <token>
            //client.SetBearerToken(tokenResponse.AccessToken);


            ////4. Send a request to our Protected API
            //var response = await client.GetAsync("https://localhost:5000/api/movies");
            //response.EnsureSuccessStatusCode();

            //var content = await response.Content.ReadAsStringAsync();

            //List<Movie> movielist = JsonConvert.DeserializeObject<List<Movie>>(content);
            //return movielist;


            ////var movieList = new List<Movie>();
            ////movieList.Add(
            ////    new Movie
            ////    {
            ////        Id = 1,
            ////        Genre = "Comics",
            ////        Title = "asd",
            ////        Rating = "9.2",
            ////        ImageUrl = "images/src",
            ////        ReleaseDate = DateTime.Now,
            ////        Owner = "uba"
            ////    }
            ////);
            ////return await Task.FromResult(movieList);
        }

        public Task<Movie> GetMovieById(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<Movie> CreateMovie(Movie movie)
        {
            throw new System.NotImplementedException();
        }

        public Task<Movie> UpdateMovie(Movie movie)
        {
            throw new System.NotImplementedException();
        }

        public Task DeleteMovie(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}