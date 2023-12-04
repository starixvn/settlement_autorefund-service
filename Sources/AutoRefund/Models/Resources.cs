using System.Xml.Serialization;

namespace AutoRefund.Models;

[XmlRoot("Resources")]
public class Resources
{
    [XmlElement("Resource")]
    public List<ResourceItem> ResourceItems { get; set; }
}