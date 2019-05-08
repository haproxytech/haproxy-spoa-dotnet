using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAProxy.StreamProcessingOffload.Agent.Payloads
{
    public class ListOfActionsPayload : Payload
    {
        public ListOfActionsPayload() : base(PayloadType.ListOfActions)
        {
            this.Actions = new List<SpoeAction>();
        }

        public IList<SpoeAction> Actions { get; set; }

        public override byte[] Bytes
        {
            get
            {
                var bytes = new List<byte>();

                foreach (var action in this.Actions)
                {
                    bytes.AddRange(action.Bytes);
                }

                return bytes.ToArray();
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var action in this.Actions)
            {
                sb.AppendLine(string.Format(action.ToString()));
            }

            return sb.ToString();
        }

        internal override void Parse(byte[] buffer, ref int offset)
        {
            throw new System.NotImplementedException();
        }
    }
}