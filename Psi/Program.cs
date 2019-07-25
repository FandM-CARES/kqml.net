using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KQML.Samples;
using log4net.Config;
using Microsoft.Psi;

namespace KQML
{
    class Program
    {
        public static void Main(string[] args)
        {
            _ = XmlConfigurator.Configure(new FileInfo("logging.xml"));
            using (Pipeline pipeline = Pipeline.Create())
            {
                var test = new InteractionManagerInterface(pipeline, "processKioskUtterance", "interaction-manager");
                test.POut.Do(p => Console.WriteLine(p));
                var input = Generators.Sequence(pipeline, Input(), TimeSpan.FromMilliseconds(10));
                input.PipeTo(test.PIn);

                pipeline.Run();
                
            }
        }

        private static IEnumerable<String> Input()
        {
            while (true)
            {
                var input = Console.ReadLine();
                yield return input;

                if (input == "q")
                {
                    yield break;
                }
            }
        }
    }
}
