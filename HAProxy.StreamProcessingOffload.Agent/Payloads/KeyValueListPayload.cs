using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HAProxy.StreamProcessingOffload.Agent.Payloads
{
    public class KeyValueListPayload : Payload
    {
        public KeyValueListPayload() : base(PayloadType.KeyValueList)
        {
            this.KeyValueItems = new Dictionary<string, TypedData>();
        }

        public IDictionary<string, TypedData> KeyValueItems { get; private set; }

        public override byte[] Bytes
        {
            get
            {
                var bytes = new List<byte>();

                foreach (KeyValuePair<string, TypedData> kv in this.KeyValueItems)
                {
                    VariableInt lengthOfName = VariableInt.EncodeVariableInt(kv.Key.Length);
                    bytes.AddRange(lengthOfName.Bytes);
                    bytes.AddRange(System.Text.Encoding.ASCII.GetBytes(kv.Key));
                    bytes.AddRange(kv.Value.Bytes);
                }

                return bytes.ToArray();
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            foreach (KeyValuePair<string, TypedData> kv in this.KeyValueItems)
            {
                sb.AppendLine(string.Format("{0} = {1}", kv.Key, kv.Value.Value));
            }

            return sb.ToString();
        }

        internal override void Parse(byte[] buffer, ref int offset)
        {
            while (offset < buffer.Length)
            {
                // key-value format: [length-of-keyname(varint)][keyname][datatype][value]
                VariableInt keyNameLength = VariableInt.DecodeVariableInt(buffer.Skip(offset).ToArray());
                offset += keyNameLength.Length;

                string keyname = Encoding.ASCII.GetString(buffer, offset, (int)keyNameLength.Value);
                offset += keyname.Length;

                TypedData data = TypedDataParser.ParseNext(buffer, ref offset);
                this.KeyValueItems.Add(keyname, data);
            }
        }
    }
}