using System.Collections;
using NUnit.Framework;

namespace HAProxy.StreamProcessingOffload.Agent.Tests
{
    [TestFixture]
    public class  MetadataFlagsTests
    {
        [Test]
        public void Bytes_WhenFinIsTrue_ReturnsExpectedResult()
        {
            // arrange
            // 00000000 00000000 00000000 10000000
            BitArray ba = new BitArray(
                new bool[]{
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    true, false, false, false, false, false, false, false });

            byte[] expected = new byte[4];
            ba.CopyTo(expected, 0);
            var expectedBytesString = HelperMethods.ToBitString(expected);

            // act
            var flags = new MetadataFlags();
            flags.Fin = true;

            // assert
            Assert.AreEqual(expectedBytesString, HelperMethods.ToBitString(flags.Bytes));
        }

        [Test]
        public void Bytes_WhenAbortIsTrue_ReturnsExpectedResult()
        {
            // arrange
            // 00000000 00000000 00000000 01000000
            BitArray ba = new BitArray(
                new bool[]{
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, false, false, false, false, false, false, false,
                    false, true, false, false, false, false, false, false });

            byte[] expected = new byte[4];
            ba.CopyTo(expected, 0);
            var expectedBytesString = HelperMethods.ToBitString(expected);

            // act
            var flags = new MetadataFlags();
            flags.Abort = true;

            // assert
            Assert.AreEqual(expectedBytesString, HelperMethods.ToBitString(flags.Bytes));
        }
    }
}