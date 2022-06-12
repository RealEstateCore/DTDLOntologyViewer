using Microsoft.UI.Xaml;
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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BOE
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        readonly IGraph graph = new Graph();
        DTDLInterface? selectedInterface;
        ObservableCollection<DTDLInterface> ExtendsCollection = new();
        ObservableCollection<LocalizedString> DisplayNameCollection = new();
        ObservableCollection<LocalizedString> DescriptionCollection = new();

        public MainWindow()
        {
            this.InitializeComponent();

            // TODO: Implement real loading somewhere
            IUriNode agentNode = graph.CreateUriNode(new Uri("dtmi:digitaltwins:rec_3_3:agents:Agent;1"));
            selectedInterface = new DTDLInterface(agentNode, graph);

            if (selectedInterface != null)
            {
                DtmiTextBlock.Text = selectedInterface.Dtmi;
            }

            TreeViewNode rootNode1 = new TreeViewNode() { Content = "Root 1" };
            rootNode1.Children.Add(new TreeViewNode() { Content = "Child 1" });
            rootNode1.Children.Add(new TreeViewNode() { Content = "Child 2" });
            rootNode1.Children.Add(new TreeViewNode() { Content = "Child 3" });
            rootNode1.Children.Add(new TreeViewNode() { Content = "Child 4" });
            TreeViewNode rootNode2 = new TreeViewNode() { Content = "Root 2" };
            rootNode2.Children.Add(new TreeViewNode() { Content = "Child 5" });
            rootNode2.Children.Add(new TreeViewNode() { Content = "Child 6" });
            rootNode2.Children.Add(new TreeViewNode() { Content = "Child 7" });
            rootNode2.Children.Add(new TreeViewNode() { Content = "Child 8" });
            InheritanceHierarchyView.RootNodes.Add(rootNode1);
            InheritanceHierarchyView.RootNodes.Add(rootNode2);

            IUriNode personNode = graph.CreateUriNode(new Uri("dtmi:digitaltwins:rec_3_3:agents:Person;1"));
            IUriNode organizationNode = graph.CreateUriNode(new Uri("dtmi:digitaltwins:rec_3_3:agents:Organization;1"));
            DTDLInterface personInterface = new DTDLInterface(personNode, graph);
            DTDLInterface organizationInterface = new DTDLInterface(organizationNode, graph);
            ExtendsCollection.Add(personInterface);
            ExtendsCollection.Add(organizationInterface);
            ExtendsListView.ItemsSource = ExtendsCollection;

            LocalizedString displayNameEnglish = new("En", "Agent");
            LocalizedString displayNameItalian = new("It", "Agento");
            DisplayNameCollection.Add(displayNameEnglish);
            DisplayNameCollection.Add(displayNameItalian);
            DisplayNameListView.ItemsSource = DisplayNameCollection;

            LocalizedString descriptionEnglish = new("En", "An organization of any sort(e.g., a business, association, project, consortium, tribe, etc.)");
            LocalizedString descriptionSwedish = new("Se", "En organisation av något slag (exempelvis företag, förening, projekt, konsortium, stam, etc.)");
            DescriptionCollection.Add(descriptionEnglish);
            DescriptionCollection.Add(descriptionSwedish);
            DescriptionListView.ItemsSource = DescriptionCollection;
        }
    }

    public record LocalizedString(string Language, string Content);
}
