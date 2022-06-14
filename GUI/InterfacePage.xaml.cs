using DotNetRdfExtensions;
using DotNetRdfExtensions.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using VDS.RDF;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BOE.GUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InterfacePage : Page
    {
        ObservableCollection<ILiteralNode> DisplayNameCollection = new();
        ObservableCollection<ILiteralNode> DescriptionCollection = new();
        ObservableCollection<DTDLProperty> PropertiesCollection = new();
        ObservableCollection<DTDLRelationship> RelationshipsCollection = new();
        ObservableCollection<DTDLInterface> ExtendsCollection = new();
        private MainWindow MainWindow
        {
            get => (Application.Current as App)!.Window!;
        }
        private DTDLInterface? _selectedInterface;
        public DTDLInterface? SelectedInterface
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
            DtmiTextBlock.Text = string.Empty;
            ExtendsCollection.Clear();
            DisplayNameCollection.Clear();
            DescriptionCollection.Clear();
            PropertiesCollection.Clear();
            RelationshipsCollection.Clear();
        }

        private void PopulateFields()
        {
            if (SelectedInterface != null)
            {
                DtmiTextBlock.Text = SelectedInterface.Dtmi;

                foreach(DTDLInterface iface in SelectedInterface.Extends)
                {
                    ExtendsCollection.Add(iface);
                }

                foreach(ILiteralNode displayNameNode in SelectedInterface.DisplayNames)
                {
                    DisplayNameCollection.Add(displayNameNode);
                }

                foreach (ILiteralNode descriptionNode in SelectedInterface.Descriptions)
                {
                    DescriptionCollection.Add(descriptionNode);
                }

                foreach (DTDLProperty property in SelectedInterface.Contents.Properties())
                {
                    PropertiesCollection.Add(property);
                }

                foreach (DTDLRelationship relationship in SelectedInterface.Contents.Relationships())
                {
                    RelationshipsCollection.Add(relationship);
                }
            }
        }
    }
}
