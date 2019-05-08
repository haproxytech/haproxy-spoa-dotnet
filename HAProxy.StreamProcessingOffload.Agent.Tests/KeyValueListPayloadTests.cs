using System.Collections;
using HAProxy.StreamProcessingOffload.Agent.Payloads;
using NUnit.Framework;

namespace HAProxy.StreamProcessingOffload.Agent.Tests
{
    [TestFixture]
    public class KeyValueListPayloadTests
    {
        [Test]
        public void Parse_WhenBufferContainsStringValue_AddsValueToKeyValueItems()
        {
            // arrange
            var kvListPayload = new KeyValueListPayload();
            int offset = 0;

            var ba = new BitArray(
                new bool[]{
                    false, true, false, false, true, false, false, false, // 01001000
                    true, true, false, false, true, true, true, false, // 11001110
                    true, false, true, false, true, true, true, false, // 10101110
                    false, false, false, false, true, true, true, false, // 00001110
                    false, false, false, false, true, true, true, false, // 00001110
                    true, true, true, true, false, true, true, false, // 11110110
                    false, true, false, false, true, true, true, false, // 01001110
                    false, false, true, false, true, true, true, false, // 00101110
                    true, false, true, false, false, true, true, false, // 10100110
                    false, false, true, false, false, true, true, false, // 00100110
                    true, false, true, true, false, true, false, false, // 10110100
                    false, true, true, false, true, true, true, false, // 01101110
                    true, false, true, false, false, true, true, false, // 10100110
                    false, true, false, false, true, true, true, false, // 01001110
                    true, true, false, false, true, true, true, false, // 11001110
                    true, false, false, true, false, true, true, false, // 10010110
                    true, true, true, true, false, true, true, false, // 11110110
                    false, true, true, true, false, true, true, false, // 01110110
                    true, true, false, false, true, true, true, false, // 11001110
                    false, false, false, true, false, false, false, false, // 00010000
                    true, true, false, false, false, false, false, false, // 11000000
                    false, true, false, false, true, true, false, false, // 01001100
                    false, true, true, true, false, true, false, false, // 01110100
                    false, false, false, false, true, true, false, false // 00001100
                });

            byte[] buffer = new byte[24];
            ba.CopyTo(buffer, 0);

            // act
            kvListPayload.Parse(buffer, ref offset);

            // assert
            Assert.IsTrue(kvListPayload.KeyValueItems.ContainsKey("supported-versions"));
            Assert.AreEqual("2.0", kvListPayload.KeyValueItems["supported-versions"].Value);
        }

        [Test]
        public void Bytes_WhenContainsStringValue_ReturnsExpectedResult()
        {
            // arrange
            var kvListPayload = new KeyValueListPayload();
            kvListPayload.KeyValueItems.Add("supported-versions", new TypedData(DataType.String, "2.0"));

            var ba = new BitArray(
                new bool[]{
                    false, true, false, false, true, false, false, false, // 01001000
                    true, true, false, false, true, true, true, false, // 11001110
                    true, false, true, false, true, true, true, false, // 10101110
                    false, false, false, false, true, true, true, false, // 00001110
                    false, false, false, false, true, true, true, false, // 00001110
                    true, true, true, true, false, true, true, false, // 11110110
                    false, true, false, false, true, true, true, false, // 01001110
                    false, false, true, false, true, true, true, false, // 00101110
                    true, false, true, false, false, true, true, false, // 10100110
                    false, false, true, false, false, true, true, false, // 00100110
                    true, false, true, true, false, true, false, false, // 10110100
                    false, true, true, false, true, true, true, false, // 01101110
                    true, false, true, false, false, true, true, false, // 10100110
                    false, true, false, false, true, true, true, false, // 01001110
                    true, true, false, false, true, true, true, false, // 11001110
                    true, false, false, true, false, true, true, false, // 10010110
                    true, true, true, true, false, true, true, false, // 11110110
                    false, true, true, true, false, true, true, false, // 01110110
                    true, true, false, false, true, true, true, false, // 11001110
                    false, false, false, true, false, false, false, false, // 00010000
                    true, true, false, false, false, false, false, false, // 11000000
                    false, true, false, false, true, true, false, false, // 01001100
                    false, true, true, true, false, true, false, false, // 01110100
                    false, false, false, false, true, true, false, false // 00001100
                });

            byte[] expected = new byte[24];
            ba.CopyTo(expected, 0);

            // assert
            Assert.AreEqual(
                HelperMethods.ToBitString(expected),
                HelperMethods.ToBitString(kvListPayload.Bytes));
        }
    }
}