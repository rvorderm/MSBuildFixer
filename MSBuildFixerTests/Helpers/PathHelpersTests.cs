using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.Helpers;

namespace MSBuildFixerTests.Helpers
{
    [TestClass]
    public class PathHelpersTests
    {
        [TestMethod]
        public void AbsolutePaths()
        {
            var from = @"C:\Users\ryan.vordermann\Desktop\file.txt";
            var to = @"C:\Users\ryan.vordermann\file.txt";
            string relativePath = PathHelpers.MakeRelativePath(@from, to);
            Assert.AreEqual(@"..\file.txt", relativePath);
        }

        [TestMethod]
        public void RelativePaths()
        {
            Assert.Fail("This test is not yet working, the relative paths method could use serious improvement.");
            var from = @"..\Desktop\file.txt";
            var to = @"..\file.txt";
            string relativePath = PathHelpers.MakeRelativePath(@from, to);
            Assert.AreEqual(@"..\file.txt", relativePath);
        }
    }
}
