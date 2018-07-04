#! "netcoreapp2.0"
#r "nuget: YAXLib, 2.15.0"
#r "nuget: System.Xml.Linq, 3.5.21022.801"

using System.Linq;
using System.Xml.Linq;
using YAXLib;

public interface ICustomSerializable
{
    string ToXAttributeValue();
    XElement ToXElement();
    string ToValue();
    void LoadXAttribute(XAttribute attrib);
    void LoadXElement(XElement element);
    void LoadValue(string value);
}

public class BaseCustomSerializable : ICustomSerializable
{
    public virtual string ToXAttributeValue()
    {
        throw new NotImplementedException();
    }
    public virtual XElement ToXElement()
    {
        throw new NotImplementedException();
    }
    public virtual string ToValue()
    {
        throw new NotImplementedException();
    }
    public virtual void LoadXAttribute(XAttribute attrib)
    {
        throw new NotImplementedException();
    }
    public virtual void LoadXElement(XElement element)
    {
        throw new NotImplementedException();
    }
    public virtual void LoadValue(string value)
    {
        throw new NotImplementedException();
    }
}

public class CustomSerializer<T> : ICustomSerializer<T> where T : ICustomSerializable, new()
{
    public void SerializeToAttribute(T serializable, XAttribute attrToFill)
    {
        attrToFill.Value = serializable.ToXAttributeValue();
    }

    public void SerializeToElement(T serializable, XElement elemToFill)
    {
        elemToFill.Add(serializable.ToXElement());
    }

    public string SerializeToValue(T serializable)
    {
        return serializable.ToValue();
    }

    public T DeserializeFromAttribute(XAttribute attrib)
    {
        var serializable = new T();
        serializable.LoadXAttribute(attrib);
        return serializable;
    }

    public T DeserializeFromElement(XElement element)
    {
        var serializable = new T();
        serializable.LoadXElement(element);
        return serializable;
    }

    public T DeserializeFromValue(string value)
    {
        var serializable = new T();
        serializable.LoadValue(value);
        return serializable;
    }
}

[YAXNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/")]
public class Envelope<TCustom>
{
    [YAXAttributeForClass()]
    public string encodingStyle { get; set; } = "http://schemas.xmlsoap.org/soap/encoding/";
    
    public TCustom Header { get; set; }

    public TCustom Body { get; set; }

    public override string ToString()
    {
        YAXSerializer serializer = new YAXSerializer(typeof(Envelope<TCustom>));
        var doc = serializer.SerializeToXDocument(this);
        doc.Root.ReplaceAttributes(doc.Root.Attributes()
                                   .OrderByDescending(attr => attr.Name.Namespace.NamespaceName));
        return "<?xml version=\"1.0\" encoding=\"GBK\"?>" +
            doc.ToString(SaveOptions.DisableFormatting).Replace(" />", "/>");
    }

    public static Envelope<TCustom> Load(string xml)
    {
       YAXSerializer serializer = new YAXSerializer(typeof(Envelope<TCustom>));
       return (Envelope<TCustom>)serializer.Deserialize(xml);
    }
}

[YAXCustomSerializer(typeof(CustomSerializer<Input>))]
public class Input : BaseCustomSerializable
{
    public static XNamespace Namespace => "http://www.molss.gov.cn/";
    public static string NamespacePrefix => "in";

    public string Name { get; private set; }
    public Dictionary<string, object> Params { get; } = new Dictionary<string, object>();

    public Input(string name)
    {
        Name = name;
    }

    public Input() : this("") {}
    
    public override XElement ToXElement()
    {
        var elem = new XElement(Namespace + Name);
        elem.SetAttributeValue(XNamespace.Xmlns + NamespacePrefix, Namespace);

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

    public override void LoadXElement(XElement element)
    {
        var elem = element.Elements().First(e => e.Name.Namespace == Namespace);
        if (elem != null)
        {
            this.Name = elem.Name.LocalName;
            foreach (var el in elem.Elements("para"))
            {
                foreach (var attr in el.Attributes())
                {
                    this.Params.Add(attr.Name.LocalName, attr.Value);
                }
            }
        }
        return;
    }
}

var env = new Envelope<Input>
{
    Header = new Input("system"),
    Body  = new Input("business")
};
env.Header.Params.Add("usr", "hqm");
env.Header.Params.Add("pwd", "YLZ_A2A5F63315129CB2998A0E0FCE31BA51");
env.Header.Params.Add("funid", "F00.00.00.00|192.168.1.110|PC-20170427DGON|00-05-0F-08-1A-34");

string xml = env.ToString();
Console.WriteLine(xml);

env = Envelope<Input>.Load(xml);
Console.WriteLine(env.encodingStyle);
foreach (var (k, v) in env.Header.Params)
{
    Console.WriteLine("{0}:{1}", k, v);
}
