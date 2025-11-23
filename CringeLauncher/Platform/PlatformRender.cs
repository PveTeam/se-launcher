using System.Diagnostics.CodeAnalysis;
using CringeLauncher.Render;
using NLog;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using VRage;
using VRage.Platform.Windows.Render;
using VRageRender;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device1 = SharpDX.Direct3D11.Device1;

namespace CringeLauncher.Platform;

internal class PlatformRender(VRageWindowSurrogate surrogate) : IVRageRender
{
    private const ulong RwTexturesVRamPool = 104857600 /*0x06400000*/;
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private ulong _streamedResourcesMemoryBudget = MyVRage.Platform.System.GetTotalPhysicalMemory() / 5UL;
    private readonly ulong _generatedTexturesMemoryBudget = MyVRage.Platform.System.GetTotalPhysicalMemory() / 32UL;
    private ulong _voxelTextureArraysMemoryBudget = MyVRage.Platform.System.GetTotalPhysicalMemory() / 10UL;
    
    private MyRenderDeviceSettings? _currentSettings;
    
    public void CreateRenderDevice(ref MyRenderDeviceSettings? settings, [UnscopedRef] out object? deviceInstance,
        [UnscopedRef] out object? swapChain)
    {
        deviceInstance = MyPlatformRender.DeviceInstance = surrogate.Window.DeviceInstance?.QueryInterface<Device1>();
        swapChain = MyPlatformRender.m_swapchain = surrogate.Window.SwapChainInstance?.QueryInterface<SwapChain>();
        MyPlatformRender.m_factory = surrogate.Window.SwapChainInstance?.GetParent<Factory>();
        
        var settingsValue = settings ?? MyPlatformRender.GetDefaultDeviceSettings();
        settingsValue.NewAdapterOrdinal = settingsValue.AdapterOrdinal = MyPlatformRender.ValidateAdapterIndex(settingsValue.AdapterOrdinal);
        MyPlatformRender.GetAdapter(settingsValue.AdapterOrdinal, out var adapter, out var adapterInfo);
        MyPlatformRender.FixSettings(ref settingsValue, adapter, adapterInfo, MyPlatformRender.GetAdaptersList());

        if (settingsValue.WindowMode != MyWindowModeEnum.FullscreenWindow || surrogate.Window.ClientSize !=
            new Size(settingsValue.BackBufferWidth, settingsValue.BackBufferHeight))
        {
            ImGuiHandler.Rtv?.Dispose();
            ImGuiHandler.Rtv = null;
            MyPlatformRender.m_swapchain!.ResizeBuffers(2, settingsValue.BackBufferWidth,
                settingsValue.BackBufferHeight,
                Format.Unknown, SwapChainFlags.AllowModeSwitch);
        }
        
        AdjustMemoryBudgets(adapterInfo);
        
        settings = settingsValue;
    }

    public void DisposeRenderDevice()
    {
        MyPlatformRender.DisposeRenderDevice();
    }

    public void SuspendRenderContext()
    {
    }

    public void ResumeRenderContext()
    {
    }

    public MyRenderPresetEnum GetRenderQualityHint() => MyRenderPresetEnum.NORMAL;

    public MyAdapterInfo[] GetRenderAdapterList() => MyPlatformRender.GetAdaptersList();

    public void ApplyRenderSettings(MyRenderDeviceSettings? settings)
    {
        surrogate.Window.OwnsSwapChain = false;
        MyPlatformRender.ApplySettings(settings);
        if (settings is null) return;
        
        var settingsValue = settings.Value;
        if (_currentSettings.HasValue && _currentSettings.Value.Equals(ref settingsValue)) return;
        _currentSettings = settings;

        var adapterInfo = MyPlatformRender.GetAdaptersList()[settingsValue.NewAdapterOrdinal];
        var desktopBounds = adapterInfo.DesktopBounds;
        surrogate.Window.ResizeFullScreen((FullScreenMode)settingsValue.WindowMode,
            new Rectangle(desktopBounds.X, desktopBounds.Y, desktopBounds.Width, desktopBounds.Height),
            new Size(settingsValue.BackBufferWidth, settingsValue.BackBufferHeight));
        
        AdjustMemoryBudgets(adapterInfo);
    }

    private void AdjustMemoryBudgets(MyAdapterInfo adapter)
    {
        var totalPhysicalMemory = MyVRage.Platform.System.GetTotalPhysicalMemory();
        if (adapter.VendorId == VendorIds.Intel && !adapter.DeviceName.Contains("Arc"))
        {
            _voxelTextureArraysMemoryBudget = MyRenderProxy.Settings.VoxelTexturesStreamingPool = (ulong) (totalPhysicalMemory / 10.0);
            var leftOver = adapter.SVRAM - MyRenderProxy.Settings.VoxelTexturesStreamingPool - RwTexturesVRamPool;
            _streamedResourcesMemoryBudget = MyRenderProxy.Settings.ResourceStreamingPool = (ulong) (leftOver * 0.25);
        }
        else
        {
            _voxelTextureArraysMemoryBudget = MyRenderProxy.Settings.VoxelTexturesStreamingPool = (ulong) Math.Min(adapter.VRAM * 0.4, totalPhysicalMemory / 10.0);
            var leftOver = adapter.VRAM - MyRenderProxy.Settings.VoxelTexturesStreamingPool - RwTexturesVRamPool;
            _streamedResourcesMemoryBudget = MyRenderProxy.Settings.ResourceStreamingPool = Math.Min(leftOver, totalPhysicalMemory / 2UL);
        }
    }

    public object? CreateRenderAnnotation(object deviceContext)
    {
        try
        {
            return ((ComObject) deviceContext).QueryInterface<UserDefinedAnnotation>();
        }
        catch (Exception ex)
        {
            Log.Warn(ex, "Annotations for render context are not available");
        }
        return null;
    }

    public ulong GetMemoryBudgetForStreamedResources() => _streamedResourcesMemoryBudget;

    public void RequestSuspendWait()
    {
    }

    public void SetMemoryUsedForImprovedGFX(long bytes)
    {
    }

    public void FlushIndirectArgsFromComputeShader(object deviceContext)
    {
    }

    public ulong GetMemoryBudgetForGeneratedTextures() => _generatedTexturesMemoryBudget;

    public ulong GetMemoryBudgetForVoxelTextureArrays() => _voxelTextureArraysMemoryBudget;

    public void CustomUpdateForDeferredBuffer(object deviceContext, object buffer)
    {
    }

    public void SubmitEmptyCustomContext(object deviceContext)
    {
    }

    public void FastVSSetConstantBuffer(object deviceContext, int slot, object buffer)
    {
        ((DeviceContext) deviceContext).VertexShader.SetConstantBuffer(slot, (Buffer) buffer);
    }

    public void FastGSSetConstantBuffer(object deviceContext, int slot, object buffer)
    {
        ((DeviceContext) deviceContext).GeometryShader.SetConstantBuffer(slot, (Buffer) buffer);
    }

    public void FastPSSetConstantBuffer(object deviceContext, int slot, object buffer)
    {
        ((DeviceContext) deviceContext).PixelShader.SetConstantBuffer(slot, (Buffer) buffer);
    }

    public void FastCSSetConstantBuffer(object deviceContext, int slot, object buffer)
    {
        ((DeviceContext) deviceContext).ComputeShader.SetConstantBuffer(slot, (Buffer) buffer);
    }

    public void FastVSSetConstantBuffers1(object deviceContext, int slot, object buffer, int offset, int size,
        ref object? constantBindingsCache)
    {
        constantBindingsCache ??= Tuple.Create(new Buffer[1], new int[1], new int[1]);

        var (constantBuffersOut, firstConstantRef, numConstantsRef) =
            (Tuple<Buffer[], int[], int[]>)constantBindingsCache;
        constantBuffersOut[0] = (Buffer) buffer;
        firstConstantRef[0] = offset;
        numConstantsRef[0] = size;
        ((DeviceContext1) deviceContext).VSSetConstantBuffers1(slot, 1, constantBuffersOut, firstConstantRef, numConstantsRef);
    }

    public void FastPSSetConstantBuffers1(object deviceContext, int slot, object buffer, int offset, int size,
        ref object? constantBindingsCache)
    {
        constantBindingsCache ??= Tuple.Create(new Buffer[1], new int[1], new int[1]);

        var (constantBuffersOut, firstConstantRef, numConstantsRef) =
            (Tuple<Buffer[], int[], int[]>)constantBindingsCache;
        constantBuffersOut[0] = (Buffer) buffer;
        firstConstantRef[0] = offset;
        numConstantsRef[0] = size;
        ((DeviceContext1) deviceContext).PSSetConstantBuffers1(slot, 1, constantBuffersOut, firstConstantRef, numConstantsRef);
    }

    public void SetDepthTextureHint(VRageRender_DepthTextureHintType hint, object? deviceContext = null, object? texture = null)
    {
    }

    public bool IsExclusiveTextureLoadRequired() => false;

    public bool ForceClearGBuffer => true;
    public bool UseParallelRenderInit => false;
    public bool IsRenderOutputDebugSupported => true;
    public event Action? OnResuming;
    public event Action? OnSuspending;
}
