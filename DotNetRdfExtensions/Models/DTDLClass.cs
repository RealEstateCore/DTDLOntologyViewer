using System.Collections.Generic;
using System.Linq;
using VDS.RDF;

namespace DTDLOntologyViewer.DotNetRdfExtensions.Models
{
    public class DTDLClass
    {
        protected INode _node;
        protected IGraph _graph;

        protected internal DTDLClass(INode node, IGraph graph)
        {
            _node = node;
            _graph = graph;
        }

        public INode Node
        {
            get
            {
                return _node;
            }
        }

        public IGraph Graph
        {
            get
            {
                return _graph;
            }
        }

        public string? DisplayName
        {
            get
            {
                // Get all display name nodes that lack language tag followed by those in English; return the first
                IEnumerable<ILiteralNode> displayNameNodes = DisplayNames.Where(node => string.IsNullOrEmpty(node.Language)).Concat(DisplayNames.Where(node => node.Language.Substring(0, 2).ToLower().Equals("en")));
                if (displayNameNodes.Any())
                {
                    return displayNameNodes.First().Value;
                }

                return null;
            }
        }

        public string? Description
        {
            get
            {
                // Get all description nodes that lack language tag followed by those in English; return the first
                IEnumerable<ILiteralNode> descriptionNodes = Descriptions.Where(node => string.IsNullOrEmpty(node.Language)).Concat(Descriptions.Where(node => node.Language.Substring(0, 2).ToLower().Equals("en")));
                if (descriptionNodes.Any())
                {
                    return descriptionNodes.First().Value;
                }

                return null;
            }
        }

        public IEnumerable<ILiteralNode> DisplayNames
        {
            get
            {
                IUriNode dtdlDisplayName = Graph.CreateUriNode(DTDL.displayName);
                return Graph.GetTriplesWithSubjectPredicate(Node, dtdlDisplayName).Objects().LiteralNodes();
            }
        }

        public IEnumerable<ILiteralNode> Descriptions
        {
            get
            {
                IUriNode dtdlDescription = Graph.CreateUriNode(DTDL.description);
                return Graph.GetTriplesWithSubjectPredicate(Node, dtdlDescription).Objects().LiteralNodes();
            }
        }
    }
}
