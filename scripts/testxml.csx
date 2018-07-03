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

    InParams _systemParams = new InParams();
    [YAXElementFor("Header")]
    public InParams system => _systemParams;

    InParams _businessParams = new InParams();
    [YAXElementFor("Body")]
    public InParams business => _businessParams;

    public override string ToString()
    {
        YAXSerializer serializer = new YAXSerializer(typeof(InEnvelope));
        var doc = serializer.SerializeToXDocument(this);
        doc.Root.ReplaceAttributes(doc.Root.Attributes()
                                   .OrderByDescending(attr => attr.Name.Namespace.NamespaceName));
        return "<?xml version=\"1.0\" encoding=\"GBK\"?>" +
            doc.ToString(SaveOptions.DisableFormatting).Replace(" />", "/>");
    }
}

[YAXCustomSerializer(typeof(CustomInParamsSerializer))]
public class InParams : Dictionary<string, object>
{
    public IEnumerable<XElement> GetParamElements()
    {
        foreach (var (k, v) in this)
        {
            var para = new XElement("para");
            para.SetAttributeValue(k, v?.ToString() ?? "");
            yield return para;
        }
    }
}

public class CustomInParamsSerializer : ICustomSerializer<InParams>
{
    public void SerializeToAttribute(InParams objectToSerialize, XAttribute attrToFill)
    {
        throw new NotImplementedException();
    }

    public void SerializeToElement(InParams paras, XElement elemToFill)
    {
        XNamespace inNS = "http://www.molss.gov.cn/";
        elemToFill.SetAttributeValue(XNamespace.Xmlns + "in", inNS);
        elemToFill.Name = inNS + elemToFill.Name.LocalName;

        if (paras.Count == 0)
            elemToFill.Value = "";
        else
        {
            foreach (XElement param in paras.GetParamElements())
                elemToFill.Add(param);
        }
    }

    public string SerializeToValue(InParams objectToSerialize)
    {
        throw new NotImplementedException();
    }

    public InParams DeserializeFromAttribute(XAttribute attrib)
    {
        throw new NotImplementedException();
    }

    public InParams DeserializeFromElement(XElement element)
    {
        throw new NotImplementedException();
    }

    public InParams DeserializeFromValue(string value)
    {
        throw new NotImplementedException();
    }
}

var env = new InEnvelope();
env.system.Add("usr", "hqm");
env.system.Add("pwd", "YLZ_A2A5F63315129CB2998A0E0FCE31BA51");
env.system.Add("funid", "F00.00.00.00|192.168.1.110|PC-20170427DGON|00-05-0F-08-1A-34");

Console.WriteLine(env);
