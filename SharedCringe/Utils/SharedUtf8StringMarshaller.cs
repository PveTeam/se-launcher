using System.Runtime.InteropServices.Marshalling;

namespace SharedCringe.Utils;

[CustomMarshaller(typeof(string), MarshalMode.Default, typeof(SharedUtf8StringMarshaller))]
public static unsafe class SharedUtf8StringMarshaller
{
    public static string? ConvertToManaged(byte* unmanaged) => Utf8StringMarshaller.ConvertToManaged(unmanaged);

    public static byte* ConvertToUnmanaged(string? managed) => Utf8StringMarshaller.ConvertToUnmanaged(managed);
    
    public static void Free(byte* unmanaged) {}
}
