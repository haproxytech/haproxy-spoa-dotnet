using HAProxy.StreamProcessingOffload.Agent.Frames;
using HAProxy.StreamProcessingOffload.Agent.Payloads;
using NUnit.Framework;

namespace HAProxy.StreamProcessingOffload.Agent.Tests
{
    [TestFixture]
    public class AgentDisconnectFrameTests
    {
        private AgentDisconnectFrame frame;

        [SetUp]
        public void Setup()
        {
            this.frame = new AgentDisconnectFrame(Status.Normal, "Test");
        }

        [Test]
        public void Constructor_StreamIdIsZero()
        {
            // assert
            Assert.AreEqual(0, this.frame.Metadata.StreamId.Value);
        }

        [Test]
        public void Constructor_FrameIdIsZero()
        {
            // assert
            Assert.AreEqual(0, this.frame.Metadata.FrameId.Value);
        }

        [Test]
        public void Constructor_FinIsTrue()
        {
            // assert
            Assert.IsTrue(this.frame.Metadata.Flags.Fin);
        }

        [Test]
        public void Constructor_AbortIsFalse()
        {
            // assert
            Assert.IsFalse(this.frame.Metadata.Flags.Abort);
        }

        [Test]
        public void Constructor_StatusIsExpectedValue()
        {
            // assert
            Assert.AreEqual(0, ((KeyValueListPayload)this.frame.Payload).KeyValueItems["status-code"].Value);
        }

        [Test]
        public void Constructor_MessageIsExpectedValue()
        {
            // assert
            Assert.AreEqual("Test", ((KeyValueListPayload)this.frame.Payload).KeyValueItems["message"].Value);
        }
    }
}