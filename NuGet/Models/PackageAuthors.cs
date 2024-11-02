using System.Collections.Immutable;
using System.Text.Json.Serialization;
using NuGet.Converters;

namespace NuGet.Models;

[JsonConverter(typeof(PackageAuthorsJsonConverter))]
public record PackageAuthors(string Author, ImmutableArray<string> Authors);