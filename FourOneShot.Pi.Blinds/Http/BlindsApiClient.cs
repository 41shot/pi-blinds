using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FourOneShot.Pi.Blinds.Http
{
    public class BlindsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BlindsApiClient> _logger;

        public BlindsApiClient(IConfiguration configuration, ILogger<BlindsApiClient> logger)
        {
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(logger);

            _logger = logger;

            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(configuration["BlindsAPIBaseUri"]);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            _httpClient = httpClient;
        }

        public async Task<Stream> GetConfigurationAsStream()
        {
            var response = await GetApiResponseWithRetries("configuration");

            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<int> GetChannel()
        {
            var response = await _httpClient.GetAsync("channel");
            response.EnsureSuccessStatusCode();
            
            return int.Parse(await response.Content.ReadAsStringAsync());
        }

        public async Task SetChannel(int channel)
        {
            await PostOperation($"channel/{channel}");
        }

        public async Task Open()
        {
            await PostOperation("open");
        }

        public async Task Close()
        {
            await PostOperation("close");
        }

        public async Task Stop()
        {
            await PostOperation("stop");
        }

        public async Task ChannelUp()
        {
            await PostOperation("channel/up");
        }

        public async Task ChannelDown()
        {
            await PostOperation("channel/down");
        }

        public async Task ChannelLimit()
        {
            await PostOperation("channel/limit");
        }

        public async Task Pair()
        {
            await PostOperation("pair");
        }

        public async Task Reset()
        {
            await PostOperation("reset");
        }

        private async Task PostOperation(string operationName)
        {
            var response = await _httpClient.PostAsync(operationName, null);
            response.EnsureSuccessStatusCode();
        }

        private async Task<HttpResponseMessage> GetApiResponseWithRetries(
            string resourcePath,
            int maxRetryCount = 5,
            int timeoutSeconds = 40,
            int requestIntervalSeconds = 5)
        {
            Exception exception;
            var numberOfRetries = 0;
            var timeout = DateTime.Now.AddSeconds(timeoutSeconds);

            do
            {
                if (numberOfRetries > 0)
                {
                    // Throttle re-try attempts.
                    await Task.Delay(TimeSpan.FromSeconds(requestIntervalSeconds));
                }

                try
                {
                    var response = await _httpClient.GetAsync(resourcePath);
                    response.EnsureSuccessStatusCode();

                    return response;
                }
                catch (HttpRequestException httpEx)
                {
                    _logger.LogError(httpEx, "Blinds API request failed.");

                    exception = httpEx;
                }
            }
            while (numberOfRetries++ < maxRetryCount && DateTime.Now < timeout);

            throw new HttpRequestException(
                $"Failed to get API resource '{resourcePath}' after {numberOfRetries} attempts.",
                exception);
        }
    }
}
