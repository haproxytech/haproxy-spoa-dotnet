using System;
using System.Collections;
using NUnit.Framework;
using HAProxy.StreamProcessingOffload.Agent;

namespace HAProxy.StreamProcessingOffload.Agent.Tests
{
    [TestFixture]
    public class TypedDataParserTests
    {
        [Test]
        public void ParseNext_WhenBufferContainsBinary_ReturnsExpectedTypedDataTypeAndValue()
        {
            // arrange
            int offset = 0;

            // act
            // 10010000 11100000 01100110 11110110 11110110 10111100 01000110 10000110 01001110
            BitArray ba = new BitArray(
                new bool[]{ 
                    true, false, false, true,  false, false, false, false,
                    true, true,  true,  false, false, false, false, false,
                    false, true, true,  false, false, true,  true,  false,
                    true, true,  true,  true,  false, true,  true,  false,
                    true, true,  true,  true,  false, true,  true,  false,
                    true, false, true,  true,  true,  true,  false, false,
                    false, true, false, false, false, true,  true,  false,
                    true,  false,false, false, false, true,  true,  false,
                    false, true, false, false, true,  true,  true,  false
                });
            byte[] buffer = new byte[9];
            ba.CopyTo(buffer, 0);

            TypedData result = TypedDataParser.ParseNext(buffer, ref offset);

            // assert
            Assert.AreEqual(DataType.Binary, result.Type);
            Assert.AreEqual("Zm9vPWJhcg==", result.ToString()); // base64 foo=bar
        }
        
        [Test]
        public void ParseNext_WhenBufferContainsBoolean_ReturnsExpectedTypedDataTypeAndValue()
        {
            // arrange
            int offset = 0;

            // 10001000
            BitArray ba = new BitArray(
                new bool[]{ true, false, false, false, true, false, false, false });
            byte[] buffer = new byte[4];
            ba.CopyTo(buffer, 0);

            // act
            TypedData result = TypedDataParser.ParseNext(buffer, ref offset);

            // assert
            Assert.AreEqual(DataType.Boolean, result.Type);
            Assert.AreEqual(true, result.Value);
        }

        [Test]
        public void ParseNext_WhenBufferContainsInt32_ReturnsExpectedTypedDataTypeAndValue()
        {
            // arrange
            int offset = 0;

            // 01000000 00111111 00001111 01100000
            BitArray ba = new BitArray(
                new bool[]{
                    false, true, false, false, false, false, false, false,
                    false, false, true, true, true, true, true, true,
                    false, false, false, false, true, true, true, true,
                    false, true, true, false, false, false, false, false
                     });

            byte[] buffer = new byte[4];
            ba.CopyTo(buffer, 0);

            // act
            TypedData result = TypedDataParser.ParseNext(buffer, ref offset);

            // assert
            Assert.AreEqual(DataType.Int32, result.Type);
            Assert.AreEqual(16380, result.Value);
        }

        [Test]
        public void ParseNext_WhenBufferContainsUnsignedInt32_ReturnsExpectedTypedDataTypeAndValue()
        {
            // arrange
            int offset = 0;

            // 11000000 00111111 00001111 01100000
            BitArray ba = new BitArray(
                new bool[]{
                    true, true, false, false, false, false, false, false,
                    false, false, true, true, true, true, true, true,
                    false, false, false, false, true, true, true, true,
                    false, true, true, false, false, false, false, false
                     });

            byte[] buffer = new byte[4];
            ba.CopyTo(buffer, 0);

            // act
            TypedData result = TypedDataParser.ParseNext(buffer, ref offset);

            // assert
            Assert.AreEqual(DataType.Uint32, result.Type);
            Assert.AreEqual(16380, result.Value);
        }

        [Test]
        public void ParseNext_WhenBufferContainsIPv4_ReturnsExpectedTypedDataTypeAndValue()
        {
            // arrange
            int offset = 0;

            // 01100000 10000000 10000000 10000000 10000000
            BitArray ba = new BitArray(
                new bool[]{
                    false, true, true, false, false, false, false, false,
                    true, false, false, false, false, false, false, false,
                    true, false, false, false, false, false, false, false,
                    true, false, false, false, false, false, false, false,
                    true, false, false, false, false, false, false, false,
                 });
            byte[] buffer = new byte[5];
            ba.CopyTo(buffer, 0);

            // act
            TypedData result = TypedDataParser.ParseNext(buffer, ref offset);

            // assert
            Assert.AreEqual(DataType.Ipv4, result.Type);
            Assert.AreEqual("1.1.1.1", result.Value);
        }

        [Test]
        public void ParseNext_WhenBufferContainsIPv6_ReturnsExpectedTypedDataTypeAndValue()
        {
            // arrange
            int offset = 0;

            // 11100000 00111111 00000000 00000000 00000000 00000000 00000000 00000000 00000000
            // 00000000 00000000 00000000 00000000 00000000 00000000 00000000 00000100
            BitArray ba = new BitArray(
                new bool[]{
                    true, true, true, false, false, false, false, false,
                    false, false, true, true, true, true, true, true,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, true, false, false
                 });
            byte[] buffer = new byte[17];
            ba.CopyTo(buffer, 0);

            // act
            TypedData result = TypedDataParser.ParseNext(buffer, ref offset);

            // assert
            Assert.AreEqual(DataType.Ipv6, result.Type);
            Assert.AreEqual("FC00:0000:0000:0000:0000:0000:0000:0020", result.Value);
        }

        [Test]
        public void ParseNext_WhenBufferContainsUnsignedString_ReturnsExpectedTypedDataTypeAndValue()
        {
            // arrange
            int offset = 0;

            // 00010000 11000000 10000110 01000110 11000110
            BitArray ba = new BitArray(
                new bool[]{
                    false, false, false, true, false, false, false, false,
                    true, true, false, false, false, false, false, false,
                    true, false, false, false, false, true, true, false,
                    false, true, false, false, false, true, true, false,
                    true, true, false, false, false, true, true, false
                     });

            byte[] buffer = new byte[5];
            ba.CopyTo(buffer, 0);

            // act
            TypedData result = TypedDataParser.ParseNext(buffer, ref offset);

            // assert
            Assert.AreEqual(DataType.String, result.Type);
            Assert.AreEqual("abc", result.Value);
        }
    }
}