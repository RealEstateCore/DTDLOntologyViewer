using DTDLOntologyViewer.DotNetRdfExtensions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTDLOntologyViewer.DotNetRdfExtensions
{
    public static class DTDLClassExtensions
    {
        public static IEnumerable<DTDLProperty> Properties (this IEnumerable<DTDLInterfaceContent> contents)
        {
            foreach (var content in contents)
            {
                if (content is DTDLProperty)
                {
                    yield return (DTDLProperty)content;
                }
            }
        }

        public static IEnumerable<DTDLRelationship> Relationships(this IEnumerable<DTDLInterfaceContent> contents)
        {
            foreach (var content in contents)
            {
                if (content is DTDLRelationship)
                {
                    yield return (DTDLRelationship)content;
                }
            }
        }
    }
}
