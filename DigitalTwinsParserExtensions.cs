using Microsoft.Azure.DigitalTwins.Parser.Models;
using System.Collections.Generic;
using System.Linq;

namespace DTDLOntologyViewer
{
    public static class DigitalTwinsParserExtensions
    {
        public static IEnumerable<DTRelationshipInfo> Relationships(this DTInterfaceInfo iface)
        {
            return iface.Contents.Values.Where(content => content is DTRelationshipInfo).Select(content => (DTRelationshipInfo)content);
        }

        public static IEnumerable<DTPropertyInfo> Properties(this DTInterfaceInfo iface)
        {
            return iface.Contents.Values.Where(content => content is DTPropertyInfo).Select(content => (DTPropertyInfo)content);
        }
    }
}
