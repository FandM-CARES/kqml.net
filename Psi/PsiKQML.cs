using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Psi;
using Microsoft.Psi.Components;

namespace KQML
{
    public class PsiKQML<TIn,TOut>: KQMLModule 
    {

        public Receiver<TIn> PIn { get; }

        public Emitter<TOut> POut { get; }

        public PsiKQML(Pipeline pipeline)
        {
            this.POut = pipeline.CreateEmitter<TOut>(this, nameof(this.Out));
            this.PIn = pipeline.CreateReceiver<TIn>(this, this.Receive, nameof(this.In));

            // need to call super.Start() somewhere, probably once the pipeline is started
        }

        protected void Receive(TIn data, Envelope envelope)
        {
            var inputString = data.ToString();
            var performative = new KQMLPerformative(inputString);
            Send(performative);
        }

        public override void ReceiveAchieve(KQMLPerformative msg, KQMLObject content)
        {
            POut.Post((TOut)content, DateTime.Now);
        }

        static void Main(string[] args)
        {
            using (Pipeline pipeline = new Pipeline())
            {
                test = new PsiKQML<String, String>(pipeline);
            }
        }
    }
}
