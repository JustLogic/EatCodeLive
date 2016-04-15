using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforceReportsAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            string message = "";
            try
            {
                var task = ExecuteReport();
                task.Wait();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    message += Environment.NewLine + ex.InnerException.Message;
                }
            }

            Console.WriteLine("Results: ");
            Console.WriteLine(message);
            Console.ReadLine();
        }

        public static async Task ExecuteReport()
        {
            string username = "";
            string password = "";
            string usertoken = "";
            string consumerKey = "";
            string consumerSecret = "";

            var url = "https://login.salesforce.com/services/oauth2/token";
            
            var authClient = new Salesforce.Common.AuthenticationClient();
            authClient.ApiVersion = "v34.0";
            await authClient.UsernamePasswordAsync(consumerKey, consumerSecret, username, password + usertoken, url);

            var reportId = "xxxxxxxxxxxxxxx";    //Salesforce report id        

            string reportUrl = "/services/data/" + authClient.ApiVersion + "/analytics/reports/" + reportId;
            
            var restClient = new RestSharp.RestClient(authClient.InstanceUrl);
            var restRequest = new RestSharp.RestRequest(reportUrl, RestSharp.Method.GET);
            restRequest.AddHeader("Authorization", "Bearer " + authClient.AccessToken);
            var restResponse = restClient.Execute(restRequest);
            var reportData = restResponse.Content;           
        }
    }
}
