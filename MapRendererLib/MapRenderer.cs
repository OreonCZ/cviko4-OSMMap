using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace MapRendererLib
{
    public class MapRenderer : IDisposable
    {
        private readonly Dictionary<string, MapDrawerTile> data = new Dictionary<string, MapDrawerTile>();

        private static readonly WebClient client = new WebClient();

        private const int TILE_SIZE = 256;

        private readonly int width;
        private readonly int height;

        private SKBitmap bmp;

        public MapRenderer(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public void Set(int x, int y, string url, float opacity = 1)
        {
            if (opacity < 0)
                opacity = 0;
            else if (opacity > 1)
                opacity = 1;

            if (x < 0 || y < 0 || x >= width || y >= height)
            {
                throw new ArgumentOutOfRangeException($"Tile index out of bounds: {x}:{y}");
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            data[$"{x}:{y}"] =
                new MapDrawerTile(
                    x * TILE_SIZE,
                    y * TILE_SIZE,
                    url,
                    opacity);
        }

        public void Clear()
        {
            data.Clear();
        }

        public void Flush()
        {
            if (bmp == null)
            {
                bmp = new SKBitmap(
                    width * TILE_SIZE,
                    height * TILE_SIZE,
                    SKColorType.Rgba8888,
                    SKAlphaType.Premul);

                using (SKCanvas canvas = new SKCanvas(bmp))
                {
                    canvas.Clear(SKColors.Gray);
                }
            }

            if (data.Count == 0)
                return;

            using (SKCanvas canvas = new SKCanvas(bmp))
            {
                foreach (var tile in data.Values)
                {
                    using (Stream stream =
                           DownloadTileOrResolveFromCache(tile.Url))
                    {
                        stream.Position = 0;

                        using (SKBitmap tileImg = SKBitmap.Decode(stream))
                        {
                            if (tileImg == null)
                                continue;

                            using (SKPaint paint = new SKPaint())
                            {
                                byte alpha = (byte)(tile.Opacity * 255);

                                paint.Color = new SKColor(255, 255, 255, alpha);

                                var destination = new SKRect(
                                    tile.X,
                                    tile.Y,
                                    tile.X + tileImg.Width,
                                    tile.Y + tileImg.Height);

                                using (SKImage tileImage = SKImage.FromBitmap(tileImg))
                                {
                                    canvas.DrawImage(
                                        tileImage,
                                        destination,
                                        new SKSamplingOptions(SKFilterMode.Linear),
                                        paint);
                                }
                            }
                        }
                    }
                }
            }

            Clear();
        }

        public void Render(string path = "map.png")
        {
            Flush();

            if (bmp == null)
                return;

            using (SKImage image = SKImage.FromBitmap(bmp))
            using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (FileStream file = File.Create(path))
            {
                data.SaveTo(file);
            }

            bmp.Dispose();
            bmp = null;
        }

        private Stream DownloadTileOrResolveFromCache(string url)
        {
            DirectoryInfo di = new DirectoryInfo("./cache");

            if (!di.Exists)
                di.Create();

            FileInfo fi = new FileInfo(
                Path.Combine(
                    di.FullName,
                    url.Replace('/', '_').Replace(':', '_')));

            if (fi.Exists)
                return fi.OpenRead();

            Stream result = DownloadTile(url);

            using (FileStream cache = fi.OpenWrite())
            {
                result.Position = 0;
                result.CopyTo(cache);
            }

            result.Position = 0;

            return result;
        }

        private Stream DownloadTile(string url)
        {
            client.Headers["User-Agent"] = "VSB - CSharp";

            byte[] data = client.DownloadData(url);

            return new MemoryStream(data);
        }

        public void Dispose()
        {
            bmp?.Dispose();
            bmp = null;
        }

        private class MapDrawerTile
        {
            public int X { get; }
            public int Y { get; }
            public string Url { get; }
            public float Opacity { get; }

            public MapDrawerTile(
                int x,
                int y,
                string url,
                float opacity)
            {
                X = x;
                Y = y;
                Url = url;
                Opacity = opacity;
            }
        }
    }
}