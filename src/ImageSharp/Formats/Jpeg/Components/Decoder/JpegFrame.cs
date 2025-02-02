// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;

namespace SixLabors.ImageSharp.Formats.Jpeg.Components.Decoder
{
    /// <summary>
    /// Represent a single jpeg frame
    /// </summary>
    internal sealed class JpegFrame : IDisposable
    {
        /// <summary>
        /// Gets or sets a value indicating whether the frame uses the extended specification.
        /// </summary>
        public bool Extended { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the frame uses the progressive specification.
        /// </summary>
        public bool Progressive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the frame is a subset of the original frame
        /// </summary>
        public bool IsSubFrame { get; set; }

        /// <summary>
        /// Gets or sets the precision.
        /// </summary>
        public byte Precision { get; set; }

        /// <summary>
        /// Gets or sets the number of scanlines within the frame.
        /// </summary>
        public ushort Scanlines { get; set; }

        /// <summary>
        /// Gets or sets the offset of scanlines.
        /// </summary>
        public int LineOffset { get; set; }

        /// <summary>
        /// Gets or sets the number of samples per scanline.
        /// </summary>
        public ushort SamplesPerLine { get; set; }

        /// <summary>
        /// Gets or sets the number of components within a frame. In progressive frames this value can range from only 1 to 4.
        /// </summary>
        public byte ComponentCount { get; set; }

        /// <summary>
        /// Gets or sets the component id collection.
        /// </summary>
        public byte[] ComponentIds { get; set; }

        /// <summary>
        /// Gets or sets the order in which to process the components.
        /// in interleaved mode.
        /// </summary>
        public byte[] ComponentOrder { get; set; }

        /// <summary>
        /// Gets or sets the frame component collection.
        /// </summary>
        public JpegComponent[] Components { get; set; }

        /// <summary>
        /// Gets or sets the maximum horizontal sampling factor.
        /// </summary>
        public int MaxHorizontalFactor { get; set; }

        /// <summary>
        /// Gets or sets the maximum vertical sampling factor.
        /// </summary>
        public int MaxVerticalFactor { get; set; }

        /// <summary>
        /// Gets or sets the number of MCU's per line.
        /// </summary>
        public int McusPerLine { get; set; }

        /// <summary>
        /// Gets or sets the number of MCU's per column.
        /// </summary>
        public int McusPerColumn { get; set; }

        /// <summary>
        /// Gets or sets the offset of MCU's per column.
        /// </summary>
        public int McusPerColumnOffset { get; set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (this.Components != null)
            {
                for (int i = 0; i < this.Components.Length; i++)
                {
                    this.Components[i]?.Dispose();
                }

                this.Components = null;
            }
        }

        /// <summary>
        /// Allocates the frame component blocks.
        /// </summary>
        public void InitComponents()
        {
            this.McusPerLine = (int)MathF.Ceiling(this.SamplesPerLine / 8F / this.MaxHorizontalFactor);
            this.McusPerColumn = (int)MathF.Ceiling(this.Scanlines / 8F / this.MaxVerticalFactor);
            if (this.IsSubFrame)
            {
                this.McusPerColumnOffset = (int)MathF.Ceiling(this.LineOffset / 8F / this.MaxVerticalFactor);
            }

            for (int i = 0; i < this.ComponentCount; i++)
            {
                JpegComponent component = this.Components[i];
                component.Init();
                if (this.IsSubFrame)
                {
                    component.InitPlaceholder();
                }
            }
        }
    }
}
