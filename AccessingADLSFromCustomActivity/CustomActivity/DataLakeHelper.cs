using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Azure;
using Microsoft.Azure.Management.DataLake.StoreFileSystem;
using Microsoft.Azure.Management.DataLake.StoreFileSystem.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace CustomActivity
{
    public class DataLakeHelper
    {
        private IDataLakeStoreFileSystemManagementClient inner_client;

        private string client_id = "";
        private string client_key = "";
        private string subscription_id = "";
        private string tenant_id = "";
        private string adls_account_name = "";

        public DataLakeHelper()
        {
            create_adls_client();
        }

        private void create_adls_client()
        {

            var authenticationContext = new AuthenticationContext($"https://login.windows.net/{tenant_id}");
            var credential = new ClientCredential(clientId: client_id, clientSecret: client_key);
            var result = authenticationContext.AcquireToken(resource: "https://management.core.windows.net/", clientCredential: credential);

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            string token = result.AccessToken;

            var _credentials = new TokenCloudCredentials(subscription_id, token);
            inner_client = new DataLakeStoreFileSystemManagementClient(_credentials);
        }

        public void StoreData(string path, List<string> rows, bool append)
        {
            try
            {

                var buffer = new MemoryStream();
                var sw = new StreamWriter(buffer);

                foreach (var row in rows)
                {
                    //Ensure the request is below 4mb in size to avoid column alignment issue
                    if (buffer.Length + Encoding.UTF8.GetByteCount(row) > 3500000)
                    {
                        buffer.Position = 0;
                        if (append)
                        {
                            execute_append(path, buffer);
                        }
                        else
                        {
                            execute_create(path, buffer);
                            append = true;
                        }

                        buffer = new MemoryStream();
                        sw = new StreamWriter(buffer);
                    }
                    sw.WriteLine(row);
                    sw.Flush();
                }

                if (buffer.Length <= 0) return;

                buffer.Position = 0;
                if (append)
                {
                    execute_append(path, buffer);
                }
                else
                {
                    execute_create(path, buffer);
                }
            }
            catch (Exception e)
            {
                throw;
            }

        }

        private AzureOperationResponse execute_create(string path, MemoryStream ms)
        {
            var beginCreateResponse = inner_client.FileSystem.BeginCreate(path, adls_account_name, new FileCreateParameters());
            var createResponse = inner_client.FileSystem.Create(beginCreateResponse.Location, ms);
            Console.WriteLine("File Created");
            return createResponse;
        }

        private AzureOperationResponse execute_append(string path, MemoryStream ms)
        {
            var beginAppendResponse = inner_client.FileSystem.BeginAppend(path, adls_account_name, null);
            var appendResponse = inner_client.FileSystem.Append(beginAppendResponse.Location, ms);
            Console.WriteLine("Data Appended");
            return appendResponse;
        }
    }
}