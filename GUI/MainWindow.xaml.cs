using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DTDLOntologyViewer.DotNetRdfExtensions.Models;
using VDS.RDF;
using DTDLOntologyViewer.DotNetRdfExtensions;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.IO;
using VDS.RDF.Parsing;
using VDS.RDF.JsonLd;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Reflection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DTDLOntologyViewer.GUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public ITripleStore Store { get; }
        
        public ObservableCollection<DTDLInterface> RootInterfaces { get; }

        private string? _loadedPath;
        public string? LoadedPath
        {
            get => _loadedPath;
            set
            {
                if (value != null)
                {
                    LoadPath(value);
                    _loadedPath = value;
                }
            }
        }

        public MainWindow()
        {
            this.InitializeComponent();
            this.Title = "DTDL Ontology Viewer";
            Store = new TripleStore();
            RootInterfaces = new ObservableCollection<DTDLInterface>();
        }

        // Used to translate IEnumerable<DTDLInterface> from DTDLInterface.extendedBy property to a collection 
        // that TreeView can ingest.
        private static ObservableCollection<DTDLInterface> IEnumerableToObservableCollection(IEnumerable<DTDLInterface> members)
        {
            return new ObservableCollection<DTDLInterface>(members);
        }

        // The built in document loader in DotNetRDF cannot handle DTDL contexts, so this loader replaces it for DTDL 
        // context, but defers to the built in document loader for all other URIs
        private static RemoteDocument LoadDtdl(Uri remoteRef, JsonLdLoaderOptions loaderOptions)
        {
            if (remoteRef.AbsoluteUri.Equals(DTDL.dtdlContext))
            {
                // TODO: Clean this up for packaging; can DTDL context be included as built-in resource?
                string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
                JObject context;
                using (StreamReader file = File.OpenText($"{directoryName}\\DTDL.v2.context.json"))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    context = (JObject)JToken.ReadFrom(reader);
                }
                return new RemoteDocument() { DocumentUrl = remoteRef, ContextUrl = remoteRef, Document = context };
            }
            else
            {
                return DefaultDocumentLoader.LoadJson(remoteRef, loaderOptions);
            }
        }

        // Load a file or a directory of files from disk into the store; then update inheritance tree view
        private void LoadPath(string path)
        {
            // Get selected file or, if directory selected, all JSON files in selected dir
            IEnumerable<FileInfo> sourceFiles;
            if (File.GetAttributes(path) == System.IO.FileAttributes.Directory)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                sourceFiles = directoryInfo.EnumerateFiles("*.json", SearchOption.AllDirectories);
            }
            else
            {
                FileInfo singleSourceFile = new FileInfo(path);
                sourceFiles = new [] { singleSourceFile };
            }

            // Parse and load those JSON files into (cleared) store
            // Note use of DTDL-specific document loader, see LoadDtdl comment
            List<Uri> loadedGraphs = Store.Graphs.GraphUris.ToList();
            foreach (Uri loadedGraph in loadedGraphs) { Store.Remove(loadedGraph); }
            JsonLdProcessorOptions options = new JsonLdProcessorOptions();
            options.DocumentLoader = LoadDtdl;
            JsonLdParser parser = new JsonLdParser(options);
            foreach (FileInfo file in sourceFiles)
            {
                parser.Load(Store, file.FullName);
            }

            // Update inheritance tree view
            RootInterfaces.Clear();
            IGraph graph = Store.Graphs.First();
            IEnumerable<DTDLInterface> noParentInterfaces = graph.GetDtdlInterfaces().Where(iface => !iface.Extends.Any(parentIface => graph.ContainsDtdlInterface(parentIface)));
            foreach (DTDLInterface dtdlInterface in noParentInterfaces)
            {
                RootInterfaces.Add(dtdlInterface);
            }

            // Clear currently selected interface
            InterfacePage.SelectedInterface = null;
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
