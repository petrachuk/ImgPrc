using System.Xml.Serialization;

namespace TestPage.Models
{
    [XmlRoot("wrapper")]
    public class RequestWrapper
    {
        [XmlAttribute("mode")]
        public string Mode { get; set; }

        [XmlElement(ElementName = "request-merging-images")]
        public RequestMergingImages RequestMergingImages { get; set; }

    }

    public class RequestMergingImages
    {
        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlElement(ElementName = "request")]
        public Request Request { get; set; }
    }

    public class Request
    {
        [XmlAttribute("request-id")]
        public int RequestId { get; set; }
        [XmlAttribute("url-image-1")]
        public string UrlImage1 { get; set; }
        [XmlAttribute("url-image-2")]
        public string UrlImage2 { get; set; }
    }
}
