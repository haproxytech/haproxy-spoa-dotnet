using System.Collections;
using System.Linq;
using HAProxy.StreamProcessingOffload.Agent.Payloads;
using NUnit.Framework;

namespace HAProxy.StreamProcessingOffload.Agent.Tests
{
    [TestFixture]
    public class ListOfMessagesPayloadTests
    {
        [Test]
        public void Parse_WhenBufferContainsMessage_AddsMessageToList()
        {
            // arrange
            var messagesPayload = new ListOfMessagesPayload();
            int offset = 0;
            var ba = new BitArray(
                new bool[]{
                    true, true, true, true, false, false, false, false, // 11110000
                    true, true, false, false, false, true, true, false, // 11000110
                    false, false, false, true, false, true, true, false, // 00010110
                    true, false, true, false, false, true, true, false, // 10100110
                    true, true, false, false, false, true, true, false, // 11000110
                    true, true, false, true, false, true, true, false, // 11010110
                    true, false, true, true, false, true, false, false, // 10110100
                    true, true, false, false, false, true, true, false, // 11000110
                    false, false, true, true, false, true, true, false, // 00110110
                    true, false, false, true, false, true, true, false, // 10010110
                    true, false, true, false, false, true, true, false, // 10100110
                    false, true, true, true, false, true, true, false, // 01110110
                    false, false, true, false, true, true, true, false, // 00101110
                    true, false, true, true, false, true, false, false, // 10110100
                    true, false, false, true, false, true, true, false, // 10010110
                    false, false, false, false, true, true, true, false, // 00001110
                    true, false, false, false, false, false, false, false, // 10000000
                    false, true, false, false, false, false, false, false, // 01000000
                    true, false, false, true, false, true, true, false, // 10010110
                    false, false, false, false, true, true, true, false, // 00001110
                    false, true, true, false, false, false, false, false, // 01100000
                    false, false, false, false, false, false, true, true, // 00000011
                    false, false, false, true, false, true, false, true, // 00010101
                    false, true, false, false, true, true, false, false, // 01001100
                    true, false, false, false, false, false, false, false// 10000000
                });

            byte[] buffer = new byte[25];
            ba.CopyTo(buffer, 0);

            // act
            messagesPayload.Parse(buffer, ref offset);

            // assert
            SpoeMessage spoeMessage = messagesPayload.Messages.FirstOrDefault();
            Assert.AreEqual("check-client-ip", spoeMessage.Name);
            Assert.AreEqual(1, spoeMessage.Args.Count);
            Assert.AreEqual("ip", spoeMessage.Args.First().Key);
            Assert.AreEqual("192.168.50.1", spoeMessage.Args.First().Value.Value);
        }

        [Test]
        public void Bytes_WhenContainsMessages_ReturnsExpectedResult()
        {
            // arrange
            var messagesPayload = new ListOfMessagesPayload();
            var message = new SpoeMessage("check-client-ip");
            message.Args.Add("ip", new TypedData(DataType.Ipv4, "192.168.50.1"));
            messagesPayload.Messages.Add(message);

            var ba = new BitArray(
                new bool[]{
                    true, true, true, true, false, false, false, false, // 11110000
                    true, true, false, false, false, true, true, false, // 11000110
                    false, false, false, true, false, true, true, false, // 00010110
                    true, false, true, false, false, true, true, false, // 10100110
                    true, true, false, false, false, true, true, false, // 11000110
                    true, true, false, true, false, true, true, false, // 11010110
                    true, false, true, true, false, true, false, false, // 10110100
                    true, true, false, false, false, true, true, false, // 11000110
                    false, false, true, true, false, true, true, false, // 00110110
                    true, false, false, true, false, true, true, false, // 10010110
                    true, false, true, false, false, true, true, false, // 10100110
                    false, true, true, true, false, true, true, false, // 01110110
                    false, false, true, false, true, true, true, false, // 00101110
                    true, false, true, true, false, true, false, false, // 10110100
                    true, false, false, true, false, true, true, false, // 10010110
                    false, false, false, false, true, true, true, false, // 00001110
                    true, false, false, false, false, false, false, false, // 10000000
                    false, true, false, false, false, false, false, false, // 01000000
                    true, false, false, true, false, true, true, false, // 10010110
                    false, false, false, false, true, true, true, false, // 00001110
                    false, true, true, false, false, false, false, false, // 01100000
                    false, false, false, false, false, false, true, true, // 00000011
                    false, false, false, true, false, true, false, true, // 00010101
                    false, true, false, false, true, true, false, false, // 01001100
                    true, false, false, false, false, false, false, false// 10000000
                });

            byte[] expected = new byte[25];
            ba.CopyTo(expected, 0);

            // assert
            Assert.AreEqual(
                HelperMethods.ToBitString(expected),
                HelperMethods.ToBitString(messagesPayload.Bytes));
        }
    }
}