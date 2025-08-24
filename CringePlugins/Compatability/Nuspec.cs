using System.Xml.Serialization;

namespace CringePlugins.Compatability;

[XmlRoot("package")]
public class Nuspec
{
    [XmlElement("metadata")]
    public NuspecMetadata Metadata { get; set; } = null!;
}

public class NuspecMetadata
{
    [XmlElement("id")]
    public string Id { get; set; } = null!;
    [XmlElement("version")]
    public string Version { get; set; } = null!;
    [XmlElement("authors")]
    public string Authors { get; set; } = null!;
    [XmlElement("projectUrl")]
    public string? ProjectUrl { get; set; }
    [XmlElement("icon")]
    public string? Icon { get; set; }
    [XmlElement("description")]
    public string? Description { get; set; }
    [XmlElement("title")]
    public string? Title { get; set; }

    [XmlArray("packageTypes")]
    [XmlArrayItem("packageType")]
    public List<NuspecPackageType>? PackageTypes { get; set; }

    [XmlElement("repository")]
    public NuspecRepository? Repository { get; set; }

    [XmlElement("dependencies")]
    public NuspecDependencies? Dependencies { get; set; }

    [XmlElement("frameworkReferences")]
    public NuspecFrameworkReferences? FrameworkReferences { get; set; }
}

public class NuspecPackageType
{
    [XmlAttribute("name")]
    public string Name { get; set; } = null!;
}
public class NuspecRepository
{
    [XmlAttribute("type")]
    public string Type { get; set; } = null!;

    [XmlAttribute("url")]
    public string Url { get; set; } = null!;

    [XmlAttribute("commit")]
    public string Commit { get; set; } = null!;
}

public class NuspecDependencies
{
    [XmlElement("group")]
    public List<NuspecDependencyGroup> Groups { get; set; } = [];
}

public class NuspecDependencyGroup
{
    [XmlAttribute("targetFramework")]
    public string TargetFramework { get; set; } = null!;

    [XmlElement("dependency")]
    public List<NuspecDependency>? Dependencies { get; set; }
}

public class NuspecDependency
{
    [XmlAttribute("id")]
    public string Id { get; set; } = null!;

    [XmlAttribute("version")]
    public string Version { get; set; } = null!;

    [XmlAttribute("exclude")]
    public string Exclude { get; set; } = null!;
}

public class NuspecFrameworkReferences
{
    [XmlElement("group")]
    public List<NuspecFrameworkReferenceGroup> Groups { get; set; } = [];
}

public class NuspecFrameworkReferenceGroup
{
    [XmlAttribute("targetFramework")]
    public string TargetFramework { get; set; } = null!;

    [XmlElement("frameworkReference")]
    public List<NuspecFrameworkReference> References { get; set; } = [];
}

public class NuspecFrameworkReference
{
    [XmlAttribute("name")]
    public string Name { get; set; } = null!;
}