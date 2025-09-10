using System.Xml.Serialization;

namespace AutoRefundFM.Models;

[XmlRoot("r")]
public class WaterBreakResource<T>
{
    [XmlElement("w")]
    public T Item { get; set; }
}

public class WaterBreakModel
{
    [XmlAttribute("Id")]
    public string ResourceId { get; set; } = string.Empty;

    [XmlAttribute("h")]
    public int HomeValue { get; set; }

    [XmlAttribute("a")]
    public int AwayValue { get; set; }

    [XmlAttribute("o")]
    public string OtherInfo { get; set; } = string.Empty;

    [XmlAttribute("st")]
    public int StatusId { get; set; }

    [XmlAttribute("s")]
    public byte IsApply { get; set; } = 1;
}