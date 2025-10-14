using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using ImGuiNET;
using NLog;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
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

        _placeholderImage = new Image(null!, srv, new(1, 1));
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

        using var img = LoadFromPath(path);

        var desc = img.Description;
        image = new Image(identifier, LoadTexture(img), new(desc.Width, desc.Height));
        
        _images.Add(identifier, image, true);
        
        return image;
    }

    private ShaderResourceView LoadTexture(SharpDX.Toolkit.Graphics.Image img)
    {
        var desc = img.Description;
        var textureDescription = new Texture2DDescription
        {
            Width = desc.Width,
            Height = desc.Height,
            Format = desc.Format,
            MipLevels = desc.MipLevels,
            ArraySize = desc.ArraySize,
            SampleDescription = new()
            {
                Count = 1
            },
            BindFlags = BindFlags.ShaderResource,
            Usage = ResourceUsage.Immutable,
            CpuAccessFlags = CpuAccessFlags.None,
        };

        Texture2D tex;
        if (textureDescription.Format == Format.R8G8B8A8_UNorm_SRgb)
        {
            textureDescription.Format = Format.R8G8B8A8_Typeless;
            using (tex = new Texture2D(_device, textureDescription, img.ToDataBox()))
                return new ShaderResourceView(_device, tex, new()
                {
                    Format = Format.R8G8B8A8_UNorm,
                    Texture2D =
                    {
                        MipLevels = desc.MipLevels
                    },
                    Dimension = ShaderResourceViewDimension.Texture2D
                });
        }
        
        using (tex = new Texture2D(_device, textureDescription, img.ToDataBox()))
            return new ShaderResourceView(_device, tex);
    }

    private static SharpDX.Toolkit.Graphics.Image LoadFromPath(string path)
    {
        // for some reason Load(string path) uses just the Read method which might not always read the file fully
        // yet another keen moment I assume
        using var stream = File.OpenRead(path);
        return SharpDX.Toolkit.Graphics.Image.Load(stream, path);
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

    private class Image(ImageIdentifier identifier, ShaderResourceView srv, Vector2 size) : ImGuiImage
    {
        private bool _disposed;
        private long _lastUse = Stopwatch.GetTimestamp();

        public override ImTextureRef TextureId
        {
            get
            {
                OnUse();
                return new()
                {
                    _TexID = srv.NativePointer
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
            _lastUse = Stopwatch.GetTimestamp();
        }

        public override void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            srv.Dispose();
        }

        public override string ToString()
        {
            return $"Image {{ {identifier} {size} }}";
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