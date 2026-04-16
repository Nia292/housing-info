using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Dalamud.Plugin.Services;
using HousingDatabase.Collector;
using Newtonsoft.Json;

namespace HousingDatabase;


public class HousingDataPusher
{
    private readonly Configuration configuration;
    private readonly HttpClient httpClient;
    private readonly IPluginLog pluginLog;

    public HousingDataPusher(IPluginLog pluginLog, Configuration configuration)
    {
        this.pluginLog = pluginLog;
        this.httpClient = new HttpClient();
        this.configuration = configuration;
    }

    public void PushToRemote(List<HouseInfoEntry> entries)
    {
        try
        {
            if (!configuration.HasRemoteServer())
            {
                return;
            }
            var uri = configuration.RemoteUpdateUri();
            var payload = JsonConvert.SerializeObject(entries);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            httpRequestMessage.Content = new StringContent(payload, Encoding.UTF8, "application/json");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(configuration.RemoteKey)));
            pluginLog.Debug("Pushing housing data to " + uri);
            httpClient
                .SendAsync(httpRequestMessage)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            pluginLog.Error(e, "Failed to publish to server");
        }
    }

    public void PushGreeting(HouseId houseId, string greeting)
    {
        try
        {
            if (!configuration.HasRemoteServer())
            {
                return;
            }
            var uri = configuration.RemoteUpdateGreetingUri(houseId);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
            httpRequestMessage.Content = new StringContent(greeting, Encoding.UTF8, "plain/text");
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(configuration.RemoteKey)));
            pluginLog.Debug("Pushing greeting data to " + uri);
            httpClient
                .SendAsync(httpRequestMessage)
                .ConfigureAwait(false);
        }
        catch (Exception e)
        {
            pluginLog.Error(e, "Failed to publish greeting to server");
        }
    }
}
