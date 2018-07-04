using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using YAXLib;

namespace YHSBJ.SBGLPT
{
    public interface ICustomSerializable
    {
        void ToXAttribute(XAttribute attrToFill);
        void ToXElement(XElement elemToFill);
        string ToValue();
        void LoadXAttribute(XAttribute attrib);
        void LoadXElement(XElement element);
        void LoadValue(string value);
    }

    public class BaseCustomSerializable : ICustomSerializable
    {
        public virtual void ToXAttribute(XAttribute attrToFill)
        {
            throw new NotImplementedException();
        }
        public virtual void ToXElement(XElement elemToFill)
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
            serializable.ToXAttribute(attrToFill);
        }

        public void SerializeToElement(T serializable, XElement elemToFill)
        {
            serializable.ToXElement(elemToFill);
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

    [YAXSerializeAs("Envelope")]
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
    
        public override void ToXElement(XElement elemToFill)
        {
            var elem = new XElement(Namespace + Name);
            elem.SetAttributeValue(XNamespace.Xmlns + NamespacePrefix, Namespace);

            if (Params.Count == 0)
                elem.Value = "";
            else
            {
                foreach (var kv in Params)
                {
                    var para = new XElement("para");
                    para.SetAttributeValue(kv.Key, kv.Value?.ToString() ?? "");
                    elem.Add(para);
                }
            }
            elemToFill.Add(elem);
        }

        public override void LoadXElement(XElement element)
        {
            var elems = element.Elements().Where(e => e.Name.Namespace == Namespace);
            if (elems.Any())
            {
                var elem = elems.ElementAt(0);
                this.Name = elem.Name.LocalName;
                foreach (var el in elem.Elements("para"))
                {
                    foreach (var attr in el.Attributes())
                    {
                        this.Params.Add(attr.Name.LocalName, attr.Value);
                    }
                }
            }
        }
    }

    [YAXCustomSerializer(typeof(CustomSerializer<Output>))]
    public class Output : BaseCustomSerializable
    {
        public static XNamespace Namespace => "http://www.molss.gov.cn/";
        public static string NamespacePrefix => "out";

        public string Name { get; private set; }
        public List<Dictionary<string, object>> Results { get; set; } = new List<Dictionary<string, object>>();
        public ResultSet Resultset { get; set; } = new ResultSet();

        public class ResultSet
        {
            public string Name { get; set; }
            public List<Dictionary<string, object>> Rows = new List<Dictionary<string, object>>();
        }

        public Output(string name)
        {
            Name = name;
        }

        public Output() : this("") {}
    
        public override void LoadXElement(XElement element)
        {
            XElement elem = null;
            var elems = element.Elements().Where(e =>e.Name.Namespace == Namespace);
            if (elems.Any())
            {
                elem = elems.ElementAt(0);
                this.Name = elem.Name.LocalName;
            }
            else
            {
                elem = element;
                this.Name = "";
            }

            void populate(List<Dictionary<string, object>> list, XElement xelem, string selectName)
            {
                foreach (var el in xelem.Elements(selectName))
                {
                    var dict = new Dictionary<string, object>();
                    foreach (var attr in el.Attributes())
                        dict.Add(attr.Name.LocalName, attr.Value);
                    list.Add(dict);
                }
            }

            populate(this.Results, elem, "result");

            elems = elem.Elements("resultset");
            if (elems.Any())
            {
                var rset = elems.ElementAt(0);
                this.Resultset.Name = rset.Attribute("name")?.Value;
                populate(this.Resultset.Rows, rset, "row");
            }
        }
    }
}
