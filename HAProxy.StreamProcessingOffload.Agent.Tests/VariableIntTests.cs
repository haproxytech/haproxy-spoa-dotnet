using System.Collections;
using NUnit.Framework;
using HAProxy.StreamProcessingOffload.Agent;

namespace HAProxy.StreamProcessingOffload.Agent.Tests
{
    [TestFixture]
    public class VariableIntTests
    {

        [TestCase(1, 1)]
        [TestCase(260, 260)]
        [TestCase(1000, 1000)]
        public void EncodeVariableInt_WhenPositiveNumber_ThenReturnsExpectedVarIntValue(long input, long output)
        {
            // act
            VariableInt result = VariableInt.EncodeVariableInt(input);

            // assert
            Assert.AreEqual(output, result.Value);
        }

        [TestCase(0, "00000000")]
        [TestCase(1, "10000000")]
        [TestCase(1000, "0001111111110100")]
        public void EncodeVariableInt_WhenPositiveNumber_ThenReturnsExpectedVarIntBytes(long input, string output)
        {
            // act
            VariableInt result = VariableInt.EncodeVariableInt(input);

            // assert
            Assert.AreEqual(output, HelperMethods.ToBitString(result.Bytes));
        }

        [Test]
        public void DecodeVariableInt_WhenGivenBuffer_ThenReturnsExpectedValue()
        {
            // arrange
            // 0001111111110100
            BitArray ba = new BitArray(
                new bool[]{ false, false, false, true, true, true, true, true, true, true, true, true, false, true, false, false});

            byte[] buffer = new byte[2];
            ba.CopyTo(buffer, 0);

            // act
            VariableInt result = VariableInt.DecodeVariableInt(buffer);

            // assert
            Assert.AreEqual(1000, result.Value);
        }

        [TestCase(1, 1)]
        [TestCase(250, 2)]
        [TestCase(2290, 3)]
        public void EncodeVariableInt_WhenPositiveNumber_ThenReturnsExpectedVarIntLength(long input, int length)
        {
            // act
            VariableInt result = VariableInt.EncodeVariableInt(input);

            // assert
            Assert.AreEqual(length, result.Length);
        }
    }
}