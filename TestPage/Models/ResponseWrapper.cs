using System;
using System.Xml.Serialization;

namespace TestPage.Models
{
    [XmlRoot("wrapper")]
    public class ResponseWrapper
    {
        [XmlAttribute("mode")]
        public string Mode { get; set; } = "merging-images-result";

        [XmlElement(ElementName = "response-merging-images")]
        public ResponseMergingImages ResponseMergingImages { get; set; } = new ResponseMergingImages();

    }

    public class ResponseMergingImages
    {
        [XmlAttribute("version")]
        public string Version { get; set; } = "0.01";

        [XmlElement(ElementName = "response")]
        public Response Response { get; set; } = new Response();
    }

    public class Response
    {
        [XmlAttribute("request-id")]
        public int RequestId { get; set; }

        [XmlAttribute("response-creation-date-time")]
        public string ResponseCreationDateTime { get; set; } = DateTime.Now.ToString("dd.MM.yyyy hh:mm.ss");

        [XmlAttribute("execution-time-msec")]
        public long ExecutionTimeMsec { get; set; }

        [XmlAttribute("result-a")]
        public double ResultA { get; set; }

        [XmlAttribute("result-p")]
        public double ResultP { get; set; }

        [XmlAttribute("result-d")]
        public double ResultD { get; set; }
    }
}
