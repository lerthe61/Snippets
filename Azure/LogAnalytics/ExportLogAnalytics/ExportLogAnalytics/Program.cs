using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Azure.OperationalInsights;
using Microsoft.Azure.OperationalInsights.Models;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;

namespace ExportLogAnalytics
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var clientId = "c0b75ac5-9bae-44d5-a118-eea0ae39adfd";
            var clientSecret = "@v0s8o81NA=-FCdtozuzNipy.Q2EGM==";

            var domain = "wolterskluwer.onmicrosoft.com";
            var authEndpoint = "https://login.microsoftonline.com";
            var tokenAudience = "https://api.loganalytics.io/";
            var workspaceId = "e4505189-dcc5-4d5d-9b36-b88a88afbdf3";

            var serviceClientCredentials = GetServiceClientCredentials(clientId, clientSecret, domain, authEndpoint, tokenAudience);
            var client = new OperationalInsightsDataClient(serviceClientCredentials)
            {
                WorkspaceId = workspaceId
            };

            var query = "search *\r\n| where Type == \"ETWEvent\" and Message contains \"MultiFieldSearch\" and TaskName == \"Message\" and Message contains \"QueryString\"\r\n| project TimeGenerated, SearchTerms=extract(\"\\\"QueryString\\\":\\\"([^\\\"]*?)\\\"\", 1, Message)\r\n| take 10";

            // Run query and store results in log analyzer
            QueryResults results = null;
            try
            {
                results = await client.QueryAsync(query);
                await ProcessQueryResults(results.Results);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Environment.Exit(1);
            }
        }

        private static async Task ProcessQueryResults(IEnumerable<IDictionary<string, string>> queryResults)
        {
            bool firstResult = true;
            Configuration configuration = new Configuration()
            {
                Encoding = Encoding.UTF8
            };
            string filePath = GetFileName();
            using (var stream = File.OpenWrite(filePath))
            using (var textWriter = new StreamWriter(stream, configuration.Encoding))
            using (var writer = new CsvWriter(textWriter, configuration))
            {
                await stream.WriteAsync(configuration.Encoding.GetPreamble());

                foreach (var queryResult in queryResults)
                {
                    if (firstResult)
                    {
                        foreach (var header in queryResult)
                        {
                            writer.WriteField(header.Key);
                        }
                        await writer.NextRecordAsync();
                        firstResult = false;
                    }

                    foreach (var entry in queryResult)
                    {
                        writer.WriteField(entry.Value);
                    }
                    await writer.NextRecordAsync();
                }
            }
        }

        private static string GetFileName()
        {
            var i = 0;
            var path = Path.GetDirectoryName(typeof(Program).Assembly.Location);
            var mask = Path.Combine(path, "export{0}.csv");
            while (File.Exists( string.Format(mask, i)))
            {
                i++;
            }

            return string.Format(mask, i);
        }

        private static ServiceClientCredentials GetServiceClientCredentials(
            string clientId, 
            string clientSecret,
            string domain,
            string authEndpoint,
            string tokenAudience)
        {
            var adSettings = new ActiveDirectoryServiceSettings
            {
                AuthenticationEndpoint = new Uri(authEndpoint),
                TokenAudience = new Uri(tokenAudience),
                ValidateAuthority = true
            };

            var serviceClientCredentials = ApplicationTokenProvider.LoginSilentAsync(domain, clientId, clientSecret, adSettings)
                .GetAwaiter().GetResult();
            return serviceClientCredentials;
        }
    }
}
