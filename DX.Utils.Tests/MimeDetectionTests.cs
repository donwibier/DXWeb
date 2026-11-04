using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DX.Utils.MimeDetection;
using Xunit;

namespace DX.Utils.Tests
{
    internal static class Samples
    {
        public static readonly byte[] Png = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0x0D };
        public static readonly byte[] Jpeg = { 0xFF, 0xD8, 0xFF, 0xE0, 0, 0x10, 0x4A, 0x46 };
        public static readonly byte[] Gif = Encoding.ASCII.GetBytes("GIF89a\x01\x00");
        public static readonly byte[] Pdf = Encoding.ASCII.GetBytes("%PDF-1.7\n%...");
        public static readonly byte[] Zip = { 0x50, 0x4B, 0x03, 0x04, 0x14, 0, 0, 0 };
        public static readonly byte[] Gzip = { 0x1F, 0x8B, 0x08, 0 };
        public static readonly byte[] Text = Encoding.ASCII.GetBytes("just some text here");
    }

    public class MimeTypeTests
    {
        [Fact]
        public void Ctor_ParsesPrimaryAndSub()
        {
            var t = new MimeType("text/html");
            Assert.Equal("text", t.PrimaryType);
            Assert.Equal("html", t.SubType);
            Assert.Equal("text/html", t.Name);
            Assert.Equal("text/html", t.ToString());
        }

        [Fact]
        public void Ctor_StripsParameters()
        {
            Assert.Equal("text/html", new MimeType("text/html; charset=UTF-8").Name);
            Assert.Equal("text/html", MimeType.Clean("text/html;charset=UTF-8"));
        }

        [Fact]
        public void TwoArgCtor()
        {
            var t = new MimeType("image", "png");
            Assert.Equal("image/png", t.Name);
        }

        [Theory]
        [InlineData("texthtml")]
        [InlineData("text/")]
        [InlineData("text/;charset=x")]
        [InlineData("text/ht:ml")]
        [InlineData("text/a/b")]
        [InlineData("text/a\u0001b")]
        public void Ctor_InvalidContentType_ThrowsMimeTypeException(string name)
        {
            Assert.Throws<MimeTypeException>(() => new MimeType(name));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Ctor_NullOrEmpty_ThrowsArgumentNull(string name)
        {
            Assert.Throws<ArgumentNullException>(() => new MimeType(name));
        }

        [Fact]
        public void TwoArgCtor_InvalidPrimary_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new MimeType("", "png"));
            Assert.Throws<MimeTypeException>(() => new MimeType("im:age", "png"));
        }

        [Fact]
        public void Ctor_OpenParenthesis_IsRejected()
        {
            Assert.Throws<MimeTypeException>(() => new MimeType("text/a(b"));
        }

        [Fact]
        public void Ctor_InvalidPrimary_IsRejected()
        {
            Assert.Throws<MimeTypeException>(() => new MimeType("/plain"));
            Assert.Throws<MimeTypeException>(() => new MimeType("te;xt/plain"));
        }

        [Fact]
        public void Equality_IsByName()
        {
            Assert.Equal(new MimeType("text/html"), new MimeType("text/html; charset=x"));
            Assert.Equal(new MimeType("text/html").GetHashCode(), new MimeType("text", "html").GetHashCode());
            Assert.False(new MimeType("text/html").Equals("text/html"));
            Assert.False(new MimeType("text/html").Equals(null));
        }

        [Fact]
        public void Matches_ByExtension()
        {
            var t = new MimeTypes().ForName("application/pdf");
            Assert.True(t.Matches("report.pdf"));
            Assert.True(t.Matches("/some/dir/report.v2.pdf"));
            Assert.False(t.Matches("report.pdf."));
            Assert.False(t.Matches("report"));
            Assert.False(t.Matches("report.doc"));
        }

        [Fact]
        public void Matches_ByExtension_IsCaseInsensitive()
        {
            Assert.True(new MimeTypes().ForName("application/pdf").Matches("REPORT.PDF"));
        }
    }

    public class MimeTypesTests
    {
        [Fact]
        public void EmbeddedDefinitions_AreLoaded()
        {
            var types = new MimeTypes();
            Assert.NotNull(types.ForName("image/png"));
            Assert.NotNull(types.ForName("application/pdf"));
            Assert.Null(types.ForName("does/not-exist"));
            Assert.True(types.MinLength >= 257 + 5); // tar magic at offset 257
        }

        [Theory]
        [InlineData("png", "image/png")]
        [InlineData("jpeg", "image/jpeg")]
        [InlineData("gif", "image/gif")]
        [InlineData("pdf", "application/pdf")]
        [InlineData("gzip", "application/x-gzip")]
        public void GetMimeType_ByContent(string sample, string expected)
        {
            var bytes = (byte[])typeof(Samples).GetField(char.ToUpper(sample[0]) + sample.Substring(1)).GetValue(null);
            var t = new MimeTypes().GetMimeType(SupportUtil.ToSByteArray(bytes));
            Assert.NotNull(t);
            Assert.Equal(expected, t.Name);
        }

        [Fact]
        public void GetMimeType_ByContent_NullEmptyOrUnknown_ReturnsNull()
        {
            var types = new MimeTypes();
            Assert.Null(types.GetMimeType((sbyte[])null));
            Assert.Null(types.GetMimeType(new sbyte[0]));
            Assert.Null(types.GetMimeType(SupportUtil.ToSByteArray(Samples.Text)));
        }

        [Fact]
        public void GetMimeType_ByContent_TarMagicAtOffset257()
        {
            var data = new byte[512];
            Encoding.ASCII.GetBytes("ustar").CopyTo(data, 257);
            data[0] = (byte)'f';
            Assert.Equal("application/x-tar", new MimeTypes().GetMimeType(SupportUtil.ToSByteArray(data)).Name);
        }

        [Fact]
        public void GetMimeType_ByName_NoExtension_ReturnsNull()
        {
            var types = new MimeTypes();
            Assert.Null(types.GetMimeType("README"));
            Assert.Null(types.GetMimeType("trailingdot."));
        }

        [Theory]
        [InlineData("report.pdf", "application/pdf")]
        [InlineData("photo.PNG", "image/png")]
        [InlineData("/a/b/archive.zip", "application/zip")]
        public void GetMimeType_ByName(string name, string expected)
        {
            var t = new MimeTypes().GetMimeType(name);
            Assert.NotNull(t);
            Assert.Equal(expected, t.Name);
        }

        [Fact]
        public void GetMimeType_ByUri()
        {
            Assert.Equal("application/pdf", new MimeTypes().GetMimeType(new Uri("https://x.com/files/a.pdf?x=1")).Name);
        }

        [Fact]
        public void GetMimeType_ByNameAndData_FallsBackToContent()
        {
            var t = new MimeTypes().GetMimeType("noextension", SupportUtil.ToSByteArray(Samples.Png));
            Assert.Equal("image/png", t.Name);
        }

        [Fact]
        public void DetermineMimeType_ByContent()
        {
            Assert.Equal("image/png", MimeTypes.DetermineMimeType("x.bin", Samples.Png));
            Assert.Equal("application/pdf", MimeTypes.DetermineMimeType(null, SupportUtil.ToSByteArray(Samples.Pdf)));
        }

        [Fact]
        public void DetermineMimeType_NothingKnown_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, MimeTypes.DetermineMimeType("x", Samples.Text));
            Assert.Equal(string.Empty, MimeTypes.DetermineMimeType("x.pdf", (byte[])null));
            Assert.Null(MimeTypes.GetMimeTypeFromBytes("x.pdf", null));
        }

        [Fact]
        public void DetermineMimeType_FallsBackToExtension()
        {
            Assert.Equal("text/plain", MimeTypes.DetermineMimeType("notes.txt", Samples.Text));
        }

        [Fact]
        public void DetermineMimeType_ExtensionLookup_IsCultureInvariant()
        {
            using (new CultureScope("tr-TR"))
                Assert.Equal("image/tiff", MimeTypes.DetermineMimeType("SCAN.TIFF", Samples.Text));
        }

        [Fact]
        public void DetermineMimeType_ZipArchive_IsZip()
        {
            Assert.Equal("application/zip", MimeTypes.DetermineMimeType("archive.zip", Samples.Zip));
        }

        [Fact]
        public void DetermineMimeType_ContentWinsOverExtension_DocxIsReportedAsZip()
        {
            // By design content is checked first, and the OOXML entries are commented out in mime-types.xml.
            Assert.Equal("application/zip", MimeTypes.DetermineMimeType("report.docx", Samples.Zip));
        }

        [Fact]
        public void Get_CachesInstancePerFilePath()
        {
            var path = Path.Combine(Path.GetTempPath(), "dxutils-mime-" + Guid.NewGuid().ToString("N") + ".xml");
            File.WriteAllText(path,
                "<?xml version=\"1.0\"?><mime-types>" +
                "<mime-type name=\"x-test/foo\" description=\"Foo\"><ext>foo</ext><magic offset=\"1\" value=\"OO\"/></mime-type>" +
                "<mime-type name=\"bad\"/>" +
                "<mime-type><ext>noname</ext></mime-type>" +
                "</mime-types>");
            try
            {
                var a = MimeTypes.Get(path);
                var b = MimeTypes.Get(path);
                Assert.Same(a, b);
                Assert.NotNull(a.ForName("x-test/foo"));
                Assert.Null(a.ForName("bad"));
                Assert.Equal("x-test/foo", a.GetMimeType(SupportUtil.ToSByteArray(Encoding.ASCII.GetBytes("FOO"))).Name);
                Assert.Equal(3, a.MinLength);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void Get_IsThreadSafe()
        {
            var path = Path.Combine(Path.GetTempPath(), "dxutils-mime-" + Guid.NewGuid().ToString("N") + ".xml");
            File.WriteAllText(path, "<mime-types><mime-type name=\"a/b\"><ext>ab</ext></mime-type></mime-types>");
            try
            {
                var results = new MimeTypes[64];
                Parallel.For(0, results.Length, i => results[i] = MimeTypes.Get(path));
                Assert.All(results, r => Assert.Same(results[0], r));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void WrongRootElement_YieldsNoTypes()
        {
            var path = Path.Combine(Path.GetTempPath(), "dxutils-mime-" + Guid.NewGuid().ToString("N") + ".xml");
            File.WriteAllText(path, "<other><mime-type name=\"a/b\"/></other>");
            try
            {
                Assert.Null(new MimeTypes(path).ForName("a/b"));
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void ConcurrentDetection_IsConsistent()
        {
            var results = new string[32];
            Parallel.For(0, results.Length, i => results[i] = MimeTypes.DetermineMimeType("x", Samples.Png));
            Assert.All(results, r => Assert.Equal("image/png", r));
        }
    }

    public class SupportUtilTests
    {
        [Fact]
        public void ByteSByteRoundTrip()
        {
            var bytes = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
            var sbytes = SupportUtil.ToSByteArray(bytes);
            Assert.Equal((sbyte)-1, sbytes[255]);
            Assert.Equal(bytes, SupportUtil.ToByteArray(sbytes));
        }

        [Fact]
        public void Nulls_PassThrough()
        {
            Assert.Null(SupportUtil.ToSByteArray(null));
            Assert.Null(SupportUtil.ToByteArray((sbyte[])null));
            Assert.Null(SupportUtil.ToByteArray((object[])null));
        }

        [Fact]
        public void StringToBytes_IsUtf8()
        {
            Assert.Equal(new byte[] { 0xC3, 0xA9 }, SupportUtil.ToByteArray("é"));
        }

        [Fact]
        public void ObjectArrayToBytes()
        {
            Assert.Equal(new byte[] { 1, 2 }, SupportUtil.ToByteArray(new object[] { (byte)1, (byte)2 }));
        }

        [Fact]
        public void ToArray_CopiesCollection()
        {
            var result = SupportUtil.ToArray(new System.Collections.ArrayList { 1, "a" }, new object[0]);
            Assert.Equal(new object[] { 1, "a" }, result);
        }

        [Fact]
        public void MimeTypeException_Messages()
        {
            Assert.Equal("Mime detection exception", new MimeTypeException().Message);
            Assert.Equal("x", new MimeTypeException("x").Message);
        }
    }
}
