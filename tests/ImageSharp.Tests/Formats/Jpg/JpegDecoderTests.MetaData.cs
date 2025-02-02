// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.IO;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using SixLabors.ImageSharp.PixelFormats;
using Xunit;

// ReSharper disable InconsistentNaming
namespace SixLabors.ImageSharp.Tests.Formats.Jpg
{
    using System;
    using System.Runtime.CompilerServices;

    using SixLabors.ImageSharp.Formats.Jpeg;
    using SixLabors.ImageSharp.Metadata;

    public partial class JpegDecoderTests
    {
        // TODO: A JPEGsnoop & metadata expert should review if the Exif/Icc expectations are correct.
        // I'm seeing several entries with Exif-related names in images where we do not decode an exif profile. (- Anton)
        public static readonly TheoryData<bool, string, int, bool, bool> MetaDataTestData =
        new TheoryData<bool, string, int, bool, bool>
        {
            { false, TestImages.Jpeg.Progressive.Progress, 24, false, false },
            { false, TestImages.Jpeg.Progressive.Fb, 24, false, true },
            { false, TestImages.Jpeg.Baseline.Cmyk, 32, false, true },
            { false, TestImages.Jpeg.Baseline.Ycck, 32, true, true },
            { false, TestImages.Jpeg.Baseline.Jpeg400, 8, false, false },
            { false, TestImages.Jpeg.Baseline.Snake, 24, true, true },
            { false, TestImages.Jpeg.Baseline.Jpeg420Exif, 24, true, false },

            { true, TestImages.Jpeg.Progressive.Progress, 24, false, false },
            { true, TestImages.Jpeg.Progressive.Fb, 24, false, true },
            { true, TestImages.Jpeg.Baseline.Cmyk, 32, false, true },
            { true, TestImages.Jpeg.Baseline.Ycck, 32, true, true },
            { true, TestImages.Jpeg.Baseline.Jpeg400, 8, false, false },
            { true, TestImages.Jpeg.Baseline.Snake, 24, true, true },
            { true, TestImages.Jpeg.Baseline.Jpeg420Exif, 24, true, false },
        };

        public static readonly TheoryData<string, int, int, PixelResolutionUnit> RatioFiles =
        new TheoryData<string, int, int, PixelResolutionUnit>
        {
            { TestImages.Jpeg.Baseline.Ratio1x1, 1, 1 , PixelResolutionUnit.AspectRatio},
            { TestImages.Jpeg.Baseline.Snake, 300, 300 , PixelResolutionUnit.PixelsPerInch},
            { TestImages.Jpeg.Baseline.GammaDalaiLamaGray, 72, 72, PixelResolutionUnit.PixelsPerInch }
        };

        public static readonly TheoryData<string, int> QualityFiles =
        new TheoryData<string, int>
        {
            { TestImages.Jpeg.Baseline.Calliphora, 80 },
            { TestImages.Jpeg.Progressive.Fb, 75 },
            { TestImages.Jpeg.Issues.IncorrectQuality845, 99 }
        };

        public static readonly TheoryData<string, int, int> OffsetFiles =
        new TheoryData<string, int, int>
        {
            { TestImages.Jpeg.Baseline.Jpeg400, 600, 800 },
            { TestImages.Jpeg.Baseline.Jpeg420Exif, 2048, 1536 },
            { TestImages.Jpeg.Baseline.Jpeg420Small, 200, 100 }
        };

        public static readonly TheoryData<string> ProgressiveFiles =
        new TheoryData<string>
        {
            { TestImages.Jpeg.Progressive.Progress },
            { TestImages.Jpeg.Progressive.Fb }
        };

        [Theory]
        [MemberData(nameof(MetaDataTestData))]
        public void MetaDataIsParsedCorrectly(
            bool useIdentify,
            string imagePath,
            int expectedPixelSize,
            bool exifProfilePresent,
            bool iccProfilePresent)
        {
            TestMetaDataImpl(
                useIdentify,
                JpegDecoder,
                imagePath,
                expectedPixelSize,
                exifProfilePresent,
                iccProfilePresent);
        }

        [Theory]
        [MemberData(nameof(RatioFiles))]
        public void Decode_VerifyRatio(string imagePath, int xResolution, int yResolution, PixelResolutionUnit resolutionUnit)
        {
            var testFile = TestFile.Create(imagePath);
            using (var stream = new MemoryStream(testFile.Bytes, false))
            {
                var decoder = new JpegDecoder();
                using (Image<Rgba32> image = decoder.Decode<Rgba32>(Configuration.Default, stream))
                {
                    ImageMetadata meta = image.Metadata;
                    Assert.Equal(xResolution, meta.HorizontalResolution);
                    Assert.Equal(yResolution, meta.VerticalResolution);
                    Assert.Equal(resolutionUnit, meta.ResolutionUnits);
                }
            }
        }

        [Theory]
        [MemberData(nameof(OffsetFiles))]
        public void Decode_VerifyOffset(string imagePath, int xPixels, int yPixels)
        {
            var testFile = TestFile.Create(imagePath);
            using (var stream = new MemoryStream(testFile.Bytes, false))
            {
                var decoder = new JpegDecoder();
                int length = (int)Math.Ceiling((double)yPixels / 4);

                for (int offset = 0; offset < yPixels; offset += length)
                {
                    length = (offset + length) < yPixels ? length : yPixels - offset;
                    using (Image<Rgba32> image = decoder.Decode<Rgba32>(Configuration.Default, stream, offset, length))
                    {
                        Assert.Equal(xPixels, image.Width);
                        Assert.Equal(length, image.Height);
                    }                    
                    stream.Position = 0;
                }
            }
        }

        [Theory]
        [MemberData(nameof(ProgressiveFiles))]
        public void Decode_ProgressiveOffset_ExceptionThrown(string imagePath)
        {
            var testFile = TestFile.Create(imagePath);
            using (var stream = new MemoryStream(testFile.Bytes, false))
            {
                var decoder = new JpegDecoder();
                int lineOffset = 0;
                int lineLength = 1;

                Assert.Throws<NotImplementedException>(() =>
                    decoder.Decode<Rgba32>(Configuration.Default, stream, lineOffset, lineLength));
            }
        }

        [Theory]
        [MemberData(nameof(RatioFiles))]
        public void Identify_VerifyRatio(string imagePath, int xResolution, int yResolution, PixelResolutionUnit resolutionUnit)
        {
            var testFile = TestFile.Create(imagePath);
            using (var stream = new MemoryStream(testFile.Bytes, false))
            {
                var decoder = new JpegDecoder();
                IImageInfo image = decoder.Identify(Configuration.Default, stream);
                ImageMetadata meta = image.Metadata;
                Assert.Equal(xResolution, meta.HorizontalResolution);
                Assert.Equal(yResolution, meta.VerticalResolution);
                Assert.Equal(resolutionUnit, meta.ResolutionUnits);
            }
        }

        [Theory]
        [MemberData(nameof(QualityFiles))]
        public void Identify_VerifyQuality(string imagePath, int quality)
        {
            var testFile = TestFile.Create(imagePath);
            using (var stream = new MemoryStream(testFile.Bytes, false))
            {
                var decoder = new JpegDecoder();
                IImageInfo image = decoder.Identify(Configuration.Default, stream);
                JpegMetadata meta = image.Metadata.GetFormatMetadata(JpegFormat.Instance);
                Assert.Equal(quality, meta.Quality);
            }
        }

        [Theory]
        [MemberData(nameof(QualityFiles))]
        public void Decode_VerifyQuality(string imagePath, int quality)
        {
            var testFile = TestFile.Create(imagePath);
            using (var stream = new MemoryStream(testFile.Bytes, false))
            {
                var decoder = new JpegDecoder();
                using (Image<Rgba32> image = decoder.Decode<Rgba32>(Configuration.Default, stream))
                {
                    JpegMetadata meta = image.Metadata.GetFormatMetadata(JpegFormat.Instance);
                    Assert.Equal(quality, meta.Quality);
                }
            }
        }

        private static void TestImageInfo(string imagePath, IImageDecoder decoder, bool useIdentify, Action<IImageInfo> test)
        {
            var testFile = TestFile.Create(imagePath);
            using (var stream = new MemoryStream(testFile.Bytes, false))
            {
                IImageInfo imageInfo = useIdentify
                ? ((IImageInfoDetector)decoder).Identify(Configuration.Default, stream)
                : decoder.Decode<Rgba32>(Configuration.Default, stream);

                test(imageInfo);
            }
        }

        private static void TestMetaDataImpl(
            bool useIdentify,
            IImageDecoder decoder,
            string imagePath,
            int expectedPixelSize,
            bool exifProfilePresent,
            bool iccProfilePresent)
        {
            TestImageInfo(
                imagePath,
                decoder,
                useIdentify,
                imageInfo =>
                    {
                        Assert.NotNull(imageInfo);
                        Assert.NotNull(imageInfo.PixelType);

                        if (useIdentify)
                        {
                            Assert.Equal(expectedPixelSize, imageInfo.PixelType.BitsPerPixel);
                        }
                        else
                        {
                            // When full Image<TPixel> decoding is performed, BitsPerPixel will match TPixel
                            int bpp32 = Unsafe.SizeOf<Rgba32>() * 8;
                            Assert.Equal(bpp32, imageInfo.PixelType.BitsPerPixel);
                        }

                        ExifProfile exifProfile = imageInfo.Metadata.ExifProfile;

                        if (exifProfilePresent)
                        {
                            Assert.NotNull(exifProfile);
                            Assert.NotEmpty(exifProfile.Values);
                        }
                        else
                        {
                            Assert.Null(exifProfile);
                        }

                        IccProfile iccProfile = imageInfo.Metadata.IccProfile;

                        if (iccProfilePresent)
                        {
                            Assert.NotNull(iccProfile);
                            Assert.NotEmpty(iccProfile.Entries);
                        }
                        else
                        {
                            Assert.Null(iccProfile);
                        }
                    });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IgnoreMetaData_ControlsWhetherMetaDataIsParsed(bool ignoreMetaData)
        {
            var decoder = new JpegDecoder { IgnoreMetadata = ignoreMetaData };

            // Snake.jpg has both Exif and ICC profiles defined:
            var testFile = TestFile.Create(TestImages.Jpeg.Baseline.Snake);

            using (Image<Rgba32> image = testFile.CreateRgba32Image(decoder))
            {
                if (ignoreMetaData)
                {
                    Assert.Null(image.Metadata.ExifProfile);
                    Assert.Null(image.Metadata.IccProfile);
                }
                else
                {
                    Assert.NotNull(image.Metadata.ExifProfile);
                    Assert.NotNull(image.Metadata.IccProfile);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Decoder_Reads_Correct_Resolution_From_Jfif(bool useIdentify)
        {
            TestImageInfo(TestImages.Jpeg.Baseline.Floorplan, JpegDecoder, useIdentify,
                imageInfo =>
                    {
                        Assert.Equal(300, imageInfo.Metadata.HorizontalResolution);
                        Assert.Equal(300, imageInfo.Metadata.VerticalResolution);
                    });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Decoder_Reads_Correct_Resolution_From_Exif(bool useIdentify)
        {
            TestImageInfo(TestImages.Jpeg.Baseline.Jpeg420Exif, JpegDecoder, useIdentify,
                imageInfo =>
                    {
                        Assert.Equal(72, imageInfo.Metadata.HorizontalResolution);
                        Assert.Equal(72, imageInfo.Metadata.VerticalResolution);
                    });
        }
    }
}
