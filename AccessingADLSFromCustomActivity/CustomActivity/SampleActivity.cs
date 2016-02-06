using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.DataFactories.Models;
using Microsoft.Azure.Management.DataFactories.Runtime;

namespace CustomActivity
{
    public class SampleActivity : IDotNetActivity
    {
        public IDictionary<string, string> Execute(IEnumerable<LinkedService> linkedServices, IEnumerable<Dataset> datasets, Activity activity, IActivityLogger logger)
        {
            var adl_helper = new DataLakeHelper();
            var sf_helper = new SalesForceHelper();

            var extendedProperties = ((DotNetActivity)activity.TypeProperties).ExtendedProperties;

            logger.Write("######ExtendedProperties Begin######");
            foreach (KeyValuePair<string, string> entry in extendedProperties)
            {
                logger.Write($"<key:{entry.Key}> <value:{entry.Value}>");
            }
            logger.Write("######ExtendedProperties End######");

            //Extended Properties
            var slice_start = DateTime.Parse(extendedProperties["SliceStart"]);
            var root_folder = extendedProperties["RootFolder"];
            var name = extendedProperties["FileName"];


            var file_name = $"{name}.csv";

            var current_dir = $"/{root_folder}/{slice_start.Year}/{slice_start.Month:D2}/{slice_start.Day:D2}/";

            try
            {
                var rows = new List<string>();
                var count = 0;
                var is_file_created = false;

                foreach (var row in sf_helper.GetLeads())
                {
                    rows.Add(row.ToString());
                    count++;

                    if (count != 2000) continue;

                    adl_helper.StoreData(current_dir + file_name, rows, is_file_created);

                    if (!is_file_created) is_file_created = true;

                    count = 0;
                    rows = new List<string>();
                }

                if (count <= 0) return new Dictionary<string, string>();

                adl_helper.StoreData(current_dir + file_name, rows, is_file_created);


            }
            catch (Exception e)
            {
                logger.Write(e.Message);
                logger.Write(e.InnerException.Message);
                throw;
            }

            return new Dictionary<string, string>();
        }
    }
}
