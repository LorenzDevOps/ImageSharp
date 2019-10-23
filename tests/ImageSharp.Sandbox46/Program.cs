// <copyright file="Program.cs" company="James Jackson-South">
// Copyright (c) James Jackson-South and contributors.
// Licensed under the Apache License, Version 2.0.
// </copyright>

using SixLabors.ImageSharp.Tests.PixelFormats.PixelOperations;
using SixLabors.ImageSharp.Tests.ProfilingBenchmarks;

namespace SixLabors.ImageSharp.Sandbox46
{
    using System;
    using System.IO;
    using SixLabors.ImageSharp.Formats.Jpeg;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Tests.Formats.Jpg;
    using SixLabors.Memory;
    using Xunit.Abstractions;

    public class Program
    {
        private class ConsoleOutput : ITestOutputHelper
        {
            public void WriteLine(string message) => Console.WriteLine(message);

            public void WriteLine(string format, params object[] args) => Console.WriteLine(format, args);
        }

        /// <summary>
        /// The main entry point. Useful for executing benchmarks and performance unit tests manually,
        /// when the IDE test runners lack some of the functionality. Eg.: it's not possible to run JetBrains memory profiler for unit tests.
        /// </summary>
        /// <param name="args">
        /// The arguments to pass to the program.
        /// </param>
        public static void Main(string[] args)
        {
            // RunJpegColorProfilingTests();

            // RunDecodeJpegProfilingTests();
            // RunToVector4ProfilingTest();
            //RunResizeProfilingTest();
            TestDecode();
            Console.ReadLine();
        }

        private static void RunJpegColorProfilingTests()
        {
            new JpegColorConverterTests(new ConsoleOutput()).BenchmarkYCbCr(false);
            new JpegColorConverterTests(new ConsoleOutput()).BenchmarkYCbCr(true);
        }

        private static void RunResizeProfilingTest()
        {
            var test = new ResizeProfilingBenchmarks(new ConsoleOutput());
            test.ResizeBicubic(4000, 4000);
        }

        private static void RunToVector4ProfilingTest()
        {
            var tests = new PixelOperationsTests.Rgba32OperationsTests(new ConsoleOutput());
            tests.Benchmark_ToVector4();
        }

        private static void RunDecodeJpegProfilingTests()
        {
            Console.WriteLine("RunDecodeJpegProfilingTests...");
            var benchmarks = new JpegProfilingBenchmarks(new ConsoleOutput());
            foreach (object[] data in JpegProfilingBenchmarks.DecodeJpegData)
            {
                string fileName = (string)data[0];
                benchmarks.DecodeJpeg(fileName);
            }
        }

        private static void TestDecode()
        {
            using (var stream = new FileStream(@"C:\Users\Anna\source\repos\HiveClient\src\Website\wwwroot\areaMapping\5e74f1322df24ae8bae2a9058ed36eb9\raw\e65c78cf40564e0cb2d493e06c4c6bbe.jpg", FileMode.Open))
            {
                var length = 102;
                for (var i = 0; i < 3; i++)
                {
                    using (var image = new JpegDecoder().Decode<Rgba32>(new Configuration(), stream, i* length, length))
                    {
                        image.Save(@"C:\Users\Anna\Downloads\tester2\test"+ i +".jpg", new JpegEncoder());
                        stream.Position = 0;
                    }
                }
            }
        }
    }
}
