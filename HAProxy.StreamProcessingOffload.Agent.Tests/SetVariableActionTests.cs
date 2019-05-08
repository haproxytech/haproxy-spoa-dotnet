using System.Collections;
using HAProxy.StreamProcessingOffload.Agent.Actions;
using NUnit.Framework;

namespace HAProxy.StreamProcessingOffload.Agent.Tests
{
    [TestFixture]
    public class SetVariableActionTests
    {
        [Test]
        public void Bytes_WhenSetVariable_ReturnsExpectedResult()
        {
            // arrange
            var action = new SetVariableAction(VariableScope.Session, "ip_score", new TypedData(DataType.Int32, 5));
            var ba = new BitArray(
                new bool[]{
                    true, false, false, false, false, false, false, false, // 10000000
                    true, true, false, false, false, false, false, false, // 11000000
                    true, false, false, false, false, false, false, false, // 10000000
                    false, false, false, true, false, false, false, false, // 00010000
                    true, false, false, true, false, true, true, false, // 10010110
                    false, false, false, false, true, true, true, false, // 00001110
                    true, true, true, true, true, false, true, false, // 11111010
                    true, true, false, false, true, true, true, false, // 11001110
                    true, true, false, false, false, true, true, false, // 11000110
                    true, true, true, true, false, true, true, false, // 11110110
                    false, true, false, false, true, true, true, false, // 01001110
                    true, false, true, false, false, true, true, false, // 10100110
                    false, true, false, false, false, false, false, false, // 01000000
                    true, false, true, false, false, false, false, false // 10100000
                });

            byte[] expected = new byte[14];
            ba.CopyTo(expected, 0);

            // assert
            Assert.AreEqual(
                HelperMethods.ToBitString(expected),
                HelperMethods.ToBitString(action.Bytes));
        }
    }
}