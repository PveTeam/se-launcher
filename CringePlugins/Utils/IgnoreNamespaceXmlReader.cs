using System.Xml;

namespace CringePlugins.Utils;
public sealed class IgnoreNamespaceXmlReader(Stream input) : XmlTextReader(input)
{
    public override string NamespaceURI => "";
}
