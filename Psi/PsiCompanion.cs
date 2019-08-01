using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Companions;

namespace KQML
{
    public class PsiCompanion<TIn,TOut>: Netonian 
    {

        public Receiver<TIn> PIn { get; }

        public Emitter<TOut> POut { get; }

        public PsiCompanion(Pipeline pipeline)
        {
            this.POut = pipeline.CreateEmitter<TOut>(this, nameof(this.Out));
            this.PIn = pipeline.CreateReceiver<TIn>(this, this.Receive, nameof(this.In));

            // need to call super.Start() somewhere, probably once the pipeline is started
        }

        protected virtual void Receive(TIn data, Envelope envelope)
        {
            var inputString = data.ToString();
            var performative = new KQMLPerformative(inputString);
            Send(performative);
        }

        public override void ReceiveAchieve(KQMLPerformative msg, KQMLObject content)
        {
            POut.Post((TOut)content, DateTime.Now);
        }

        public override void ReceiveTell(KQMLPerformative msg, KQMLObject content)
        {
            Console.Out.WriteLine(content.ToString());
            POut.Post((TOut)content, DateTime.Now);
        }

        /// <summary>
        /// Generates content value for achieve using a specified action and its arguments
        /// </summary>
        /// <param name="action">Action to be achieved</param>
        /// <param name="args">arguments for action</param>
        /// <returns></returns>
        public KQMLList MakeAction(string action, List<object> args)
        {
            KQMLList contentList = new KQMLList("task");
            KQMLList combinedAction = new KQMLList(args);
            combinedAction.Push(action);
            contentList.Set("action", combinedAction);
            // Console.WriteLine(contentList);
            return contentList;

        }

        

        
    }
}
