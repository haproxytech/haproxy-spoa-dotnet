using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAProxy.StreamProcessingOffload.Agent.Payloads
{
    public class ListOfMessagesPayload : Payload
    {
        public ListOfMessagesPayload() : base(PayloadType.ListOfMessages)
        {
            this.Messages = new List<SpoeMessage>();
        }

        public IList<SpoeMessage> Messages { get; set; }

        public override byte[] Bytes
        {
            get
            {
                var bytes = new List<byte>();

                foreach (var message in this.Messages)
                {
                    VariableInt lengthOfName = VariableInt.EncodeVariableInt(message.Name.Length);
                    bytes.AddRange(lengthOfName.Bytes);
                    bytes.AddRange(System.Text.Encoding.ASCII.GetBytes(message.Name));

                    bytes.Add((byte)message.Args.Count);

                    foreach (var arg in message.Args)
                    {
                        VariableInt lengthOfKey = VariableInt.EncodeVariableInt(arg.Key.Length);
                        bytes.AddRange(lengthOfKey.Bytes);
                        bytes.AddRange(System.Text.Encoding.ASCII.GetBytes(arg.Key));
                        bytes.AddRange(arg.Value.Bytes);
                    }
                }

                return bytes.ToArray();
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (var message in this.Messages)
            {
                sb.AppendLine(string.Format("(message) {0}", message.Name));

                foreach (var arg in message.Args)
                {
                    sb.AppendLine(string.Format("(arg) {0} = {1}", arg.Key, arg.Value.Value));
                }
            }

            return sb.ToString();
        }

        internal override void Parse(byte[] buffer, ref int offset)
        {
            while (offset < buffer.Length)
            {
                VariableInt messageNameLength = VariableInt.DecodeVariableInt(buffer.Skip(offset).ToArray());
                offset += messageNameLength.Length;

                string messageName = Encoding.ASCII.GetString(buffer, offset, (int)messageNameLength.Value);
                offset += messageName.Length;

                byte numberOfArgs = buffer.Skip(offset).Take(1).First();
                offset++;

                var message = new SpoeMessage(messageName);

                for (byte i = 0; i < numberOfArgs; i++)
                {
                    VariableInt keyNameLength = VariableInt.DecodeVariableInt(buffer.Skip(offset).ToArray());
                    offset += keyNameLength.Length;

                    string keyname = Encoding.ASCII.GetString(buffer, offset, (int)keyNameLength.Value);
                    offset += keyname.Length;

                    TypedData data = TypedDataParser.ParseNext(buffer, ref offset);
                    message.Args.Add(keyname, data);
                }

                this.Messages.Add(message);
            }
        }
    }
}