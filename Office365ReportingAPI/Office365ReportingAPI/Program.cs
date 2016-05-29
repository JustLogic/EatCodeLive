using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Office365ReportingAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            var prg = new Program();
            //prg.get_report_list();
            //prg.run_all_reports();
            //prg.run_report_messagetrace();
            Console.ReadLine();
        }

        public void get_report_list()
        {
            var username = "";
            var password = "";

            var ub = new UriBuilder("https", "reports.office365.com");
            ub.Path = "ecp/reportingwebservice/reporting.svc";
            var fullRestURL = Uri.EscapeUriString(ub.Uri.ToString());
            var request = (HttpWebRequest)HttpWebRequest.Create(fullRestURL);
            request.Credentials = new NetworkCredential(username, password);

            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                var encode = System.Text.Encoding.GetEncoding("utf-8");
                var readStream = new StreamReader(response.GetResponseStream(), encode);
                var doc = new XDocument();
                doc = XDocument.Load(response.GetResponseStream());

                var nodes = doc.DescendantNodes().Where(x => x.NodeType == XmlNodeType.Text).ToList();

                var node_str = string.Join(",", nodes);

                doc.Save(@"C:\Office365\Reports\ReportList.xml");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void run_all_reports()
        {
            var username = "";
            var password = "";

            foreach (var report in Reports.ReportList)
            {
                var ub = new UriBuilder("https", "reports.office365.com");
                ub.Path = string.Format("ecp/reportingwebservice/reporting.svc/{0}", report);
                var fullRestURL = Uri.EscapeUriString(ub.Uri.ToString());
                var request = (HttpWebRequest)WebRequest.Create(fullRestURL);
                request.Credentials = new NetworkCredential(username, password);

                try
                {
                    var response = (HttpWebResponse)request.GetResponse();
                    var encode = System.Text.Encoding.GetEncoding("utf-8");
                    var readStream = new StreamReader(response.GetResponseStream(), encode);
                    var doc = new XmlDocument();
                    doc.LoadXml(readStream.ReadToEnd());

                    doc.Save($@"C:\Office365\Reports\{DateTime.Now:yyyyMMdd}_{report}.xml");

                    Console.WriteLine("Saved: {0}", report);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void run_report_messagetrace()
        {
            var message_items = new List<MessageTraceItem>();

            var username = "";
            var password = "";

            var credentials = new NetworkCredential(username, password);
            var handler = new HttpClientHandler { Credentials = credentials };

            using (var inner_client = new HttpClient(handler))
            {
                inner_client.BaseAddress = new Uri("https://reports.office365.com/ecp/reportingwebservice/reporting.svc/");

                inner_client.Timeout = TimeSpan.FromMinutes(25);
                inner_client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                if (inner_client != null)
                {
                    var httpResponse = inner_client.GetAsync("MessageTrace").Result;
                    var raw_response_string = httpResponse.Content.ReadAsStringAsync().Result;
                    var response_string = raw_response_string;

                    if (httpResponse.IsSuccessStatusCode)
                    {
                        var settings = new JsonSerializerSettings();
                        var result = JsonConvert.DeserializeObject<OData>(response_string, settings);

                        message_items.AddRange(result.Value);


                        if (!string.IsNullOrEmpty(result.NextLink))
                        {
                            var skiptoken = result.Value.Max(x => x.Index);
                            while (true)
                            {
                                Console.WriteLine("SkipToken: {0}", skiptoken);
                                var continuation_resp = inner_client.GetAsync(string.Format("{0}?$skiptoken={1}", "MessageTrace", skiptoken)).Result;

                                var continuation_results = JsonConvert.DeserializeObject<OData>(continuation_resp.Content.ReadAsStringAsync().Result, settings);

                                skiptoken = continuation_results.Value.Max(x => x.Index);

                                message_items.AddRange(continuation_results.Value);

                                if (string.IsNullOrEmpty(continuation_results.NextLink)) break;
                            }
                        }
                    }
                    else
                    {
                        var error = httpResponse.Content.ReadAsStringAsync().Result;
                        Console.WriteLine(error + response_string);
                    }
                }
            }
        }
    }
    public class MessageTraceItem
    {
        public string Organization { get; set; }
        public string MessageId { get; set; }
        public DateTime Received { get; set; }
        public string SenderAddress { get; set; }
        public string RecipientAddress { get; set; }
        public string Subject { get; set; }
        public string Status { get; set; }
        public string ToIP { get; set; }
        public string FromIP { get; set; }
        public int Size { get; set; }
        public Guid MessageTraceId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Index { get; set; }
    }

    public class OData
    {
        [JsonProperty("odata.metadata")]
        public string Metadata { get; set; }

        [JsonProperty("odata.nextlink")]
        public string NextLink { get; set; }
        public List<MessageTraceItem> Value { get; set; }
    }

    static class Reports
    {
        public static List<string> ReportList => new List<string>()
        {
            "MailboxActivityDaily",
            "MailboxActivityWeekly",
            "MailboxActivityMonthly",
            "MailboxActivityYearly",
            "GroupActivityDaily",
            "GroupActivityWeekly",
            "GroupActivityMonthly",
            "GroupActivityYearly",
            "StaleMailbox",
            "StaleMailboxDetail",
            "MailboxUsage",
            "MailboxUsageDetail",
            "ClientSoftwareBrowserSummary",
            "ClientSoftwareOSSummary",
            "ClientSoftwareBrowserDetail",
            "ClientSoftwareOSDetail",
            "CsActiveUserDaily",
            "CsActiveUserWeekly",
            "CsActiveUserMonthly",
            "CsActiveUserYearly",
            "CsConferenceDaily",
            "CsConferenceWeekly",
            "CsConferenceMonthly",
            "CsP2PSessionDaily",
            "CsP2PSessionWeekly",
            "CsP2PSessionMonthly",
            "CsP2PAVTimeDaily",
            "CsP2PAVTimeWeekly",
            "CsP2PAVTimeMonthly",
            "CsAVConferenceTimeDaily",
            "CsAVConferenceTimeWeekly",
            "CsAVConferenceTimeMonthly",
            "CsPSTNConferenceTimeDaily",
            "CsPSTNConferenceTimeWeekly",
            "CsPSTNConferenceTimeMonthly",
            "CsPSTNUsageDetailDaily",
            "CsClientDeviceMonthly",
            "CsClientDeviceDetailMonthly",
            "CsUserActivitiesMonthly",
            "CsUsersBlockedDaily",
            "ConnectionbyClientTypeDaily",
            "ConnectionbyClientTypeWeekly",
            "ConnectionbyClientTypeMonthly",
            "ConnectionbyClientTypeYearly",
            "ConnectionbyClientTypeDetailDaily",
            "ConnectionbyClientTypeDetailWeekly",
            "ConnectionbyClientTypeDetailMonthly",
            "ConnectionbyClientTypeDetailYearly",
            "MailTraffic",
            "AdvancedThreatProtectionTraffic",
            "MailTrafficTop",
            "SpoofMailReport",
            "MailTrafficPolicy",
            "MailFilterList",
            "MailTrafficSummary",
            "MailDetailSpam",
            "MailDetailMalware",
            "MailDetailTransportRule",
            "MessageTrace",
            "MessageTraceDetail",
            "UrlTrace",
            "MxRecordReport",
            "MobileDeviceDashboardSummaryReport",
            "OutboundConnectorReport",
            "ServiceDeliveryReport",
            "SPOSkyDriveProDeployedWeekly",
            "SPOSkyDriveProDeployedMonthly",
            "SPOSkyDriveProStorageWeekly",
            "SPOSkyDriveProStorageMonthly",
            "SPOTenantStorageMetricDaily",
            "SPOTenantStorageMetricWeekly",
            "SPOTenantStorageMetricMonthly",
            "SPOTeamSiteDeployedWeekly",
            "SPOTeamSiteDeployedMonthly",
            "SPOTeamSiteStorageWeekly",
            "SPOTeamSiteStorageMonthly",
            "SPOActiveUserDaily",
            "SPOActiveUserWeekly",
            "SPOActiveUserMonthly",
            "PartnerCustomerUser",
            "ScorecardMetrics",
            "ScorecardClientOutlook",
            "ScorecardClientOS",
            "ScorecardClientDevice",
            "LicenseVsUsageSummary",
            "DlpDetectionsReport",
            "DlpDetailReport",
            "DlpOverrideDetailReport",
            "DlpOverridesReport",
            "DeviceCompliancePolicyInventory",
            "DeviceComplianceSummaryReport",
            "DeviceComplianceUserInventory",
            "DeviceComplianceUserReport",
            "DeviceComplianceReportDate",
            "DeviceComplianceDetailsReport"

        };
    }
}
