using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Pathfinding.Ionic.Zip
{
    [ComVisible(true)]
    [Guid("ebc25cf6-9120-4283-b972-0e5520d00004")]
    public class ZipEntry
    {
#if NETFX_CORE
        readonly System.IO.Compression.ZipArchiveEntry entry;

        internal ZipEntry(System.IO.Compression.ZipArchiveEntry entry)
        {
            this.entry = entry;
        }
#else
        readonly ICSharpCode.SharpZipLib.Zip.ZipEntry entry;
        readonly ICSharpCode.SharpZipLib.Zip.ZipFile zipFile;
        internal ZipEntry(ICSharpCode.SharpZipLib.Zip.ZipFile zipFile, ICSharpCode.SharpZipLib.Zip.ZipEntry entry)
        {
            if (zipFile == null)
                throw new ArgumentNullException("zipFile", "zipFile is null.");
            if (entry == null)
                throw new ArgumentNullException("entry", "entry is null.");

            this.zipFile = zipFile;
            this.entry = entry;
        }
#endif

        public void Extract(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "stream is null.");

#if NETFX_CORE
            using (var entryStream = entry.Open())
                entryStream.CopyTo(stream);
#else
            if(!entry.CanDecompress)
                throw new InvalidOperationException();

            using(var entryStream = zipFile.GetInputStream(entry))
            {
                var buffer = new byte[256];
                var read = 0;
                do
                {
                    read = entryStream.Read(buffer, 0, buffer.Length);

                    if (read > 0)
                        stream.Write(buffer, 0, read);
                }
                while (read > 0);
            }
#endif
        }
    }
}
