using SharpDX.DXGI;
using VRageRender;

namespace CringeLauncher.Platform;

internal class AdaptersList(Factory factory, Device device, SwapChain swapChain)
{
    public MyAdapterInfo[] Value => field ??= CreateAdaptersList();

    private MyAdapterInfo[] CreateAdaptersList()
    {
        var list = new List<MyAdapterInfo>();

        var adapters = factory.Adapters;
        for (var i = 0; i < adapters.Length; i++)
        {
            using var adapter = adapters[i];
            var adapterDescription = adapter.Description;

            var adapterName = TrimName(adapterDescription.Description, 24);

            var outputs = adapter.Outputs;
            for (var outputIndex = 0; outputIndex < outputs.Length; outputIndex++)
            {
                using var output = outputs[outputIndex];
                var outputDescription = output.Description;

                list.Add(new()
                {
                    Name = $"{adapterName} + {outputIndex + 1}",
                    DeviceName = adapterName,
                    Description = adapterName,
                    OutputName = outputDescription.DeviceName,
                    VendorId = (VendorIds)adapterDescription.VendorId,
                    AdapterDeviceId = i,
                    DeviceId = adapterDescription.DeviceId,
                    DesktopBounds = new(outputDescription.DesktopBounds.Left, outputDescription.DesktopBounds.Top,
                        outputDescription.DesktopBounds.Right, outputDescription.DesktopBounds.Bottom),
                    OutputId = outputIndex,
                    DesktopResolution = new(outputDescription.DesktopBounds.Right - outputDescription.DesktopBounds.Left,
                        outputDescription.DesktopBounds.Bottom - outputDescription.DesktopBounds.Top),
                    IsDx11Supported = true,
                    Has512MBRam = adapterDescription.DedicatedVideoMemory >= (nint)500000000,
                    Quality = MyRenderPresetEnum.NORMAL,
                    VRAM = (ulong)(nint)adapterDescription.DedicatedVideoMemory,
                    SVRAM = (ulong)(nint)adapterDescription.DedicatedVideoMemory,
                    IsOutputAttached = outputDescription.IsAttachedToDesktop,
                    SupportedDisplayModes = output.GetDisplayModeList(Format.R8G8B8A8_UNorm, DisplayModeEnumerationFlags.Interlaced)
                        .Select(mode => new MyDisplayMode(mode.Width, mode.Height, mode.RefreshRate.Numerator, mode.RefreshRate.Denominator))
                        .ToArray(),
                });
            }
        }

        return list.ToArray();
    }

    private static string TrimName(string str, int maxLength)
    {
        if (str.Length < maxLength) return str;
        var parts = str.Split(' ', StringSplitOptions.RemoveEmptyEntries).AsSpan();
        do
        {
            if (parts.IsEmpty) break;
            parts = parts[..^1];
        } while (PartsLength(parts) > maxLength);

        return parts.IsEmpty ? str : string.Join(" ", parts);

        static int PartsLength(Span<string> span)
        {
            var len = 0;
            foreach (var se in span)
            {
                len += se.Length;
                len++;
            }
            return len;
        } 
    }
}
