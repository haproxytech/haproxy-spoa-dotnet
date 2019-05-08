using System.Collections;
using NUnit.Framework;

namespace HAProxy.StreamProcessingOffload.Agent.Tests
{
    [TestFixture]
    public class MetadataTests
    {
        [Test]
        public void Bytes_WhenFlagsAndStreamIdAndFrameIdSet_ReturnsExpectedResult()
        {
            // arrange
            var metadata = new Metadata();
            metadata.Flags.Fin = true;
            metadata.StreamId = VariableInt.EncodeVariableInt(1);
            metadata.FrameId = VariableInt.EncodeVariableInt(1);

            // 00000000 00000000 00000000 10000000 10000000 10000000
            BitArray ba = new BitArray(
                new bool[]{
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    true, false, false, false, false, false, false, false,
                    true, false, false, false, false, false, false, false,
                    true, false, false, false, false, false, false, false });

            byte[] expected = new byte[6];
            ba.CopyTo(expected, 0);

            // assert
            Assert.AreEqual(
                HelperMethods.ToBitString(expected),
                HelperMethods.ToBitString(metadata.Bytes));
        }

        [Test]
        public void Parse_WhenBufferContainsMetadata_SetsFlagsAndStreadIdAndFrameId()
        {
            // arrange
            var metadata = new Metadata();
            int offset = 0;

            // 00000000 00000000 00000000 10000000 11010000 10000000 11110000
            BitArray ba = new BitArray(
                new bool[]{
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    true, false, false, false, false, false, false, false,
                    true, true, false, true, false, false, false, false,
                    true, false, false, false, false, false, false, false,
                    true, true, true, true, false, false, false, false
                });

            byte[] buffer = new byte[7];
            ba.CopyTo(buffer, 0);

            // act
            metadata.Parse(buffer, ref offset);

            // assert
            Assert.IsTrue(metadata.Flags.Fin);
            Assert.IsFalse(metadata.Flags.Abort);
            Assert.AreEqual(11, metadata.StreamId.Value);
            Assert.AreEqual(1, metadata.FrameId.Value);
            Assert.AreEqual(6, offset);
        }
    }
}