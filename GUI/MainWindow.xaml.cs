﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.IO;
using Microsoft.Azure.DigitalTwins.Parser;
using Microsoft.Azure.DigitalTwins.Parser.Models;

namespace DTDLOntologyViewer.GUI
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        
        private ObservableCollection<DTInterfaceWrapper> RootInterfaces { get; }
        private IReadOnlyDictionary<Dtmi, DTEntityInfo> DTEntities;
        private string? _loadedPath;
        private string? LoadedPath
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
            RootInterfaces = new ObservableCollection<DTInterfaceWrapper>();
            DTEntities = new ReadOnlyDictionary<Dtmi, DTEntityInfo>(new Dictionary<Dtmi, DTEntityInfo>());
        }

        public static string Label(DTInterfaceInfo dtInterface)
        {
            IReadOnlyDictionary<string,string> displayNames = dtInterface.DisplayName;
            if (displayNames.ContainsKey("")) return displayNames[""];
            if (displayNames.ContainsKey("en")) return displayNames["en"];
            if (displayNames.Count > 0) return displayNames.First().Value;
            return dtInterface.Id.Labels.Last();
        }

        // Load a file or a directory of files from disk into the store; then update inheritance tree view
        private async void LoadPath(string path)
        {
            // Get selected file or, if directory selected, all JSON files in selected dir
            IEnumerable<FileInfo> sourceFiles;
            if (File.GetAttributes(path) == System.IO.FileAttributes.Directory)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                var jsonFiles = directoryInfo.EnumerateFiles("*.json", SearchOption.AllDirectories);
                var jsonLdFiles = directoryInfo.EnumerateFiles("*.jsonld", SearchOption.AllDirectories);
                sourceFiles = jsonFiles.Union(jsonLdFiles);
            }
            else
            {
                FileInfo singleSourceFile = new FileInfo(path);
                sourceFiles = new [] { singleSourceFile };
            }


            List<string> modelJson = new List<string>();
            foreach (FileInfo file in sourceFiles)
            {
                using StreamReader modelReader = new StreamReader(file.FullName);
                modelJson.Add(modelReader.ReadToEnd());
            }
            ModelParser modelParser = new ModelParser(0);

            try { 
                DTEntities = await modelParser.ParseAsync(modelJson);

                // Update inheritance tree view
                RootInterfaces.Clear();

                IEnumerable<DTInterfaceInfo> allInterfaces = DTEntities.Values.Where(entity => entity is DTInterfaceInfo).Select(entity => (DTInterfaceInfo)entity);
                IEnumerable<DTInterfaceInfo> noParentInterfaces = allInterfaces.Where(iface => !iface.Extends.Any(parentIface => allInterfaces.Contains(parentIface)));
                foreach (DTInterfaceInfo dtdlInterface in noParentInterfaces)
                {
                    RootInterfaces.Add(new DTInterfaceWrapper(dtdlInterface, DTEntities));
                }

                // Clear currently selected interface
                InterfacePage.SelectedInterface = null;
            }
            catch (ParsingException parserEx)
            {

                ContentDialog errorDialog = new()
                {
                    XamlRoot = Content.XamlRoot,
                    Title = $"DTDL Parser Error",
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close,
                    Content = new ScrollViewer() {
                        Content = new TextBlock() {
                            Text = parserEx.Message + "\n\n" + string.Join("\n\n", parserEx.Errors.Select(error => error.Message))
                        },
                        VerticalScrollMode = ScrollMode.Enabled,
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        HorizontalScrollMode = ScrollMode.Enabled,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch
                    },
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                await errorDialog.ShowAsync();
            }
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
            filePicker.FileTypeFilter.Add(".jsonld");
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

        public void ChangeInheritanceTreeSelection(DTInterfaceInfo iface)
        {
            TreeViewNode? targetNode = InheritanceHierarchyView.RootNodes.SelectMany(rootNode => GetTransitiveChildren(rootNode)).FirstOrDefault(node => ((DTInterfaceWrapper)node.Content).WrappedInterface.Id == iface.Id);
            if (targetNode != null)
            {
                InheritanceHierarchyView.SelectedNode = targetNode;
            }
        }

        private void InheritanceHierarchyView_InterfaceSelected(TreeView sender, TreeViewItemInvokedEventArgs args)
        {
            InterfacePage.SelectedInterface = ((DTInterfaceWrapper)args.InvokedItem).WrappedInterface;
        }

        private IEnumerable<TreeViewNode> GetTransitiveChildren(TreeViewNode inputNode)
        {
            return inputNode.Children.Prepend(inputNode).Concat(inputNode.Children.SelectMany(child => GetTransitiveChildren(child)));
        }
    }

    public class DTInterfaceWrapper
    {
        public DTInterfaceInfo WrappedInterface { get; set; }
        public IReadOnlyDictionary<Dtmi, DTEntityInfo> WrappedOntology { get; set; }
        public IEnumerable<DTInterfaceWrapper> Children
        {
            get
            {
                IEnumerable<DTInterfaceInfo> allInterfaces = WrappedOntology.Values.Where(entity => entity is DTInterfaceInfo).Select(entity => (DTInterfaceInfo)entity);
                return allInterfaces.Where(childInterface => childInterface.Extends.Contains(WrappedInterface)).Select(childInterface => new DTInterfaceWrapper(childInterface, WrappedOntology));
            }
        }
        public DTInterfaceWrapper(DTInterfaceInfo wrappedInterface, IReadOnlyDictionary<Dtmi, DTEntityInfo> wrappedOntology)
        {
            WrappedInterface = wrappedInterface;
            WrappedOntology = wrappedOntology;
        }
    }
}
