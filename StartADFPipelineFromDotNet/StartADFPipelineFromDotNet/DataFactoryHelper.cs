using System;
using Microsoft.Azure;
using Microsoft.Azure.Management.DataFactories.Core;
using Microsoft.Azure.Management.DataFactories.Core.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace StartADFPipelineFromDotNet
{
    public class DataFactoryHelper
    {
        private IDataFactoryManagementClient inner_client;

        private string client_id = "";
        private string client_key = "";
        private string subscription_id = "";
        private string tenant_id = "";


        public DataFactoryHelper()
        {
            create_adf_client();
        }

        private void create_adf_client()
        {
            var authenticationContext = new AuthenticationContext($"https://login.windows.net/{tenant_id}");
            var credential = new ClientCredential(clientId: client_id, clientSecret: client_key);
            var result = authenticationContext.AcquireToken(resource: "https://management.core.windows.net/", clientCredential: credential);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            var token = result.AccessToken;

            var _credentials = new TokenCloudCredentials(subscription_id, token);
            inner_client = new DataFactoryManagementClient(_credentials);
        }

        public void StartPipeline(string resourceGroup, string dataFactory, string pipelineName, DateTime slice)
        {
            var pipeline = inner_client.Pipelines.Get(resourceGroup, dataFactory, pipelineName);

            pipeline.Pipeline.Properties.Start = DateTime.Parse($"{slice.Date:yyyy-MM-dd}T00:00:00Z");
            pipeline.Pipeline.Properties.End = DateTime.Parse($"{slice.Date:yyyy-MM-dd}T23:59:59Z");
            pipeline.Pipeline.Properties.IsPaused = false;

            inner_client.Pipelines.CreateOrUpdate(resourceGroup, dataFactory, new PipelineCreateOrUpdateParameters()
            {
                Pipeline = pipeline.Pipeline
            });
        }
    }
}