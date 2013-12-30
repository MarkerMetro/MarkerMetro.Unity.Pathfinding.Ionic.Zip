using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;

namespace Pathfinding.Ionic.Zip.Unity.Tests
{
    [TestClass]
    public class ZipFileTests
    {
        private const string STR_TestBin = "test.bin";
        [TestMethod]
        public void SaveAndRead_DNF()
        {
            using (var ms = new MemoryStream())
            {
                using (var zipFile = new ZipFile())
                {
                    zipFile.Save(ms);
                }

                ms.Position = 0;

                using (var readFile = ZipFile.Read(ms)) { }
            }
        }

        [TestMethod]
        public void TestZip_ShouldContinEntry_TestTxt()
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream(@"Pathfinding.Ionic.Zip.Unity.Tests.Files.test.zip"))
            {
                using (var zip = ZipFile.Read(stream))
                {
                    var actual = zip.ContainsEntry("test.txt");

                    Assert.IsTrue(actual);
                }
            }
        }

        [TestMethod]
        public void TestZip_ShouldContinExactText()
        {
            using (var stream = GetType().Assembly.GetManifestResourceStream(@"Pathfinding.Ionic.Zip.Unity.Tests.Files.test.zip"))
            {
                using (var zip = ZipFile.Read(stream))
                {
                    var entry = zip["test.txt"];

                    using (var ms = new MemoryStream())
                    {
                        entry.Extract(ms);
                        ms.Position = 0;

                        var actual = new StreamReader(ms).ReadToEnd();

                        Assert.AreEqual("this is a test", actual);
                    }
                }
            }
        }

        [TestMethod]
        public void EmptyZip_AddEntry_SaveAndReRead_ShouldHaveSameContent()
        {
            var data = new byte[] { 1, 2, 3, 4 };

            using (var stream = new MemoryStream())
            {
                using (var zip = new ZipFile())
                {
                    zip.AddEntry(STR_TestBin, data);
                    zip.Save(stream);

                    stream.Position = 0;
                    using (var fs = new FileStream(@"C:\Users\Ivan.z\Documents\test.bin.zip", FileMode.OpenOrCreate))
                    {
                        fs.Position = 0;

                        stream.WriteTo(fs);
                    }
                }

                stream.Position = 0;

                using (var zip = ZipFile.Read(stream))
                {
                    using (var ms = new MemoryStream())
                    {
                        zip[STR_TestBin].Extract(ms);

                        var actual = ms.ToArray();

                        CollectionAssert.AreEquivalent(data, actual);
                    }
                }
            }
        }
    }
}
