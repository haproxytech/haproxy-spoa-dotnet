using NUnit.Framework;

namespace HAProxy.StreamProcessingOffload.Agent.Tests
{
    [TestFixture]
    public class SpoeMessageTests
    {
        private SpoeMessage message;

        [SetUp]
        public void Setup()
        {
            this.message = new SpoeMessage("test");
        }

        [Test]
        public void Name_WhenSetInConstructor_ReturnsValue()
        {
            // act
            var name = this.message.Name;

            // assert
            Assert.AreEqual("test", name);
        }

        [Test]
        public void Args_WhenArgAdded_ReturnsArg()
        {
            // arrange
            this.message.Args.Add("testkey", new TypedData(DataType.String, "testData"));

            // act
            var arg = this.message.Args["testkey"];

            // assert
            Assert.AreEqual("testData", arg.Value);
        }
    }
}