using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Rest.Azure;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var adl_helper = new DataLakeHelper();

            var slice_start = DateTime.Now;
            var root_folder = "TestData";
            var name = "Data";


            var file_name = $"{name}.csv";

            var current_dir = $"/{root_folder}/{slice_start.Year}/{slice_start.Month:D2}/{slice_start.Day:D2}/";

            try
            {
                var rows = new List<string>();
                var count = 0;
                var is_file_created = false;

                foreach (var row in SampleData.GetData())
                {
                    rows.Add(row);
                    count++;

                    if (count != 2000) continue;

                    adl_helper.StoreData(current_dir + file_name, rows, is_file_created);

                    if (!is_file_created) is_file_created = true;

                    count = 0;
                    rows = new List<string>();
                }

                if (count <= 0) return;

                adl_helper.StoreData(current_dir + file_name, rows, is_file_created);


            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
