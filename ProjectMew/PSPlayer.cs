using System;
using System.Threading.Tasks;
using ProjectMew.Auth;
using System.Net;
using System.Net.Http;
using ProjectMew.Exceptions;
using ProjectMew.Extensions;
using PokemonGoDesktop.API.Proto;
using PokemonGoDesktop.API.Common;
using PokemonGoDesktop.API.Proto.Services;
using System.Collections.Generic;

namespace ProjectMew
{
    /*
      public PlayerData(PlayerData other) : this() 
      {
          creationTimestampMs_ = other.creationTimestampMs_;
          username_ = other.username_;
          team_ = other.team_;
          tutorialState_ = other.tutorialState_.Clone();
          Avatar = other.avatar_ != null ? other.Avatar.Clone() : null;
          maxPokemonStorage_ = other.maxPokemonStorage_;
          maxItemStorage_ = other.maxItemStorage_;
          DailyBonus = other.dailyBonus_ != null ? other.DailyBonus.Clone() : null;
          EquippedBadge = other.equippedBadge_ != null ? other.EquippedBadge.Clone() : null;
          ContactSettings = other.contactSettings_ != null ? other.ContactSettings.Clone() : null;
          currencies_ = other.currencies_.Clone();
       }
     */
    public class PSPlayer
    {
        private string _apiUrl;
        private AuthTicket _authTicket;
        private AuthType _authType = AuthType.Google;
        private readonly HttpClient _httpClient;
        public bool isLoggedin = false;

        /// <summary>
        /// This represents the player data from the server
        /// </summary>
        public PlayerData PokemonPlayer { get; set; }

        /// <summary>
        /// This represents the GeoLocation the player is currently situated
        /// </summary>
        public GeoLocation CurrentLocation { get; set; }

        /// <summary>
        /// This represents the GeoLocation the player is attempting to move to 
        /// </summary>
        public GeoLocation TargettedLocation { get; set; }

        /// <summary>
        /// This represents the last recorded soft ban for the player
        /// </summary>
        public DateTime LastSoftBan { get; set; }

        /// <summary>
        /// This represents the Inventory of the player - TO CONFIRM
        /// </summary>
        public InventoryDelta Inventory { get; set; }

        /// <summary>
        /// Access Token for Pokemon Go Sessions
        /// </summary>
        public string AccessToken { get; set; }

        public async Task DoPtcLogin(string username, string password)
        {
            _authType = AuthType.PTC;
            try
            {
                AccessToken = await PtcLogin.GetAccessToken(username, password);
                isLoggedin = true;
            }
            catch (PtcOfflineException)
            {
                ProjectMew.Log.ConsoleError("Error Occured While Attempting to Login: PTC Offline Exception");
            }
            catch (AccountNotVerifiedException)
            {
                ProjectMew.Log.ConsoleError("Error Occured While Attempting to Login: PTC Account is not verified, please verify your account by clicking on the activation link in the email");
            }
        }

        public async Task DoGoogleLogin()
        {
            _authType = AuthType.Google;
            GoogleLogin.TokenResponseModel tokenResponse = null;
            if (string.IsNullOrEmpty(ProjectMew.Config.RefreshToken))
            {
                tokenResponse = await GoogleLogin.GetAccessToken(ProjectMew.Config.RefreshToken);
                AccessToken = tokenResponse?.id_token;
            }

            if (AccessToken == null)
            {
                var deviceCode = await GoogleLogin.GetDeviceCode();
                tokenResponse = await GoogleLogin.GetAccessToken(deviceCode);
                ProjectMew.Config.RefreshToken = tokenResponse?.refresh_token;
                AccessToken = tokenResponse?.id_token;
            }
            isLoggedin = true;
        }

        public async Task SetServer(GeoLocation Location = null)
        {
            var serverRequest = RequestEnvelopeBuilder.GetInitialRequestEnvelope(AccessToken, _authType, (Location == null ? CurrentLocation : Location),
                RequestType.GetPlayerProfile, RequestType.GetHatchedEggs, RequestType.GetInventory,
                RequestType.CheckAwardedBadges, RequestType.DownloadSettings);

            var serverResponse = await _httpClient.PostProto(Resources.RpcUrl, serverRequest);

            if (serverResponse.AuthTicket == null)
                throw new AccessTokenExpiredException(serverResponse.Error);

            _authTicket = serverResponse.AuthTicket;

            _apiUrl = serverResponse.ApiUrl;
        }

        internal async Task StartBot()
        {
            await SetServer();
        }

        public PSPlayer()
        {
            //credits to ferox
            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = false
            };
            _httpClient = new HttpClient(new RetryHandler(handler));
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Niantic App");
            //"Dalvik/2.1.0 (Linux; U; Android 5.1.1; SM-G900F Build/LMY48G)");
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type",
                "application/x-www-form-urlencoded");
        }

        internal void Evolve(PokemonId id)
        {
            throw new NotImplementedException();
        }

        internal void Evolve(IEnumerable<PokemonId> ids, int amount)
        {
            foreach (var id in ids)
            {
                for (int i = 0; i < amount; i++)
                {
                    Evolve(id);
                }
            }
        }
    }
}
