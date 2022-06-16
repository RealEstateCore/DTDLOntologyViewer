using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;

namespace DTDLOntologyViewer.DotNetRdfExtensions.Models
{
    public class DTDLInterfaceContent : DTDLClass
    {
        protected internal DTDLInterfaceContent(INode node, IGraph graph) : base(node, graph)
        {
        }

        public string Name
        {
            get
            {
                IUriNode dtdlName = Graph.CreateUriNode(DTDL.name);
                return Graph.GetTriplesWithSubjectPredicate(Node, dtdlName).Objects().LiteralNodes().Select(node => node.Value).First();
            }
        }

        public override string ToString()
        {
            return DisplayName ?? Name;
        }
    }
}
