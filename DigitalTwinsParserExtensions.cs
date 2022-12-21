using Microsoft.Azure.DigitalTwins.Parser.Models;
using System.Collections.Generic;
using System.Linq;

namespace DTDLOntologyViewer
{
    public static class DigitalTwinsParserExtensions
    {
        public static IEnumerable<DTInterfaceInfo> TransitiveExtends(this DTInterfaceInfo iface)
        {
            return iface.Extends.Concat(iface.Extends.SelectMany(parent => parent.TransitiveExtends()));
        }
        public static IEnumerable<DTComponentInfo> InheritedComponents(this DTInterfaceInfo iface)
        {
            return iface.Contents.Values
                .Where(content => content is DTComponentInfo && content.DefinedIn != iface.Id)
                .Select(content => (DTComponentInfo)content);
        }

        public static IEnumerable<DTComponentInfo> DirectComponents(this DTInterfaceInfo iface)
        {
            return iface.Contents.Values
                .Where(content => content is DTComponentInfo && content.DefinedIn == iface.Id)
                .Select(content => (DTComponentInfo)content);
        }
        public static IEnumerable<DTPropertyInfo> InheritedProperties(this DTInterfaceInfo iface)
        {
            return iface.Contents.Values
                .Where(content => content is DTPropertyInfo && content.DefinedIn != iface.Id)
                .Select(content => (DTPropertyInfo)content);
        }

        public static IEnumerable<DTPropertyInfo> DirectProperties(this DTInterfaceInfo iface)
        {
            return iface.Contents.Values
                .Where(content => content is DTPropertyInfo && content.DefinedIn == iface.Id)
                .Select(content => (DTPropertyInfo)content);
        }
        public static IEnumerable<DTRelationshipInfo> InheritedRelationships(this DTInterfaceInfo iface)
        {
            return iface.Contents.Values
                .Where(content => content is DTRelationshipInfo && content.DefinedIn != iface.Id)
                .Select(content => (DTRelationshipInfo)content);
        }

        public static IEnumerable<DTRelationshipInfo> DirectRelationships(this DTInterfaceInfo iface)
        {
            return iface.Contents.Values
                .Where(content => content is DTRelationshipInfo && content.DefinedIn == iface.Id)
                .Select(content => (DTRelationshipInfo)content);
        }
    }
}
