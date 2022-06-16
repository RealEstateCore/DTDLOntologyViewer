using System.Collections.Generic;
using VDS.RDF;

namespace DTDLOntologyViewer.DotNetRdfExtensions
{
    public static class NodeExtensions
    {
        public static IEnumerable<IUriNode> RdfTypes(this INode node)
        {
            IUriNode RDF_type = node.Graph.CreateUriNode(RDF.type);
            return node.Graph.GetTriplesWithSubjectPredicate(node, RDF_type).Objects().UriNodes();
        }
    }
}
