using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace HAProxy.StreamProcessingOffload.Agent
{
    public class TypedData
    {
        /// <summary>
        /// Initializes a new instance of the TypedData class.
        /// </summary>
        /// <param name="type">The type of data</param>
        /// <param name="value">The value of the data</param>
        public TypedData(DataType type, object value)
        {
            this.Type = type;
            this.Value = value;
        }

        /// <summary>
        /// Gets the type of data.
        /// </summary>
        public DataType Type { get; private set; }

        /// <summary>
        /// Gets the value of the data.
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// Gets the bytes for this typed data object.
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                var bytes = new List<byte>();

                // Boolean values are combined with the data type ito a single byte
                if (this.Type == DataType.Boolean)
                {
                    int typeAndValueCombined = Convert.ToByte((bool)this.Value) << 4 | (byte)this.Type;
                    bytes.Add((byte)typeAndValueCombined);
                    return bytes.ToArray();
                }

                bytes.Add((byte)this.Type);

                switch(this.Type)
                {
                    case DataType.Binary:
                        bytes.AddRange(GetBytesForBinaryValue());
                        break;
                    case DataType.Int32:
                        bytes.AddRange(GetBytesForInt32Value());
                        break;
                    case DataType.Int64:
                        bytes.AddRange(GetBytesForInt64Value());
                        break;
                    case DataType.Ipv4:
                        bytes.AddRange(GetBytesForIpv4Value());
                        break;
                    case DataType.Ipv6:
                        bytes.AddRange(GetBytesForIpv6Value());
                        break;
                    case DataType.Null:
                        bytes.AddRange(GetBytesForNullValue());
                        break;
                    case DataType.String:
                        bytes.AddRange(GetBytesForStringValue());
                        break;
                    case DataType.Uint32:
                        bytes.AddRange(GetBytesForUint32Value());
                        break;
                    case DataType.Uint64:
                        bytes.AddRange(GetBytesForUint64Value());
                        break;
                    default:
                        throw new ApplicationException(string.Format("Unimplemented type: {0}", this.Type.ToString()));
                }

                return bytes.ToArray();
            }
        }

        /// <summary>
        /// Gets a string representation of this type data object.
        /// </summary>
        public override string ToString()
        {
            switch (this.Type)
            {
                case DataType.Binary:
                    return (string)this.Value;
                case DataType.Int32:
                    return ((int)this.Value).ToString();
                case DataType.Int64:
                    return ((long)this.Value).ToString();
                case DataType.Ipv4:
                    return (string)this.Value;
                case DataType.Ipv6:
                    return (string)this.Value;
                case DataType.Null:
                    return string.Empty;
                case DataType.String:
                    return (string)this.Value;
                case DataType.Uint32:
                    return ((uint)this.Value).ToString();
                case DataType.Uint64:
                    return ((ulong)this.Value).ToString();
                default:
                    return string.Empty;
            }
        }

        private byte[] GetBytesForBinaryValue()
        {
            var bytes = new List<byte>();
            VariableInt lengthOfValue = VariableInt.EncodeVariableInt(((string)this.Value).Length);
            bytes.AddRange(lengthOfValue.Bytes);
            bytes.AddRange(Convert.FromBase64String((string)this.Value));
            return bytes.ToArray();
        }

        private byte[] GetBytesForInt32Value()
        {
            var bytes = new List<byte>();
            VariableInt value = VariableInt.EncodeVariableInt((int)this.Value);
            bytes.AddRange(value.Bytes);
            return bytes.ToArray();
        }

        private byte[] GetBytesForInt64Value()
        {
            var bytes = new List<byte>();
            VariableInt value = VariableInt.EncodeVariableInt((long)this.Value);
            bytes.AddRange(value.Bytes);
            return bytes.ToArray();
        }

        private byte[] GetBytesForIpv4Value()
        {
            IPAddress address = IPAddress.Parse((string)this.Value);
            return address.GetAddressBytes();
        }

        private byte[] GetBytesForIpv6Value()
        {
            IPAddress address = IPAddress.Parse((string)this.Value);
            return address.GetAddressBytes();
        }

        private byte[] GetBytesForNullValue()
        {
            return new byte[]{};
        }

        private byte[] GetBytesForStringValue()
        {
            var bytes = new List<byte>();
            VariableInt lengthOfValue = VariableInt.EncodeVariableInt(((string)this.Value).Length);
            bytes.AddRange(lengthOfValue.Bytes);
            bytes.AddRange(System.Text.Encoding.ASCII.GetBytes((string)this.Value));
            return bytes.ToArray();
        }

        private byte[] GetBytesForUint32Value()
        {
            var bytes = new List<byte>();
            VariableInt value = VariableInt.EncodeVariableUint((uint)this.Value);
            bytes.AddRange(value.Bytes);
            return bytes.ToArray();
        }

        private byte[] GetBytesForUint64Value()
        {
            var bytes = new List<byte>();
            VariableInt value = VariableInt.EncodeVariableUint((ulong)this.Value);
            bytes.AddRange(value.Bytes);
            return bytes.ToArray();
        }
    }
}