using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace DTDLOntologyViewer.DotNetRdfExtensions
{
    public static class TripleExtensions
    {
        public static IEnumerable<INode> Subjects(this IEnumerable<Triple> triples)
        {
            return triples.Select(triple => triple.Subject);
        }

        public static IEnumerable<INode> Predicates(this IEnumerable<Triple> triples)
        {
            return triples.Select(triple => triple.Predicate);
        }

        public static IEnumerable<INode> Objects(this IEnumerable<Triple> triples)
        {
            return triples.Select(triple => triple.Object);
        }
    }
}
