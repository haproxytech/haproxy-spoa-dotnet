using NUnit.Framework;

namespace HAProxy.StreamProcessingOffload.Agent.Tests
{
    [TestFixture]
    public class TypedDataTests
    {
        [Test]
        public void Bytes_WhenTypeIsString_ReturnsExpectedResult()
        {
            // arrange
            TypedData data = new TypedData(DataType.String, "abc");

            // act
            byte[] bytes = data.Bytes;

            // assert
            Assert.AreEqual("0001000011000000100001100100011011000110", ToBitString(bytes));
        }

        [Test]
        public void Bytes_WhenTypeIsIpv6_ReturnsExpectedResult()
        {
            // arrange
            TypedData data = new TypedData(DataType.Ipv6, "FC00:0000:0000:0000:0000:0000:0000:0020");

            // act
            byte[] bytes = data.Bytes;

            // assert
            Assert.AreEqual("1110000000111111000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000100", ToBitString(bytes));
        }

        [Test]
        public void Bytes_WhenTypeIsIpv4_ReturnsExpectedResult()
        {
            // arrange
            TypedData data = new TypedData(DataType.Ipv4, "1.1.1.1");

            // act
            byte[] bytes = data.Bytes;

            // assert
            Assert.AreEqual("0110000010000000100000001000000010000000", ToBitString(bytes));
        }

        [Test]
        public void Bytes_WhenTypeIsBoolean_ReturnsExpectedResult()
        {
            // arrange
            TypedData data = new TypedData(DataType.Boolean, true);

            // act
            byte[] bytes = data.Bytes;

            // assert
            Assert.AreEqual("10001000", ToBitString(bytes));
        }

        [Test]
        public void Bytes_WhenTypeIsInt32_ReturnsExpectedResult()
        {
            // arrange
            TypedData data = new TypedData(DataType.Int32, 16380);

            // act
            byte[] bytes = data.Bytes;

            // assert
            Assert.AreEqual("01000000001111110000111101100000", ToBitString(bytes));
        }

        [Test]
        public void Bytes_WhenTypeIsUint32_ReturnsExpectedResult()
        {
            // arrange
            TypedData data = new TypedData(DataType.Uint32, (uint)16380);

            // act
            byte[] bytes = data.Bytes;

            // assert
            Assert.AreEqual("11000000001111110000111101100000", ToBitString(bytes));
        }

        [TestCase(true, "true")]
        [TestCase(false, "false")]
        public void ToString_WhenValueIsBoolean_ReturnsStringValue(bool value, string expectedResult)
        {
            // arrange
            TypedData data = new TypedData(DataType.Boolean, value);

            // act
            string result = data.ToString();

            // assert
            Assert.AreEqual(expectedResult, result);
        }

        private static string ToBitString(byte[] buffer)
        {
            var bits = new System.Collections.BitArray(buffer);
            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < bits.Count; i++)
            {
                char c = bits[i] ? '1' : '0';
                sb.Append(c);
            }

            return sb.ToString();
        }
    }
}