using System.Text;
using NUnit.Framework;

namespace HAProxy.StreamProcessingOffload.Agent.Tests
{
    [TestFixture]
    public class FragmentCatalogueTests
    {
        [Test]
        public void Push_WhenCalledWithSameStreamIdAndFrameId_ConcatsData()
        {
            // arrange
            var catalogue = new FragmentCatalogue();
            byte[] b1 = Encoding.ASCII.GetBytes("a");
            byte[] b2 = Encoding.ASCII.GetBytes("b");
            byte[] b3 = Encoding.ASCII.GetBytes("c");

            // act
            catalogue.Push(1, 1, b1);
            catalogue.Push(1, 1, b2);
            catalogue.Push(1, 1, b3);

            // assert
            byte[] resultBytes = catalogue.Pop(1, 1);
            string result = Encoding.ASCII.GetString(resultBytes);
            Assert.AreEqual("abc", result);
        }

        [Test]
        public void Push_WhenCalledWithMultipleStreamIdsAndFrameIds_ConcatsDataOnlyFromSameKey()
        {
            // arrange
            var catalogue = new FragmentCatalogue();
            byte[] b1 = Encoding.ASCII.GetBytes("a");
            byte[] b2 = Encoding.ASCII.GetBytes("b");
            byte[] b3 = Encoding.ASCII.GetBytes("c");

            byte[] c1 = Encoding.ASCII.GetBytes("d");
            byte[] c2 = Encoding.ASCII.GetBytes("e");
            byte[] c3 = Encoding.ASCII.GetBytes("f");

            // act
            catalogue.Push(1, 1, b1);
            catalogue.Push(1, 2, c1);
            catalogue.Push(1, 1, b2);
            catalogue.Push(1, 2, c2);
            catalogue.Push(1, 1, b3);
            catalogue.Push(1, 2, c3);

            // assert
            byte[] resultBytes = catalogue.Pop(1, 1);
            string result = Encoding.ASCII.GetString(resultBytes);
            Assert.AreEqual("abc", result);

            byte[] resultBytes2 = catalogue.Pop(1, 2);
            string result2 = Encoding.ASCII.GetString(resultBytes2);
            Assert.AreEqual("def", result2);
        }

        [Test]
        public void Pop_WhenCalled_DeletesKeyAndData()
        {
            // arrange
            var catalogue = new FragmentCatalogue();
            byte[] b1 = Encoding.ASCII.GetBytes("a");
            byte[] b2 = Encoding.ASCII.GetBytes("b");

            // act
            catalogue.Push(1, 1, b1);
            byte[] resultBytes = catalogue.Pop(1, 1); // a
            catalogue.Push(1, 1, b2);

            // assert
            byte[] resultBytes2 = catalogue.Pop(1, 1); // b
            string result2 = Encoding.ASCII.GetString(resultBytes2);
            Assert.AreEqual("b", result2);
        }
    }
}