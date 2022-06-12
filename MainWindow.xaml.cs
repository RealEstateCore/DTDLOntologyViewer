﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using DotNetRdfExtensions.Models;
using VDS.RDF;
using DotNetRdfExtensions;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BOE
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public IGraph Graph { get; }
        public ObservableCollection<DTDLInterface> RootInterfaces { get; }

        private string? _loadedPath;
        public string? LoadedPath
        {
            get => _loadedPath;
            set
            {
                if (value != null)
                {
                    ParsePath(value);
                    DrawInheritanceTree();
                    _loadedPath = value;
                }
            }
        }

        public MainWindow()
        {
            this.InitializeComponent();
            Graph = new Graph();
            RootInterfaces = new ObservableCollection<DTDLInterface>();
        }

        private void DrawInheritanceTree()
        {
            // Fake real data
            IUriNode agentNode = Graph.CreateUriNode(new Uri("dtmi:digitaltwins:rec_3_3:agents:Agent;1"));
            IUriNode personNode = Graph.CreateUriNode(new Uri("dtmi:digitaltwins:rec_3_3:agents:Person;1"));
            IUriNode organizationNode = Graph.CreateUriNode(new Uri("dtmi:digitaltwins:rec_3_3:agents:Organization;1"));
            IUriNode RDF_type = Graph.CreateUriNode(RDF.type);
            IUriNode DTDL_Interface = Graph.CreateUriNode(DTDL.Interface);
            IUriNode dtdlExtends = Graph.CreateUriNode(DTDL.extends);
            Graph.Assert(agentNode, RDF_type, DTDL_Interface);
            Graph.Assert(personNode, RDF_type, DTDL_Interface);
            Graph.Assert(organizationNode, RDF_type, DTDL_Interface);
            Graph.Assert(personNode, dtdlExtends, agentNode);
            Graph.Assert(organizationNode, dtdlExtends, agentNode);

            RootInterfaces.Clear();
            IEnumerable<DTDLInterface> noParentInterfaces = Graph.GetDtdlInterfaces().Where(dtdlInterface => !dtdlInterface.Extends.Any());
            foreach (DTDLInterface dtdlInterface in noParentInterfaces)
            {
                RootInterfaces.Add(dtdlInterface);
            }
        }

        private void ParsePath(string path)
        {
            // TODO: Implement loading from single file or directory of files
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement a window to select path
            LoadedPath = "c:\\test\\test\\test";
        }

        private void InheritanceHierarchyView_InterfaceSelected(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            InterfacePage.SelectedInterface = (DTDLInterface)args.InvokedItem;
        }
    }

    // TODO: Remove this scaffolding
    public record LocalizedString(string Language, string Content);
}
