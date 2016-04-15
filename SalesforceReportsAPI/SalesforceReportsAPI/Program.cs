using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesforceReportsAPI
{
    class Program
    {
        static string username = "";
        static string password = "";
        static string usertoken = "";
        static string consumerKey = "";
        static string consumerSecret = "";
        static string url = "https://login.salesforce.com/services/oauth2/token";
        static string reportId = "xxxxxxxxxxxxxxx";    //Salesforce report id  

        static void Main(string[] args)
        {

            var task = ExecuteReport();
            task.Wait();

            Console.ReadLine();
        }

        public static async Task<string> ExecuteReport()
        {
            var sf_client = new Salesforce.Common.AuthenticationClient();
            sf_client.ApiVersion = "v34.0";
            await sf_client.UsernamePasswordAsync(consumerKey, consumerSecret, username, password + usertoken, url);

            string reportUrl = "/services/data/" + sf_client.ApiVersion + "/analytics/reports/" + reportId;

            var client = new RestSharp.RestClient(sf_client.InstanceUrl);
            var request = new RestSharp.RestRequest(reportUrl, RestSharp.Method.GET);
            request.AddHeader("Authorization", "Bearer " + sf_client.AccessToken);
            var restResponse = client.Execute(request);
            var reportData = restResponse.Content;
            return reportData;
        }
    }
}
