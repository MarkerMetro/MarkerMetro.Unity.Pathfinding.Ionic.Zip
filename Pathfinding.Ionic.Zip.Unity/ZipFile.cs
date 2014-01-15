using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

#if NETFX_CORE
using System.IO.Compression;
#elif SILVERLIGHT
#else
using System.IO.Compression;
#endif

namespace Pathfinding.Ionic.Zip
{
    [ComVisible(true)]
    [Guid("ebc25cf6-9120-4283-b972-0e5520d00005")]
    public class ZipFile :
        //IEnumerable,
        //IEnumerable<ZipEntry>,
     IDisposable
    {
        Stream _stream = null;
#if NETFX_CORE
        ZipArchive _zipArchive = null;
#else
        ICSharpCode.SharpZipLib.Zip.ZipFile _zipFile;
#endif
        public ZipFile()
        {
            _stream = new MemoryStream();
#if NETFX_CORE
            _zipArchive = new ZipArchive(_stream, ZipArchiveMode.Create, true);
#else
            _zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(_stream)
            {
                IsStreamOwner = false,
            };
            _zipFile.BeginUpdate();
#endif
        }

#if NETFX_CORE
        ZipFile(Stream stream, ZipArchive zipArchive)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "stream is null.");
            if (zipArchive == null)
                throw new ArgumentNullException("zipArchive", "zipArchive is null.");

            _stream = stream;
            _zipArchive = zipArchive;
        }
#else
        ZipFile(Stream stream, ICSharpCode.SharpZipLib.Zip.ZipFile zipFile)
        {
            if (zipFile == null)
                throw new ArgumentNullException("zipFile", "zipFile is null.");
            if (stream == null)
                throw new ArgumentNullException("stream", "stream is null.");


            _stream = stream;
            _zipFile = zipFile;
            _zipFile.BeginUpdate();
        }
#endif

        void AssertIsOpen()
        {
            if (_stream == null)
                throw new InvalidOperationException();
#if NETFX_CORE
            if (_zipArchive == null)
                throw new InvalidOperationException();
#else
            if (_zipFile == null)
                throw new InvalidOperationException();
#endif
        }

        ~ZipFile()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
#if NETFX_CORE
                if (_zipArchive != null)
                    _zipArchive.Dispose();
#else
                if (_zipFile != null)
                {
                    _zipFile.Close();
                    ((IDisposable)_zipFile).Dispose();
                }
#endif
                if (_stream != null)
                    _stream.Dispose();
            }
        }

        //public IEnumerator<ZipEntry> GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    throw new NotImplementedException();
        //}

        public Encoding AlternateEncoding { get; set; }
        public ZipOption AlternateEncodingUsage { get; set; }

        public static ZipFile Read(Stream zipStream)
        {
            if (zipStream == null)
                throw new ArgumentNullException("zipStream", "zipStream is null.");

#if NETFX_CORE
            var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
            var zipFile = new ZipFile(zipStream, archive);

            return zipFile;
#else
            var sharpZipLibFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(zipStream)
            {
                IsStreamOwner = false,
            };
            var zipFile = new ZipFile(zipStream, sharpZipLibFile);

            return zipFile;
#endif
        }

        public void Save(Stream outputStream)
        {
            if (outputStream == null)
                throw new ArgumentNullException("outputStream", "outputStream is null.");

            AssertIsOpen();

#if NETFX_CORE
            var mode = _zipArchive.Mode;
            _zipArchive.Dispose();

            _stream.Position = 0;
            _stream.CopyTo(outputStream);
            _stream.Position = 0;

            _zipArchive = new ZipArchive(_stream, mode);
#else
            _zipFile.CommitUpdate();
            _zipFile.Close();
            ((IDisposable)_zipFile).Dispose();
            _zipFile = null;

            var position = _stream.Position;
            _stream.Position = 0;
            var buffer = new byte[256];
            for (int i = 0, read = 0; i < _stream.Length; i += read)
            {
                read = _stream.Read(buffer, i, buffer.Length);

                if (read > 0)
                    outputStream.Write(buffer, 0, read);
            }
            _stream.Position = position;

            _zipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(_stream)
            {
                IsStreamOwner = false,
            };
#endif
        }

        public ZipEntry AddEntry(string entryName, byte[] byteContent)
        {
            if (String.IsNullOrEmpty(entryName))
                throw new ArgumentException("entryName is null or empty.", "entryName");
            if (byteContent == null)
                throw new ArgumentException("byteContent is null.", "byteContent");

            AssertIsOpen();

#if NETFX_CORE
            var entry = _zipArchive.CreateEntry(entryName);
            using(var stream = entry.Open())
                stream.Write(byteContent, 0, byteContent.Length);

            return new ZipEntry(entry);
#else
            _zipFile.Add(new ICSharpCode.SharpZipLib.Zip.StreamDataSource(new MemoryStream(byteContent)),
                entryName);

            _zipFile.CommitUpdate();
            _zipFile.BeginUpdate();

            var entry = _zipFile.GetEntry(entryName);

            return new ZipEntry(_zipFile, entry);
#endif
        }        

        public ZipEntry AddEntry(string entryName, MemoryStream stream)
        {
            if (String.IsNullOrEmpty(entryName))
                throw new ArgumentException("entryName is null or empty.", "entryName");
            if (stream == null)
                throw new ArgumentException("stream is null.", "stream");

            AssertIsOpen();

#if NETFX_CORE
            var entry = _zipArchive.CreateEntry(entryName);
            var byteContent = StreamToByteArray(stream);
            using(var byteStream = entry.Open())
                byteStream.Write(byteContent, 0, byteContent.Length);

            return new ZipEntry(entry);
#else
            _zipFile.Add(new ICSharpCode.SharpZipLib.Zip.StreamDataSource(stream),
                entryName);

            _zipFile.CommitUpdate();
            _zipFile.BeginUpdate();

            var entry = _zipFile.GetEntry(entryName);

            return new ZipEntry(_zipFile, entry);
#endif
        }


        public ZipEntry this[string fileName]
        {
            get
            {
                if (!ContainsEntry(fileName))
                    return null;

#if NETFX_CORE
                var entry = _zipArchive.Entries.First(z => z.FullName == fileName);

                return new ZipEntry(entry);
#else
                var entry = _zipFile.GetEntry(fileName);

                return new ZipEntry(_zipFile, entry);
#endif
            }
        }

        public bool ContainsEntry(string name)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("name is null or empty.", "name");

            AssertIsOpen();
#if NETFX_CORE
            return _zipArchive.Entries.Any(z => z.FullName == name);
#else
            return _zipFile.FindEntry(name, false) > -1;
#endif
        }

        private static byte[] StreamToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                    ms.Write(buffer, 0, read);
                
                return ms.ToArray();
            }
        }
    }
}
