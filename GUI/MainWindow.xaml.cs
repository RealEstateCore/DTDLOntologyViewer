using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DotNetRdfExtensions.Models;
using VDS.RDF;
using DotNetRdfExtensions;
using Windows.Storage.Pickers;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BOE.GUI
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
            RootInterfaces.Clear();
            IEnumerable<DTDLInterface> noParentInterfaces = Graph.GetDtdlInterfaces().Where(dtdlInterface => !dtdlInterface.Extends.Any());
            foreach (DTDLInterface dtdlInterface in noParentInterfaces)
            {
                RootInterfaces.Add(dtdlInterface);
            }
        }

        public static ObservableCollection<object> IEnumerableToObservableCollection(IEnumerable<object> members)
        {
            return new ObservableCollection<object>(members);
        }

        private void ParsePath(string path)
        {
            // TODO: Implement loading from single file or directory of files

            // Fake data
            IUriNode agentNode = Graph.CreateUriNode(new Uri("dtmi:digitaltwins:rec_3_3:agents:Agent;1"));
            IUriNode personNode = Graph.CreateUriNode(new Uri("dtmi:digitaltwins:rec_3_3:agents:Person;1"));
            IUriNode organizationNode = Graph.CreateUriNode(new Uri("dtmi:digitaltwins:rec_3_3:agents:Organization;1"));
            IUriNode RDF_type = Graph.CreateUriNode(RDF.type);
            IUriNode DTDL_Interface = Graph.CreateUriNode(DTDL.Interface);
            IUriNode dtdlDescription = Graph.CreateUriNode(DTDL.description);
            IUriNode dtdlDisplayName = Graph.CreateUriNode(DTDL.displayName);
            IUriNode dtdlExtends = Graph.CreateUriNode(DTDL.extends);
            Graph.Assert(agentNode, RDF_type, DTDL_Interface);
            Graph.Assert(personNode, RDF_type, DTDL_Interface);
            Graph.Assert(organizationNode, RDF_type, DTDL_Interface);
            Graph.Assert(organizationNode, dtdlDisplayName, Graph.CreateLiteralNode("Organization", "en"));
            Graph.Assert(organizationNode, dtdlDisplayName, Graph.CreateLiteralNode("Organisation", "se"));
            Graph.Assert(personNode, dtdlExtends, agentNode);
            Graph.Assert(organizationNode, dtdlExtends, agentNode);
            Graph.Assert(organizationNode, dtdlDescription, Graph.CreateLiteralNode("An organization of any sort(e.g., a business, association, project, consortium, tribe, etc.)", "en"));
            Graph.Assert(organizationNode, dtdlDescription, Graph.CreateLiteralNode("En organisation av något slag (exempelvis företag, förening, projekt, konsortium, stam, etc.", "se"));
        }

        private async void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker filePicker = new FileOpenPicker();

            // Get the current window's HWND by passing in the Window object
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            // Associate the HWND with the file picker
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            // Use file picker like normal!
            filePicker.FileTypeFilter.Add(".json");
            StorageFile pickedFile = await filePicker.PickSingleFileAsync();

            if (pickedFile != null)
            {
                // Application now has read/write access to the picked file
                LoadedPath = pickedFile.Path;
            }
        }

        private async void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();

            // Get the current window's HWND by passing in the Window object
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            // Associate the HWND with the file picker
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            // Use file picker like normal!
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder pickedFolder = await folderPicker.PickSingleFolderAsync();

            if (pickedFolder != null)
            {
                // Application now has read/write access to the picked file
                LoadedPath = pickedFolder.Path;
            }
        }

        private void InheritanceHierarchyView_InterfaceSelected(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            InterfacePage.SelectedInterface = (DTDLInterface)args.InvokedItem;
        }
    }
}
