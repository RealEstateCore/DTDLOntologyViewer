using DTDLOntologyViewer.DotNetRdfExtensions.Models;
using System.Collections.Generic;
using VDS.RDF;

namespace DTDLOntologyViewer.DotNetRdfExtensions
{
    public static class GraphExtensions
    {
        public static IEnumerable<DTDLInterface> GetDtdlInterfaces(this IGraph graph)
        {
            IUriNode RDF_type = graph.CreateUriNode(RDF.type);
            IUriNode DTDL_Interface = graph.CreateUriNode(DTDL.Interface);
            foreach (IUriNode interfaceNode in graph.GetTriplesWithPredicateObject(RDF_type, DTDL_Interface).Subjects().UriNodes())
            {
                yield return new DTDLInterface(interfaceNode, graph);
            }
        }

        public static bool ContainsTriple(this IGraph graph, INode subject, INode predicate, INode obj)
        {
            return graph.ContainsTriple(new Triple(subject, predicate, obj));
        }

        public static bool ContainsDtdlInterface(this IGraph graph, DTDLInterface iface)
        {
            IUriNode RDF_type = graph.CreateUriNode(RDF.type);
            IUriNode DTDL_Interface = graph.CreateUriNode(DTDL.Interface);
            return graph.ContainsTriple(iface.Node, RDF_type, DTDL_Interface);
        }
    }
}