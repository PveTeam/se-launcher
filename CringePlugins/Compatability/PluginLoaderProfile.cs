using System.Xml.Serialization;

namespace CringePlugins.Compatability;

//https://github.com/sepluginloader/PluginLoader/blob/main/PluginLoader/Profile.cs
[XmlType("Profile")]
public class PluginLoaderProfile
{
    public string Key { get; set; } = "";
    public string Name { get; set; } = "";
    public string[] Plugins { get; set; } = [];
}
