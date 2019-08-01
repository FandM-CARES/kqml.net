using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Psi;


namespace KQML.Samples
{
    public class InteractionManagerInterface : PsiCompanion<string, string>
    {
        string Action;
        string Receiver;

        /// <summary>
        /// Initializes a new instance of the class with specified pipline and receiver. 
        /// It sends an achieve with a specified action. 
        /// </summary>
        /// <param name="pipeline">Psi stuff</param>
        /// <param name="action">The action to achieve</param>
        /// <param name="receiver">receiver of messages</param>
        public InteractionManagerInterface(Pipeline pipeline, string action, string receiver) : base(pipeline)
        {
            Action = action;
            Receiver = receiver;
            //Name = "IM-interface";
        }
        protected override void Receive(string data, Envelope envelope)
        {
            AchieveOnAgent(Receiver, MakeAction(Action, new List<object> { data }));
        }

        public override void ReceiveAchieve(KQMLPerformative msg, KQMLObject content) 
        {
            POut.Post(content.ToString(), DateTime.Now);
        }

        public override void ReceiveTell(KQMLPerformative msg, KQMLObject content)
        {
            POut.Post(content.ToString(), DateTime.Now);

        }
    }
}
