#! "netcoreapp2.0"
#r "nuget: YAXLib, 2.15.0"
#r "nuget: System.Xml.Linq, 3.5.21022.801"

using System.Linq;
using System.Xml.Linq;
using YAXLib;

[YAXNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/")]
[YAXSerializeAs("Envelope")]
public class InEnvelope
{
    [YAXAttributeForClass()]
    public string encodingStyle => "http://schemas.xmlsoap.org/soap/encoding/";

    [YAXCustomSerializer(typeof(CustomInputSerializer))]
    public Input Header { get; } = new Input("system");

    [YAXCustomSerializer(typeof(CustomInputSerializer))]
    public Input Body { get; } = new Input("business");

    public override string ToString()
    {
        YAXSerializer serializer = new YAXSerializer(typeof(InEnvelope));
        var doc = serializer.SerializeToXDocument(this);
        doc.Root.ReplaceAttributes(doc.Root.Attributes()
                                   .OrderByDescending(attr => attr.Name.Namespace.NamespaceName));
        return "<?xml version=\"1.0\" encoding=\"GBK\"?>" +
            doc.ToString(SaveOptions.DisableFormatting).Replace(" />", "/>");
    }

    public static InEnvelope Load(string xml)
    {
       YAXSerializer serializer = new YAXSerializer(typeof(InEnvelope));
       return (InEnvelope)serializer.Deserialize(xml);
    }
}

public class Input
{
    public Input(string name)
    {
        Name = name;
        Params = new Dictionary<string, object>();
    }
    
    public string Name { get; }
    public Dictionary<string, object> Params { get; }

    public XElement ToXElement()
    {
        XNamespace inNS = "http://www.molss.gov.cn/";
        var elem = new XElement(inNS + Name);
        elem.SetAttributeValue(XNamespace.Xmlns + "in", inNS);

        if (Params.Count == 0)
            elem.Value = "";
        else
        {
            foreach (var (k, v) in Params)
            {
                var para = new XElement("para");
                para.SetAttributeValue(k, v?.ToString() ?? "");
                elem.Add(para);
            }
        }
        return elem;
    }
}

public class CustomInputSerializer : ICustomSerializer<Input>
{
    public void SerializeToAttribute(Input objectToSerialize, XAttribute attrToFill)
    {
        throw new NotImplementedException();
    }

    public void SerializeToElement(Input input, XElement elemToFill)
    {
        elemToFill.Add(input.ToXElement());
    }

    public string SerializeToValue(Input objectToSerialize)
    {
        throw new NotImplementedException();
    }

    public Input DeserializeFromAttribute(XAttribute attrib)
    {
        throw new NotImplementedException();
    }

    public Input DeserializeFromElement(XElement element)
    {
        throw new NotImplementedException();
    }

    public Input DeserializeFromValue(string value)
    {
        throw new NotImplementedException();
    }
}

var env = new InEnvelope();
env.Header.Params.Add("usr", "hqm");
env.Header.Params.Add("pwd", "YLZ_A2A5F63315129CB2998A0E0FCE31BA51");
env.Header.Params.Add("funid", "F00.00.00.00|192.168.1.110|PC-20170427DGON|00-05-0F-08-1A-34");

string xml = env.ToString();
Console.WriteLine(xml);

env = InEnvelope.Load(xml);
Console.WriteLine(env.encodingStyle);
foreach (var (k, v) in env.Header.Params)
{
    Console.WriteLine("{0}:{1}", k, v);
}

