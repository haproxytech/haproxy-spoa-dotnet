using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace HAProxy.StreamProcessingOffload.Agent
{
    internal class TypedDataParser
    {
        /// <summary>
        /// Parses the typed data object from the given buffer at the given offset.
        /// </summary>
        /// <returns>The typed data object from the buffer</returns>
        public static TypedData ParseNext(byte[] buffer, ref int offset)
        {
            byte dataTypeByte = buffer.Skip(offset).First();
            var dataType = dataTypeByte & 0x0F;
            var flags = dataTypeByte >> 4;
            offset++;

            switch ((DataType)dataType)
            {
                case DataType.Binary:
                    return TypedDataParser.ParseBinary(buffer.Skip(offset).ToArray(), ref offset);
                case DataType.Boolean:
                    BitArray bits = new BitArray(new byte[]{ (byte)flags });
                    return new TypedData(DataType.Boolean, bits[0]);
                case DataType.Int32:
                    return TypedDataParser.ParseInt32(buffer.Skip(offset).ToArray(), ref offset);
                case DataType.Int64:
                    return TypedDataParser.ParseInt64(buffer.Skip(offset).ToArray(), ref offset);
                case DataType.Ipv4:
                    return TypedDataParser.ParseIpv4(buffer.Skip(offset).ToArray(), ref offset);
                case DataType.Ipv6:
                    return TypedDataParser.ParseIpv6(buffer.Skip(offset).ToArray(), ref offset);
                case DataType.Null:
                    return TypedDataParser.ParseNull(buffer.Skip(offset).ToArray(), ref offset);
                case DataType.String:
                    return TypedDataParser.ParseString(buffer.Skip(offset).ToArray(), ref offset);
                case DataType.Uint32:
                    return TypedDataParser.ParseUint32(buffer.Skip(offset).ToArray(), ref offset);
                case DataType.Uint64:
                    return TypedDataParser.ParseUint64(buffer.Skip(offset).ToArray(), ref offset);
                default:
                    throw new ApplicationException(string.Format("Unable to parse data type: {0}", dataType));
            }
        }

        private static TypedData ParseBinary(byte[] buffer, ref int offset)
        {
            VariableInt lengthOfBinary = VariableInt.DecodeVariableInt(buffer);
            int length = (int)lengthOfBinary.Value;
            int lengthOfLength = lengthOfBinary.Length;

            // if the data is larger than the max-frame-size, and fragmentation
            // is supported, then the data may be fragmented into chunks.
            // Check if the size of the data exceeds the space left in the buffer.
            int remainingBufferSize = buffer.Skip(lengthOfLength).Count();
            if (remainingBufferSize < length)
            {
                length = remainingBufferSize;
            }

            string value = Convert.ToBase64String(buffer.Skip(lengthOfBinary.Length).Take((int)lengthOfBinary.Value).ToArray());
            offset = offset + lengthOfLength +  length;
            return new TypedData(DataType.Binary, value);
        }

        private static TypedData ParseBoolean(byte[] buffer, ref int offset)
        {
            BitArray bits = new BitArray(buffer.Take(1).ToArray());
            bool value = bits[0];
            offset++;
            return new TypedData(DataType.Boolean, value);
        }

        private static TypedData ParseInt32(byte[] buffer, ref int offset)
        {
            VariableInt value = VariableInt.DecodeVariableInt(buffer);
            offset += value.Length;
            return new TypedData(DataType.Int32, (int)value.Value);
        }

        private static TypedData ParseInt64(byte[] buffer, ref int offset)
        {
            VariableInt value = VariableInt.DecodeVariableInt(buffer);
            offset += value.Length;
            return new TypedData(DataType.Int64, value.Value);
        }

        private static TypedData ParseIpv4(byte[] buffer, ref int offset)
        {
            var ipAddress = new StringBuilder();

            for (int i = 0; i < 4; i++)
            {
                byte octet = buffer.Skip(i).Take(1).First();
                ipAddress.Append(octet);
                ipAddress.Append(".");
            }

            offset += 4;
            return new TypedData(DataType.Ipv4, ipAddress.ToString().TrimEnd('.'));
        }

        private static TypedData ParseIpv6(byte[] buffer, ref int offset)
        {
            var ipAddress = new StringBuilder();

            for (int i = 0; i < 16; i++)
            {
                byte hextet = buffer.Skip(i).Take(1).First();
                ipAddress.Append(hextet.ToString("X2"));

                if (i % 2 != 0)
                {
                    ipAddress.Append(":");
                }
            }

            offset += 16;
            return new TypedData(DataType.Ipv6, ipAddress.ToString().TrimEnd(':'));
        }

        private static TypedData ParseNull(byte[] buffer, ref int offset)
        {
            return new TypedData(DataType.Null, null);
        }

        private static TypedData ParseString(byte[] buffer, ref int offset)
        {
            VariableInt lengthOfString = VariableInt.DecodeVariableInt(buffer);
            int length = (int)lengthOfString.Value;
            int lengthOfLength = lengthOfString.Length;

            // if the data is larger than the max-frame-size, and fragmentation
            // is supported, then the data may be fragmented into chunks.
            // Check if the size of the data exceeds the space left in the buffer.
            int remainingBufferSize = buffer.Skip(lengthOfLength).Count();
            if (remainingBufferSize < length)
            {
                length = remainingBufferSize;
            }

            string value = System.Text.Encoding.ASCII.GetString(buffer, lengthOfLength, length);
            offset = offset + lengthOfLength +  length;
            return new TypedData(DataType.String, value);
        }

        private static TypedData ParseUint32(byte[] buffer, ref int offset)
        {
            VariableInt value = VariableInt.DecodeVariableUnsignedInt(buffer);
            offset += value.Length;
            return new TypedData(DataType.Uint32, (uint)value.UnsignedValue);
        }

        private static TypedData ParseUint64(byte[] buffer, ref int offset)
        {
            VariableInt value = VariableInt.DecodeVariableUnsignedInt(buffer);
            offset += value.Length;
            return new TypedData(DataType.Uint64, value.UnsignedValue);
        }
    }
}