using System;
using System.IO;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Shapes;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace MigraDoc.Playground
{
    class Program
    {
        //https://github.com/ststeiger/PdfSharpCore
        //https://fonts.google.com/?category=Monospace&selection.family=Roboto+Mono:400i


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            GlobalFontSettings.FontResolver = new FontResolver();
            ImageSource.ImageSourceImpl = new ImageSourceX();


            var printer = new MigraDoc.Rendering.PdfDocumentRenderer(true, PdfFontEmbedding.Always);
            printer.Document = Generate();
            printer.RenderDocument();

            using (var pdfData = new MemoryStream())
            {
                printer.PdfDocument.Save(pdfData, false);

                File.WriteAllBytes("test.pdf", pdfData.ToArray());
            }
        }

        private static Document Generate()
        {
            var document = new Document();
            document.Info.Title = "test";
            document.Info.Subject = "test doc";
            document.Info.Author = "ml";

            var style = document.Styles[StyleNames.Normal];
            style.Font.Name = "Open Sans";
            style.Font.Size = new Unit(10, UnitType.Point);

            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.TopMargin = Unit.FromMillimeter(10);
            section.PageSetup.LeftMargin = Unit.FromMillimeter(10);
            section.PageSetup.RightMargin = Unit.FromMillimeter(10);
            section.PageSetup.BottomMargin = Unit.FromMillimeter(10);
            section.PageSetup.FooterDistance = Unit.FromMillimeter(0);

            section.AddParagraph("Hello PDF world!!");

            AddBarcode(section, BarcodeType.BarcodeEan13);
            AddBarcode(section, BarcodeType.Barcode128);
            AddBarcode(section, BarcodeType.Barcode25i);
            AddBarcode(section, BarcodeType.Barcode39);

            var image = section.AddImage(@"C:\Users\Michael\OneDrive\Afbeeldingen\Avatar2016.png");
            image.Height = Unit.FromCentimeter(8);
            image.Width = Unit.FromCentimeter(4);

            return document;
        }

        private static void AddBarcode(Section section, BarcodeType barcodeType)
        {
            var barcode = section.AddBarcode();
            barcode.Code = "123456123456";
            barcode.Text = true;
            barcode.Type = barcodeType;
            //barcode.Type = BarcodeType.Barcode39;
            barcode.Height = Unit.FromCentimeter(1);
            barcode.Width = Unit.FromCentimeter(5);
        }
    }

    public class ImageSourceX : ImageSource
    {
        public const int DefaultQuality = 75;

        protected override IImageSource FromFileImpl(string path, int? quality = DefaultQuality)
        {
            return new ImageSharpSourceBinary(path, File.ReadAllBytes(path), quality ?? DefaultQuality);
        }

        protected override IImageSource FromBinaryImpl(string name, Func<byte[]> imageSource, int? quality = DefaultQuality)
        {
            return new ImageSharpSourceBinary(name, imageSource(), quality ?? DefaultQuality);
        }

        protected override IImageSource FromStreamImpl(string name, Func<Stream> imageStream, int? quality = DefaultQuality)
        {
            return new ImageSharpSourceStream(name, imageStream(), quality ?? DefaultQuality);
        }

        public class ImageSharpSource : IImageSource
        {
            private readonly Image<Rgba32> _image;
            public string Name { get; }
            public int Width { get; }
            public int Height { get; }
            public int Quality { get; }

            public ImageSharpSource(string name, Image<Rgba32> image, int quality)
            {
                _image = image;
                Name = name;
                Width = _image.Width;
                Height = _image.Height;
                Quality = quality;
            }

            public void SaveAsJpeg(MemoryStream ms)
            {
                _image.SaveAsJpeg(ms, new JpegEncoder()
                {
                    Quality = Quality
                });
            }
        }

        public class ImageSharpSourceBinary : ImageSharpSource
        {
            public ImageSharpSourceBinary(string name, byte[] bytes, int quality)
                :base(name, SixLabors.ImageSharp.Image.Load<Rgba32>(bytes), quality)
            {
            }
        }
        public class ImageSharpSourceStream : ImageSharpSource
        {
            public ImageSharpSourceStream(string name, Stream stream, int quality)
                : base(name, SixLabors.ImageSharp.Image.Load<Rgba32>(stream), quality)
            {
            }
        }
    }


    public class FontResolver : IFontResolver
    {
        public string DefaultFontName { get; }

        public FontResolver()
        {
            DefaultFontName = "Open Sans";
        }

        public byte[] GetFont(string faceName)
        {
            using (var ms = new MemoryStream())
            {
                using (var fs = File.Open(faceName, FileMode.Open))
                {
                    fs.CopyTo(ms);
                    ms.Position = 0;
                    return ms.ToArray();
                }
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            //if (familyName.Equals("Open Sans", StringComparison.CurrentCultureIgnoreCase))
            {
                if (isBold && isItalic)
                {
                    return new FontResolverInfo("OpenSans-BoldItalic.ttf");
                }
                else if (isBold)
                {
                    return new FontResolverInfo("OpenSans-Bold.ttf");
                }
                else if (isItalic)
                {
                    return new FontResolverInfo("OpenSans-Italic.ttf");
                }
                else
                {
                    return new FontResolverInfo("OpenSans-Regular.ttf");
                }
            }
            return null;
        }
    }
}
