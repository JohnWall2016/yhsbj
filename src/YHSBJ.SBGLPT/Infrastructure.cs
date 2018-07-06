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

    public class ParamsDict : Dictionary<string, string> {}

    [YAXCustomSerializer(typeof(CustomSerializer<Input>))]
    public class Input : BaseCustomSerializable
    {
        public static XNamespace Namespace => "http://www.molss.gov.cn/";
        public static string NamespacePrefix => "in";

        public string Name { get; private set; }
        public ParamsDict Params { get; } = new ParamsDict();

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

        public void Add(string key, string value)
        {
            Params.Add(key, value);
        }
    }

    public class ResultDict : Dictionary<string, string>
    {
        public List<ResultDict> SubItems { get; set; }
    }

    [YAXCustomSerializer(typeof(CustomSerializer<Output>))]
    public class Output : BaseCustomSerializable
    {
        public static XNamespace Namespace => "http://www.molss.gov.cn/";
        public static string NamespacePrefix => "out";

        public string Name { get; private set; }
        public List<ResultDict> Results { get; } = new List<ResultDict>();
        public ResultSet Resultset { get; } = new ResultSet();

        public class ResultSet
        {
            public string Name { get; set; }
            public List<ResultDict> Rows { get; } = new List<ResultDict>();
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

            void populate(List<ResultDict> list, XElement xelem, string selectName)
            {
                foreach (var el in xelem.Elements(selectName))
                {
                    var dict = new ResultDict();
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

    public class MetaDict : Dictionary<string, string>
    {
        public MetaDict() {}
        
        public MetaDict(string[] keys, string []values)
        {
            int minLen = keys.Length <= values.Length ? keys.Length
                : values.Length;
            for (var i = 0; i < minLen; i++)
                this.Add(keys[i], values[i]?.Trim() ?? "");
        }

        public static MetaDict Fetch(Session session, string funid, string functionid)
        {
            session.SendInput(inEnv =>
            {
                inEnv.Header.Add("funid", funid);
                inEnv.Body.Add("functionid", functionid);
            });
            var output = session.GetOutput();

            string en = null;
            string cn = null;
            
            foreach (var rs in output.Body.Results)
            {
                if (rs.ContainsKey("resultfielden"))
                    en = rs["resultfielden"];
                if (rs.ContainsKey("resultfieldcn"))
                    cn = rs["resultfieldcn"];
            }

            if (en == null && cn == null)
            {
                var rset = output.Body.Resultset;
                if (rset.Rows.Count > 0)
                {
                    var row = rset.Rows[0];
                    if (row.ContainsKey("resultfielden") &&
                        row.ContainsKey("resultfieldcn"))
                    {
                        en = row["resultfielden"];
                        cn = row["resultfieldcn"];
                    }
                }
            }
            if (en != null && cn != null)
                return new MetaDict(en.Split(','), cn.Split(','));
            return null;
        }
        
        public string Get(string key)
        {
            if (TryGetValue(key, out var meta))
                return meta;
            return "";
        }

        public string GetOrWithPrefix(string key, string prefix = "v.")
        {
            var str = Get(prefix + key);
            if (str != "")
                return str;
            return Get(key);
        }
    }

    public class SessionAction
    {
        public SessionAction(Session session)
        {
            S = session;
        }

        public Session S { get; }
    }

    public class SessionActionWithMetaData : SessionAction
    {
        public SessionActionWithMetaData(Session session, string funid, string functionid) : base(session)
        {
            _funid = funid;
            _functionid = functionid;
        }

        string _funid;
        string _functionid;
        
        MetaDict _metaData;
        public MetaDict MetaData
        {
            get
            {
                if (_metaData == null)
                {
                    _metaData = MetaDict.Fetch(S, _funid, _functionid);
                    if (_metaData == null)
                        _metaData = new MetaDict();
                }
                return _metaData;
            }
        }
    }
}
