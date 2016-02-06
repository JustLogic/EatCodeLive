using System;
using System.Collections.Generic;
using System.Net.Http;
using Salesforce.Common;
using Salesforce.Common.Models;
using Salesforce.Force;

namespace CustomActivity
{
    public class SalesForceHelper
    {
        private IForceClient inner_sf_client;
        private HttpClient inner_http_client;
        private string instance_url;
        private string api_version;

        public SalesForceHelper()
        {
            initialize_helper();
        }

        private void initialize_helper()
        {
            var securityToken = "";
            var consumerKey = "";
            var consumerSecret = "";
            var username = "";
            var password = "";//password + securityToken;

            var auth = new AuthenticationClient();

            var url = "https://login.salesforce.com/services/oauth2/token";

            auth.UsernamePasswordAsync(consumerKey, consumerSecret, username, password, url).Wait();
            inner_sf_client = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);

            inner_http_client = new HttpClient();
            inner_http_client.DefaultRequestHeaders.Add("Authorization", "Bearer " + auth.AccessToken);
        }


        public IEnumerable<SalesforceLeadDto> GetLeads()
        {
            QueryResult<SalesforceLeadDto> qryResults = null;
            try
            {
                qryResults = inner_sf_client.QueryAsync<SalesforceLeadDto>(@"Select City, Company, Country, CreatedById, CreatedDate, Description, Email, Fax, FirstName, Id, 
                                                                             Industry, IsConverted, IsDeleted, LastActivityDate, LastModifiedById, LastModifiedDate, LastName, 
                                                                             LeadSource, MobilePhone, Name, OwnerId, Phone, PostalCode, RecordTypeId, State, Status, Street, 
                                                                             SystemModstamp from Lead").Result;
            }
            catch (ForceException fe)
            {
                Console.WriteLine(fe.Error);
                throw;
            }
            catch (AggregateException e)
            {
                foreach (var innerException in e.InnerExceptions)
                {
                    Console.WriteLine(innerException.Message);
                }

                throw;
            }

            var nextRecordsUrl = qryResults.NextRecordsUrl;

            foreach (var record in qryResults.Records)
            {
                yield return record;
            }

            Console.WriteLine("Api Access Count: {0} Number Records: {1}", 1, qryResults.Records.Count);
            if (!string.IsNullOrEmpty(nextRecordsUrl))
            {
                var index = 1;
                while (true)
                {
                    QueryResult<SalesforceLeadDto> continuationResults = null;

                    try
                    {
                        continuationResults = inner_sf_client.QueryContinuationAsync<SalesforceLeadDto>(nextRecordsUrl).Result;
                    }
                    catch (ForceException fe)
                    {
                        Console.WriteLine(fe.Error);
                        throw;
                    }
                    catch (AggregateException e)
                    {
                        foreach (var innerException in e.InnerExceptions)
                        {
                            Console.WriteLine(innerException.Message);
                        }

                        throw;
                    }

                    foreach (var record in continuationResults.Records)
                    {
                        yield return record;
                    }
                    index++;
                    Console.WriteLine("Api Access Count: {0} Number Records: {1}", index, continuationResults.Records.Count);
                    if (string.IsNullOrEmpty(continuationResults.NextRecordsUrl)) yield break;
                    nextRecordsUrl = continuationResults.NextRecordsUrl;
                }
            }
        }

    }
}