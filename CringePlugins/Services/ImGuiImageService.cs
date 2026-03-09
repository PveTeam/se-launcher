using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using ImGuiNET;
using NLog;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using VRage.Collections;
using Device = SharpDX.Direct3D11.Device;

namespace CringePlugins.Services;

public interface IImGuiImageService
{
    ImGuiImage GetFromUrl(Uri url);
    ImGuiImage GetFromPath(string path);
}

internal sealed class ImGuiImageService(HttpClient client) : IImGuiImageService
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private readonly string _dir = Directory.CreateDirectory(
        Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CringeLauncher", "cache", "images")).FullName;
    private readonly CachingDictionary<ImageIdentifier, Image> _images = [];
    private readonly Dictionary<ImageIdentifier, ImageReference> _imageReferences = [];
    private readonly Dictionary<WebImageIdentifier, EntityTagHeaderValue> _webCacheEtag = [];
    private Image? _placeholderImage;
    private Device? _device;

    internal void Initialize(Device device)
    {
        _device = device;
        using var tex = new Texture2D(device, new()
        {
            Width = 1,
            Height = 1,
            Format = Format.R8G8B8A8_UNorm,
            MipLevels = 1,
            ArraySize = 1,
            SampleDescription = new()
            {
                Count = 1
            },
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.ShaderResource,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = ResourceOptionFlags.None,
        });

        var srv = new ShaderResourceView(device, tex);

        _placeholderImage = new Image(null!, [(srv, TimeSpan.Zero)], new(1, 1));
    }

    internal void Update()
    {
        foreach (var (identifier, image) in _images)
        {
            if (!image.IsUnused)
                continue;

            _images.Remove(identifier);
            _imageReferences.Remove(identifier);
            image.Dispose();
        }
        _images.ApplyRemovals();
    }

    public ImGuiImage GetFromUrl(Uri url)
    {
        var identifier = new WebImageIdentifier(url);
        if (_images.TryGetValue(identifier, out var image))
            return image;
        if (_imageReferences.TryGetValue(identifier, out var imageReference))
            return imageReference;

        var cachePath = Path.Join(_dir,
            Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(url.ToString()))));

        var reference = new ImageReference(_placeholderImage!);
        LoadAsync(url, cachePath, reference);
        _imageReferences.Add(identifier, reference);
        return reference;
    }

    private async void LoadAsync(Uri url, string cachePath, ImageReference reference)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (_webCacheEtag.TryGetValue(new(url), out var existingEtag))
                request.Headers.IfNoneMatch.Add(existingEtag);
            using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            if (response.Headers.ETag is { } etag)
                _webCacheEtag[new(url)] = etag;

            if (!File.Exists(cachePath) || (response.StatusCode != HttpStatusCode.NotModified &&
                                            !CompareCache(cachePath, response.Headers)))
            {
                await using var stream = await response.Content.ReadAsStreamAsync();
                await using var file = File.Create(cachePath);
                await stream.CopyToAsync(file);
            }

            reference.Image = GetFromPath(cachePath);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to load image {Url}", url);
            reference.ErrorImage = null; // todo make an error image
        }
    }

    private static bool CompareCache(string path, HttpResponseHeaders headers)
    {
        if (headers.CacheControl is not { } cacheControl)
            return false;

        if (cacheControl.NoCache)
            return false;

        if (cacheControl.MaxAge.HasValue)
        {
            var responseAge = DateTimeOffset.UtcNow - cacheControl.MaxAge.Value;
            return File.GetLastWriteTimeUtc(path) > responseAge;
        }

        return true;
    }

    public ImGuiImage GetFromPath(string path)
    {
        path = Path.GetFullPath(path);
        var identifier = new FileImageIdentifier(path);
        if (_images.TryGetValue(identifier, out var image))
            return image;

        if (!File.Exists(path))
            throw new FileNotFoundException(null, path);

        var img = SixLabors.ImageSharp.Image.Load(path);

        try
        {
            var format = Format.R8G8B8A8_UNorm;
            switch (img)
            {
                case Image<Bgra32>:
                    format = Format.B8G8R8A8_UNorm;
                    break;
                case Image<Rgba32>:
                    break;
                default:
                {
                    var clone = img.CloneAs<Rgba32>();
                    img.Dispose();
                    img = clone;
                    break;
                }
            }

            var connectingMetadata = img.MetaData.GetWebpMetadata().ToFormatConnectingMetadata();

            var data = new DataBox[img.Frames.Count];
            var frames = new(ShaderResourceView srv, TimeSpan delay)[img.Frames.Count];
            var previousMetadata = new WebpFrameMetadata[img.Frames.Count];
            
            for (var i = 0; i < img.Frames.Count; i++)
            {
                var imgFrame = img.Frames[i];
                var metadata = previousMetadata[i] = imgFrame.Metadata.GetWebpMetadata();
                frames[i].delay = TimeSpan.FromMilliseconds(metadata.FrameDelay);
                switch (imgFrame)
                {
                    case ImageFrame<Bgra32> imageFrame:
                    {
                        CopyData(imageFrame, data, i, connectingMetadata, previousMetadata);
                        break;
                    }
                    case ImageFrame<Rgba32> imageFrame:
                    {
                        CopyData(imageFrame, data, i, connectingMetadata, previousMetadata);
                        break;
                    }
                }
            }

            using var tex = new Texture2D(_device, new()
            {
                Width = img.Width,
                Height = img.Height,
                Format = format,
                MipLevels = 1,
                ArraySize = img.Frames.Count,
                SampleDescription = new()
                {
                    Count = 1
                },
                Usage = ResourceUsage.Immutable,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
            }, data);
            
            foreach (var box in data)
            {
                if (!box.IsEmpty) Marshal.FreeHGlobal(box.DataPointer);
            }
            
            for (var i = 0; i < frames.Length; i++)
            {
                frames[i].srv = new(_device, tex, new()
                {
                    Format = format,
                    Dimension = ShaderResourceViewDimension.Texture2DArray,
                    Texture2DArray =
                    {
                        MipLevels = 1,
                        ArraySize = 1,
                        FirstArraySlice = i,
                    }
                });
            }

            image = new Image(identifier, frames, new(img.Width, img.Height));
            _images.Add(identifier, image, true);
            return image;
        }
        finally
        {
            img.Dispose();
        }
    }

    private static unsafe void CopyData<T>(ImageFrame<T> frame, DataBox[] data, int frameIndex,
        FormatConnectingMetadata connectingMetadata, WebpFrameMetadata[] previousMetadata) where T : unmanaged, IPixel<T>
    {
        var cb = frame.Width * frame.Height;
        data[frameIndex] = new()
        {
            DataPointer = Marshal.AllocHGlobal(cb * sizeof(T)),
            RowPitch = frame.Width * sizeof(T)
        };
        var destination = new Span<T>((void*)data[frameIndex].DataPointer, cb);

        if (frameIndex > 0)
        {
            switch (previousMetadata[frameIndex - 1].DisposalMode)
            {
                case FrameDisposalMode.Unspecified:
                    break;
                case FrameDisposalMode.DoNotDispose:
                {
                    var previousFrame = new Span<T>((void*)data[frameIndex - 1].DataPointer, cb);
                    previousFrame.CopyTo(destination);
                    break;
                }
                case FrameDisposalMode.RestoreToBackground:
                    destination.Fill(connectingMetadata.BackgroundColor.ToPixel<T>());
                    break;
                case FrameDisposalMode.RestoreToPrevious:
                    if (frameIndex > 1)
                    {
                        var previousFrame = new Span<T>((void*)data[frameIndex - 2].DataPointer, cb);
                        previousFrame.CopyTo(destination);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            destination.Fill(connectingMetadata.BackgroundColor.ToPixel<T>());
        }
        
        if (previousMetadata[frameIndex].BlendMode == FrameBlendMode.Over && frameIndex > 0)
        {
            var previousFrame = new Span<T>((void*)data[frameIndex - 1].DataPointer, cb);
            previousFrame.CopyTo(destination);
        }

        frame.CopyPixelDataTo(destination);
    }

    private class ImageReference(ImGuiImage placeholderImage) : ImGuiImage
    {
        public ImGuiImage? Image;
        public ImGuiImage? ErrorImage;

        public override ImTextureRef TextureId => Image ?? ErrorImage ?? placeholderImage;
        public override Vector2 Size => Image ?? ErrorImage ?? placeholderImage;

        public override void Dispose()
        {
            Image?.Dispose();
            ErrorImage?.Dispose();
        }
    }

    private class Image(ImageIdentifier identifier, (ShaderResourceView srv, TimeSpan delay)[] srvs, Vector2 size) : ImGuiImage
    {
        private bool _disposed;
        private long _lastUse = Stopwatch.GetTimestamp();
        private long _lastFrame = Stopwatch.GetTimestamp();
        private int _frame;

        public override ImTextureRef TextureId
        {
            get
            {
                OnUse();
                return new()
                {
                    _TexID = srvs[_frame].srv.NativePointer
                };
            }
        }

        public override Vector2 Size
        {
            get
            {
                OnUse();
                return size;
            }
        }

        public bool IsUnused => _disposed || Stopwatch.GetElapsedTime(_lastUse) > TimeSpan.FromMinutes(5);

        private void OnUse()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (srvs.Length > 1 && Stopwatch.GetElapsedTime(_lastFrame) >= srvs[_frame].delay)
            {
                _frame++;
                if (_frame >= srvs.Length) _frame = 0;
                _lastFrame = Stopwatch.GetTimestamp();
            }
            _lastUse = Stopwatch.GetTimestamp();
        }

        public override void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (var (srv, _) in srvs)
            {
                srv.Dispose();
            }
        }

        public override string ToString()
        {
            return $"Image {{ {identifier} {size} x{srvs.Length} }}";
        }
    }

    private abstract record ImageIdentifier;
    private record WebImageIdentifier(Uri Url) : ImageIdentifier;
    private record FileImageIdentifier(string Path) : ImageIdentifier;
}

public abstract class ImGuiImage : IDisposable
{
    public abstract ImTextureRef TextureId { get; }
    public abstract Vector2 Size { get; }

    public static implicit operator ImTextureRef(ImGuiImage image) => image.TextureId;
    public static implicit operator Vector2(ImGuiImage image) => image.Size;
    public abstract void Dispose();
}
