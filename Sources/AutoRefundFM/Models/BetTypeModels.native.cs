using System.Xml.Serialization;

namespace AutoRefundFM.Models;


[XmlRoot("BetTypes")]
public class BetTypesRoot
{
    [XmlElement("BetType")]
    public List<BetTypeElement> BetType { get; set; }
}

public class BetTypeElement
{
    [XmlAttribute]
    public int Id { get; set; }

    [XmlElement("Item")]
    public List<ItemElement> Items { get; set; }
}

public class ItemElement
{
    [XmlAttribute]
    public int Position { get; set; }

    [XmlAttribute]
    public string Result { get; set; }
}