using NuGet.Versioning;

namespace NuGet.Models;

public record RegistrationPage(int Count, NuGetVersion Lower, NuGetVersion Upper, RegistrationEntry[] Items);