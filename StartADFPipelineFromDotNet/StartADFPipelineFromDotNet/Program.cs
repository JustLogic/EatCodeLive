using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StartADFPipelineFromDotNet
{
    class Program
    {
        static void Main(string[] args)
        {

            //Do some Job
            //do_work();

            var factory_helper = new DataFactoryHelper();

            factory_helper.StartPipeline("ResourceGroup", "DataFactory", "CopyDataPipeline", DateTime.Now);

            Console.ReadLine();
        }
    }
}
