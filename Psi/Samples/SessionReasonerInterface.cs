using Microsoft.Psi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KQML.Samples
{
    class SessionReasonerInterface : PsiCompanion<string, string>

    {
        public SessionReasonerInterface(Pipeline pipeline) : base(pipeline)
        {

        }

        protected override void Receive(string data, Envelope envelope)
        {
            AskAgent("session-reasoner", data);
        }

        public override void ReceiveTell(KQMLPerformative msg, KQMLObject content)
        {
            Console.Out.WriteLine(content.ToString());
            POut.Post(content.ToString(), DateTime.Now);
        }

    }
}
