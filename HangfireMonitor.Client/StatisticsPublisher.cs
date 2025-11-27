using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace HangfireMonitor.Client
{
    public class StatisticsPublisher : IDisposable
    {
        public StatisticsPublisher(string apiKey, Uri apiBaseUrl, ILogger<StatisticsPublisher> logger) : this(CreateHttpClient(apiKey, apiBaseUrl), logger)
        {
        }

        public StatisticsPublisher(HttpClient httpClient, ILogger<StatisticsPublisher> logger)
        {
            _httpClient = httpClient;
            _logger = logger ?? NullLogger<StatisticsPublisher>.Instance;
        }

        private readonly HttpClient _httpClient;
        private readonly ILogger<StatisticsPublisher> _logger;

        public async Task PublishAsync(CancellationToken cancellationToken = default)
        {
            JobStorage currentJobStorage;
            try
            {
                currentJobStorage = JobStorage.Current;
            }
            catch (InvalidOperationException e)
            {
                _logger.LogWarning("Hangfire JobStorage is not initialized. Statistics will not be published.");
                return;
            }
            
            IMonitoringApi monitoringApi;
            try
            {
                monitoringApi = currentJobStorage.GetMonitoringApi();
                if (monitoringApi == null)
                {
                    _logger.LogWarning("Hangfire JobStorage did not return an IMonitoringApi instance. Statistics will not be published.");
                    return;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Hangfire JobStorage failed to return an IMonitoringApi instance. Statistics will not be published.");
                return;
            }

            StatisticsDto statistics;
            try
            {
                statistics = monitoringApi.GetStatistics();
                if (statistics == null)
                {
                    _logger.LogWarning("Hangfire Monitoring API did not return statistics. Statistics will not be published.");
                    return;
                }
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Hangfire Monitoring API failed to return statistics. Statistics will not be published.");
                return;
            }
            
            var json = JsonConvert.SerializeObject(statistics, Formatting.Indented);
            try
            {
                var responseMessage = await _httpClient.PostAsync("/api/statistics", new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);
                if (responseMessage.StatusCode != System.Net.HttpStatusCode.Created)
                    _logger.LogWarning("Failed to post statistics. Status code: {StatusCode}", responseMessage.StatusCode);
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Failed to post statistics.");
            }
        }
        
        private static HttpClient CreateHttpClient(string apiKey, Uri apiBaseUrl)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = apiBaseUrl,
                DefaultRequestHeaders = { {"X-API-Key", apiKey} }
            };
            
            return httpClient;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}