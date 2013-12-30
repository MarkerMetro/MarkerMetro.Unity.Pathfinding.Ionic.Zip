using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Pathfinding.Ionic.Zip.Metro.Tests
{
    [TestClass]
    public class ZipFileTests
    {
        private const string STR_TestBin = "test.bin";
        [TestMethod]
        public void Metro_SaveAndRead_DNF()
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
        public async Task Metro_TestZip_ShouldContinEntry_TestTxt()
        {
            using (var stream = await Package.Current.InstalledLocation.OpenStreamForReadAsync(@"Files\test.zip"))
            {
                using (var zip = ZipFile.Read(stream))
                {
                    var actual = zip.ContainsEntry("test.txt");

                    Assert.IsTrue(actual);
                }
            }
        }

        [TestMethod]
        public async Task Metro_TestZip_ShouldContinExactText()
        {
            using (var stream = await Package.Current.InstalledLocation.OpenStreamForReadAsync(@"Files\test.zip"))
            {
                using (var zip = ZipFile.Read(stream))
                {
                    var entry = zip["test.txt"];

                    using(var ms = new MemoryStream())
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
        public void Metro_EmptyZip_AddEntry_SaveAndReRead_ShouldHaveSameContent()
        {
            var data = new byte[] { 1, 2, 3, 4 };

            using (var stream = new MemoryStream())
            {
                using (var zip = new ZipFile())
                {
                    zip.AddEntry(STR_TestBin, data);
                    zip.Save(stream);
                }

                stream.Position = 0;

                using(var zip = ZipFile.Read(stream))
                {
                    using(var ms = new MemoryStream())
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
