using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace DTDLOntologyViewer.DotNetRdfExtensions.Models
{
    public class DTDLInterface: DTDLClass
    {
        public DTDLInterface(IUriNode node, IGraph graph) : base(node, graph)
        {
        }

        new public IUriNode Node
        {
            get
            {
                return (IUriNode)_node;
            }
        }

        public string Dtmi
        {
            get
            {
                return Node.Uri.ToString();
            }
        }

        public IEnumerable<DTDLInterfaceContent> Contents
        {
            get
            {
                IUriNode DTDL_contents = Graph.CreateUriNode(DTDL.contents);
                IUriNode DTDL_Property = Graph.CreateUriNode(DTDL.Property);
                IUriNode DTDL_Relationship = Graph.CreateUriNode(DTDL.Relationship);
                foreach (INode contentNode in Graph.GetTriplesWithSubjectPredicate(Node, DTDL_contents).Objects())
                {
                    IEnumerable<IUriNode> rdfTypes = contentNode.RdfTypes();
                    if (rdfTypes.Any(contentNodeType => contentNodeType.Equals(DTDL_Property)))
                    {
                        yield return new DTDLProperty(contentNode, Graph);
                    }
                    if (rdfTypes.Any(contentNodeType => contentNodeType.Equals(DTDL_Relationship)))
                    {
                        yield return new DTDLRelationship(contentNode, Graph);
                    }
                    // TODO: Continue with the other available content types
                }
            }
        }

        public IEnumerable<DTDLInterface> Extends
        {
            get
            {
                IUriNode dtdlExtends = _graph.CreateUriNode(DTDL.extends);
                IEnumerable<IUriNode> parentNodes = _graph.GetTriplesWithSubjectPredicate(Node, dtdlExtends).Objects().UriNodes();
                IEnumerable<DTDLInterface> parentInterfaces = parentNodes.Select(parentNode => new DTDLInterface(parentNode, Graph));
                return parentInterfaces;
            }
        }

        public IEnumerable<DTDLInterface> ExtendedBy
        {
            get
            {
                IUriNode dtdlExtends = _graph.CreateUriNode(DTDL.extends);
                IEnumerable<IUriNode> childNodes = _graph.GetTriplesWithPredicateObject(dtdlExtends, _node).Subjects().UriNodes();
                IEnumerable<DTDLInterface> childInterfaces = childNodes.Select(childNode => new DTDLInterface(childNode, Graph));
                return childInterfaces;
            }
        }

        public override string ToString()
        {
            return DisplayName ?? Node.Uri.AbsoluteUri.Split(':').Last().Split(';').First();
        }
    }
}
