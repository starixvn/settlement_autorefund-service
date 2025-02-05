using System.Xml.Serialization;

namespace AutoRefund.Core.Models;

public class BaseResourceItem
{
    [XmlAttribute("Id")]
    public string ResourceId { get; set; }

    [XmlAttribute("Home")]
    public string HomeValue { get; set; }

    [XmlAttribute("Away")]
    public string AwayValue { get; set; }

    [XmlAttribute("Remark")]
    public string Remark { get; set; }
}


public class ResourceItem : BaseResourceItem
{
    private string determined = "0";


    [XmlAttribute("OtherInfo")]
    public string OtherInfo { get; set; }

    [XmlAttribute("IsDetermined")]
    public string Determined
    {
        get => IsSettleDetermined ? "1" : determined;
        set => determined = value;
    }

    [XmlIgnore]
    public bool IsSettleDetermined { get; set; }
}


[XmlRoot("Resources")]
public class Resources<T>
{
    [XmlElement("Resource")]
    public List<T> ResourceItems { get; set; }
}