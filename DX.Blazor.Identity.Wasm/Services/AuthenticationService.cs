using Blazored.LocalStorage;
using DX.Blazor.Identity.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DX.Blazor.Identity.Wasm.Services
{
    public class AuthenticationService : AuthenticationService<RegistrationModel>        
    {
        public AuthenticationService(HttpClient client, AuthenticationStateProvider authStateProvider, 
            ILocalStorageService localStorage) 
            : base(client, authStateProvider, localStorage)
        {

        }
    }

    public class AuthenticationService<TRegistrationModel> : AuthenticationService<TRegistrationModel, AuthenticationModel>
        where TRegistrationModel : class, new()
    {
        public AuthenticationService(HttpClient client, AuthenticationStateProvider authStateProvider, 
            ILocalStorageService localStorage)
            : base(client, authStateProvider, localStorage)
        {

        }
    }


    public class AuthenticationService<TRegistrationModel, TAuthenticationModel> 
        : IAuthService<TRegistrationModel, TAuthenticationModel>
        where TRegistrationModel: class, new()
        where TAuthenticationModel : class, new()
    {
        const string authTokenName = "authToken";
        const string refreshTokenName = "refreshToken";
        const string registerEndpoint = "Accounts/Registration";
        const string loginEndpoint = "Accounts/Login";
        //const string logoutEndpoint = "Accounts/Logout";
        const string tokenRefreshEndpoint = "token/refresh";

        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _options;
        private readonly AuthenticationStateProvider _authStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AuthenticationService(HttpClient client, AuthenticationStateProvider authStateProvider, 
            ILocalStorageService localStorage)
        {
            _client = client;
            _options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            _authStateProvider = authStateProvider;
            _localStorage = localStorage;
        }

        public async Task<RegistrationResponseModel> RegisterUser(TRegistrationModel userForRegistration)
        {
            var content = JsonSerializer.Serialize(userForRegistration);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");

            var registrationResult = await _client.PostAsync(registerEndpoint, bodyContent);
            var registrationContent = await registrationResult.Content.ReadAsStringAsync();

            if (!registrationResult.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<RegistrationResponseModel>(registrationContent, _options);
                ArgumentNullException.ThrowIfNull(result);
                return result;
            }

            return new RegistrationResponseModel { IsSuccessfulRegistration = true };
        }

        public async Task<AuthResponseModel> Login(TAuthenticationModel userForAuthentication)
        {
            var content = JsonSerializer.Serialize(userForAuthentication);
            var bodyContent = new StringContent(content, Encoding.UTF8, "application/json");

            var authResult = await _client.PostAsync(loginEndpoint, bodyContent);
            var authContent = await authResult.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthResponseModel>(authContent, _options);

            ArgumentNullException.ThrowIfNull(result);

            if (!authResult.IsSuccessStatusCode)
                return result;

            await _localStorage.SetItemAsync(authTokenName, result.Token);
            await _localStorage.SetItemAsync(refreshTokenName, result.RefreshToken);
            ((AuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Token);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Token);

            return new AuthResponseModel { IsAuthSuccessful = true };
        }

        public async Task<string> RefreshToken()
        {
            var token = await _localStorage.GetItemAsync<string>(authTokenName);
            var refreshToken = await _localStorage.GetItemAsync<string>(refreshTokenName);

            var refreshModel = JsonSerializer.Serialize(new RefreshTokenModel { Token = token, RefreshToken = refreshToken });
            var bodyContent = new StringContent(refreshModel, Encoding.UTF8, "application/json");

            var refreshResult = await _client.PostAsync(tokenRefreshEndpoint, bodyContent);
            var refreshContent = await refreshResult.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<AuthResponseModel>(refreshContent, _options);

            ArgumentNullException.ThrowIfNull(result);

            if (!refreshResult.IsSuccessStatusCode)
                throw new ApplicationException("Something went wrong during the refresh token action");

            await _localStorage.SetItemAsync(authTokenName, result.Token);
            await _localStorage.SetItemAsync(refreshTokenName, result.RefreshToken);

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.Token);
            return result.Token;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync(authTokenName);
            await _localStorage.RemoveItemAsync(refreshTokenName);
            ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
            _client.DefaultRequestHeaders.Authorization = null;
        }

    }
}
