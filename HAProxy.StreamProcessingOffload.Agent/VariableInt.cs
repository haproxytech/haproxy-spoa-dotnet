using System.Collections.Generic;
using System.Linq;

namespace HAProxy.StreamProcessingOffload.Agent
{
    public class VariableInt
    {
        /// <summary>
        /// Initializes a new instance of the VariableInt class.
        /// </summary>
        private VariableInt(long value, int length, byte[] bytes)
        {
            this.Value = value;
            this.Length = length;
            this.Bytes = bytes;
        }

        private VariableInt(ulong value, int length, byte[] bytes)
        {
            this.UnsignedValue = value;
            this.Length = length;
            this.Bytes = bytes;
        }

        /// <summary>
        /// Gets the value of this variable integer, if it is a signed int.
        /// </summary>
        public long Value { get; private set; }

        /// <summary>
        /// Gets the value of this variable integer, if it is a unsigned int.
        /// </summary>
        public ulong UnsignedValue { get; private set; }

        /// <summary>
        /// Gets the number of bytes used to store this variable integer.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Gets the bytes for this variable integer.
        /// </summary>
        public byte[] Bytes { get; private set; }

        /// <summary>
        /// Given a buffer, parses a signed variable-length integer from it.
        /// </summary>
        /// <param name="buffer">The buffer to parse a variable-length integer from</param>
        public static VariableInt DecodeVariableInt(byte[] buffer)
        {
            long value = buffer[0];
            int length = 0;
            byte[] valueBytes = new byte[0];

            if (value < 240)
            {
                length++;
                valueBytes = buffer.Take(length).ToArray();
            }
            else
            {
                int shift = 4;

                do {
                    length++;
                    long nextByte = buffer.Skip(length).First();
                    value += nextByte << shift;
                    shift += 7;
                }
                while (buffer.Skip(length).First() >= 128);

                length++;

                valueBytes = buffer.Take(length).ToArray();
            }

            return new VariableInt(value, length, valueBytes);
        }

        /// <summary>
        /// Given a buffer, parses an unsigned variable-length integer from it.
        /// </summary>
        /// <param name="buffer">The buffer to parse a variable-length integer from</param>
        public static VariableInt DecodeVariableUnsignedInt(byte[] buffer)
        {
            var varint = DecodeVariableInt(buffer);
            return new VariableInt((ulong)varint.Value, varint.Length, varint.Bytes);
        }

        /// <summary>
        /// Given a signed number, converts it to a signed variable-length integer.
        /// </summary>
        /// <param name="value">The number to convert</param>
        public static VariableInt EncodeVariableInt(long value)
        {
            byte[] valueBytes = CalculateBytes(value);
            return new VariableInt(value, valueBytes.Length, valueBytes);
        }

        /// <summary>
        /// Given an unsigned number, converts it to an unsigned variable-length integer.
        /// </summary>
        /// <param name="value">The number to convert</param>
        public static VariableInt EncodeVariableUint(ulong value)
        {
            byte[] valueBytes = CalculateUnsignedIntBytes(value);
            return new VariableInt(value, valueBytes.Length, valueBytes);
        }

        private static byte[] CalculateBytes(long value)
        {
            IList<byte> bytes = new List<byte>();

            if (value < 240)
            {
                bytes.Add((byte)value);
                return bytes.ToArray();
            }

            bytes.Add((byte)(value | 240));
            value = (value - 240) >> 4;

            while (value >= 128)
            {
                bytes.Add((byte)(value | 128));
                value = (value - 128) >> 7;
            }

            bytes.Add((byte)value);
            return bytes.ToArray();
        }

        private static byte[] CalculateUnsignedIntBytes(ulong value)
        {
            return CalculateBytes((long)value);
        }
    }
}