using System;
using System.Linq;
using System.Xml.Linq;
using YAXLib;

namespace YHSBJ.SBGLPT
{
    [YAXNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/")]
    public class Envelope
    {
        [YAXAttributeForClass()]
        public string encodingStyle => "http://schemas.xmlsoap.org/soap/encoding/";

        public Header Header => new Header();
    }

    public class Header
    {
        public System system => new System();
    }

    public class Para
    {
        public string usr { get; set; }
        public string pwd { get; set; }
        public string funid { get; set; }
    }

    [YAXCustomSerializer(typeof(CustomSystemSerializer))]
    public class System
    {
        public Para para => new Para();
    }

    public class CustomSystemSerializer : ICustomSerializer<System>
    {
        public void SerializeToAttribute(System objectToSerialize, XAttribute attrToFill)
        {
            throw new NotImplementedException();
        }

        public void SerializeToElement(System objectToSerialize, XElement elemToFill)
        {
            //string message = objectToSerialize.MessageText;
            //string beforeBold = message.Substring(0, objectToSerialize.BoldIndex);
            //string afterBold = message.Substring(objectToSerialize.BoldIndex + objectToSerialize.BoldLength);
            //
            //elemToFill.Add(new XText(beforeBold));
            //elemToFill.Add(new XElement("b", objectToSerialize.BoldContent));
            //elemToFill.Add(new XText(afterBold));
            throw new NotImplementedException();
        }

        public string SerializeToValue(System objectToSerialize)
        {
            throw new NotImplementedException();
        }

        public System DeserializeFromAttribute(XAttribute attrib)
        {
            throw new NotImplementedException();
        }

        public System DeserializeFromElement(XElement element)
        {
            throw new NotImplementedException();
        }

        public System DeserializeFromValue(string value)
        {
            throw new NotImplementedException();
        }

    }

}
