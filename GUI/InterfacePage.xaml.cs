using Microsoft.Azure.DigitalTwins.Parser.Models;
using Microsoft.Msagl.Core.Geometry;
using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.Layout.Layered;
using Microsoft.Msagl.Miscellaneous;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace DTDLOntologyViewer.GUI
{
    public sealed partial class InterfacePage : Page
    {
        ObservableCollection<KeyValuePair<string,string>> DisplayNameCollection = new();
        ObservableCollection<KeyValuePair<string, string>> DescriptionCollection = new();
        ObservableCollection<DTComponentInfo> DirectComponentsCollection = new();
        ObservableCollection<DTComponentInfo> InheritedComponentsCollection = new();
        ObservableCollection<DTPropertyInfo> DirectPropertiesCollection = new();
        ObservableCollection<DTPropertyInfo> InheritedPropertiesCollection = new();
        ObservableCollection<DTRelationshipInfo> DirectRelationshipsCollection = new();
        ObservableCollection<DTRelationshipInfo> InheritedRelationshipsCollection = new();
        ObservableCollection<DTInterfaceInfo> ExtendsCollection = new();
        private MainWindow MainWindow
        {
            get => (Application.Current as App)!.Window!;
        }
        private DTInterfaceInfo? _selectedInterface;
        public DTInterfaceInfo? SelectedInterface
        {
            get => _selectedInterface;
            set
            {
                _selectedInterface = value;
                ClearFields();
                if (_selectedInterface != null)
                {
                    PopulateFields();
                }
            }
        }

        public InterfacePage()
        {
            this.InitializeComponent();
        }

        private void ClearFields()
        {
            FormHeader.Text = "";
            DtmiTextBlock.Text = string.Empty;
            ExtendsCollection.Clear();
            DisplayNameCollection.Clear();
            DescriptionCollection.Clear();
            DirectComponentsCollection.Clear();
            InheritedComponentsCollection.Clear();
            DirectPropertiesCollection.Clear();
            InheritedPropertiesCollection.Clear();
            DirectRelationshipsCollection.Clear();
            InheritedRelationshipsCollection.Clear();
        }

        public static string GetNestedProperties(DTRelationshipInfo relationship)
        {
            return string.Join(",\n", relationship.Properties.Select(prop => $"{prop.Name}( {GetSchemaString(prop.Schema)} )"));
        }

        public static string GetSchemaString(DTSchemaInfo schema)
        {
            switch (schema)
            {
                case DTBooleanInfo:
                    return "boolean";
                case DTDateInfo:
                    return "date";
                case DTDateTimeInfo:
                    return "dateTime";
                case DTDoubleInfo:
                    return "double";
                case DTDurationInfo:
                    return "duration";
                case DTFloatInfo:
                    return "float";
                case DTIntegerInfo:
                    return "integer";
                case DTLongInfo:
                    return "long";
                case DTStringInfo:
                    return "string";
                case DTTimeInfo:
                    return "time";
                case DTMapInfo map:
                    string mapKeySchema = GetSchemaString(map.MapKey.Schema);
                    string mapValueSchema = GetSchemaString(map.MapValue.Schema);
                    return $"map( {mapKeySchema} -> {mapValueSchema} )";
                case DTArrayInfo array:
                    string arrayElementSchema = GetSchemaString(array.ElementSchema);
                    return $"array( {arrayElementSchema} )";
                case DTEnumInfo enumSchema:
                    string enumOptions = string.Join($", ", enumSchema.EnumValues.Select(enumValue => enumValue.Name));
                    return $"enum( {enumOptions} )";
                case DTObjectInfo objectSchema:
                    string objectFields = string.Join($",\n",objectSchema.Fields.Select(field => $"{field.Name}( {GetSchemaString(field.Schema)} )"));
                    return $"object(\n{objectFields}\n)";
                default:
                    return schema.ToString() ?? schema.Id.ToString();
            }
        }

        private Graph ConstructVisualizationGraph(DTInterfaceInfo selectedInterface)
        {
            var drawingGraph = new Graph();
            
            // Create the logical structure of the graph. No geometry hints possible yet, only attributes (color etc)
            var selectedInterfaceId = selectedInterface.Id.ToString();
            Node selectedInterfaceNode = drawingGraph.AddNode(selectedInterfaceId);
            selectedInterfaceNode.LabelText = MainWindow.Label(selectedInterface);
            selectedInterfaceNode.Attr.FillColor = Color.Moccasin;
            selectedInterfaceNode.Attr.Color = Color.Black;
            foreach (DTRelationshipInfo relationship in selectedInterface.DirectRelationships().Where(rel => rel.Target != null))
            {
                DTInterfaceInfo targetInterface = (DTInterfaceInfo)MainWindow.Ontology[relationship.Target];
                var targetInterfaceId = targetInterface.Id.ToString();
                Node targetNode = drawingGraph.AddNode(targetInterfaceId);
                targetNode.LabelText = MainWindow.Label(targetInterface);
                if (targetInterfaceId != selectedInterfaceId)
                {
                    targetNode.Attr.Color = Color.Black;
                    targetNode.Attr.FillColor = Color.Linen;
                }
                var edge = drawingGraph.AddEdge(selectedInterfaceId, targetInterfaceId);
                edge.LabelText = relationship.Name;
            }
            foreach (DTInterfaceInfo parent in selectedInterface.Extends)
            {
                var parentInterfaceId = parent.Id.ToString();
                Node parentNode = drawingGraph.AddNode(parentInterfaceId);
                parentNode.LabelText = MainWindow.Label(parent);
                parentNode.Attr.FillColor = Color.BurlyWood;
                var edge = drawingGraph.AddEdge(selectedInterfaceId, parentInterfaceId);
                edge.LabelText = "extends";
                edge.Attr.ArrowheadAtTarget = ArrowStyle.Generalization;
            }
            // This is probably horribly inefficient
            foreach (DTInterfaceInfo child in MainWindow.Ontology.Values
                .Where(entity => entity is DTInterfaceInfo)
                .Select(entity => (DTInterfaceInfo)entity)
                .Where(child => child.Extends.Select(childParent => childParent.Id).Contains(selectedInterface.Id)))
            {
                var childInterfaceId = child.Id.ToString();
                Node childNode = drawingGraph.AddNode(childInterfaceId);
                childNode.LabelText = MainWindow.Label(child);
                childNode.Attr.FillColor = Color.LightYellow;
                var edge = drawingGraph.AddEdge(childInterfaceId, selectedInterfaceId);
                edge.LabelText = "extends";
            }
            foreach (DTComponentInfo component in selectedInterface.DirectComponents())
            {
                var componentInterface = component.Schema;
                var componentInterfaceId = componentInterface.Id.ToString();
                var componentInterfaceLabel = MainWindow.Label(componentInterface);
                Node componentNode = drawingGraph.AddNode(componentInterfaceId);
                componentNode.LabelText = "Component:\n" + componentInterfaceLabel;
                componentNode.Attr.FillColor = Color.PaleVioletRed;
                var edge = drawingGraph.AddEdge(selectedInterfaceId, componentInterfaceId);
                edge.LabelText = component.Name;
            }

            // Create geometries corresponding to logical structure.
            drawingGraph.CreateGeometryGraph();

            // Configure how we want the graph to look.
            foreach (var node in drawingGraph.Nodes)
            {
                node.GeometryNode.BoundaryCurve = CurveFactory.CreateRectangleWithRoundedCorners(200, 80, 3, 2, new Point(0, 0));
                node.Label.Width = node.Width * 0.6;
                node.Label.Height = node.Height * 0.6;
            }
            foreach (var de in drawingGraph.Edges)
            {
                de.Label.GeometryLabel.Width = 180;
                de.Label.GeometryLabel.Height = 80;
                de.Label.FontColor = Color.Black;
            }
            LayoutHelpers.CalculateLayout(drawingGraph.GeometryGraph, new SugiyamaLayoutSettings(), null);

            return drawingGraph;
        }

        private string GraphToSVG(Graph vizGraph)
        {
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            var svgWriter = new SvgGraphWriter(writer.BaseStream, vizGraph);
            svgWriter.Write();
            // get the string from MemoryStream
            ms.Position = 0;
            var sr = new StreamReader(ms);
            var rawSvg = sr.ReadToEnd();
            var svgWithoutXmlHeader = rawSvg.Substring(rawSvg.IndexOf("\n")).Trim();
            return svgWithoutXmlHeader;
        }

        private async void PopulateFields()
        {
            if (SelectedInterface != null)
            {
                // Populate visualization
                Graph vizGraph = ConstructVisualizationGraph(SelectedInterface);
                string svg = GraphToSVG(vizGraph);
                await visualizationWebView.EnsureCoreWebView2Async();
                visualizationWebView.NavigateToString(svg);

                // Populate form view
                FormHeader.Text = $"{MainWindow.Label(SelectedInterface)}";
                DtmiTextBlock.Text = SelectedInterface.Id.AbsoluteUri;

                foreach(DTInterfaceInfo iface in SelectedInterface.Extends)
                {
                    ExtendsCollection.Add(iface);
                }

                foreach(var displayName in SelectedInterface.DisplayName)
                {
                    DisplayNameCollection.Add(displayName);
                }

                foreach (var description in SelectedInterface.Description)
                {
                    DescriptionCollection.Add(description);
                }

                foreach (DTComponentInfo component in SelectedInterface.DirectComponents())
                {
                    DirectComponentsCollection.Add(component);
                }

                foreach (DTComponentInfo component in SelectedInterface.InheritedComponents())
                {
                    InheritedComponentsCollection.Add(component);
                }

                foreach (DTPropertyInfo property in SelectedInterface.DirectProperties())
                {
                    DirectPropertiesCollection.Add(property);
                }

                foreach (DTPropertyInfo property in SelectedInterface.InheritedProperties())
                {
                    InheritedPropertiesCollection.Add(property);
                }

                foreach (DTRelationshipInfo relationship in SelectedInterface.DirectRelationships())
                {
                    DirectRelationshipsCollection.Add(relationship);
                }

                foreach (DTRelationshipInfo relationship in SelectedInterface.InheritedRelationships())
                {
                    InheritedRelationshipsCollection.Add(relationship);
                }

                // Populate source view
                SourceTextBox.Text = SelectedInterface.GetJsonLdText();
            }
        }

        private void DTMI_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is TextBlock)
            {
                TextBlock tb = (TextBlock)e.OriginalSource;
                if (tb.DataContext is DTInterfaceInfo) { 
                    DTInterfaceInfo iface = (DTInterfaceInfo)((TextBlock)e.OriginalSource).DataContext;
                    SelectedInterface = iface;
                    MainWindow.ChangeInheritanceTreeSelection(SelectedInterface);
                }
                else if (tb.DataContext is DTComponentInfo)
                {
                    DTComponentInfo component = (DTComponentInfo)tb.DataContext;
                    SelectedInterface = component.Schema;
                    MainWindow.ChangeInheritanceTreeSelection(SelectedInterface);
                }
                else if (tb.DataContext is DTRelationshipInfo)
                {
                    DTRelationshipInfo relationship = (DTRelationshipInfo)tb.DataContext;
                    if (relationship.Target != null)
                    {
                        if (MainWindow.Ontology.ContainsKey(relationship.Target) && MainWindow.Ontology[relationship.Target] is DTInterfaceInfo)
                        {
                            SelectedInterface = (DTInterfaceInfo)MainWindow.Ontology[relationship.Target];
                            MainWindow.ChangeInheritanceTreeSelection(SelectedInterface);
                        }
                    }
                }
            }
        }
    }

    public record LocalizedString(string Language, string Value);
}
