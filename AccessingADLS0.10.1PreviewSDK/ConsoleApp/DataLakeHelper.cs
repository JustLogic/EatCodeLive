using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Azure.Management.DataLake.Store;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;

namespace ConsoleApp
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
            var _credentials = new TokenCredentials(token);
            inner_client = new DataLakeStoreFileSystemManagementClient(_credentials);
            inner_client.SubscriptionId = subscription_id;
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

        private void execute_create(string path, MemoryStream ms)
        {
            inner_client.FileSystem.Create(path, adls_account_name, ms, false);
            Console.WriteLine("File Created");
        }

        private void execute_append(string path, MemoryStream ms)
        {
            inner_client.FileSystem.Append(path, ms, adls_account_name);
            Console.WriteLine("Data Appended");
        }
    }
}