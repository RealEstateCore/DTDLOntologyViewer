using Microsoft.Azure.DigitalTwins.Parser.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DTDLOntologyViewer.GUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InterfacePage : Page
    {
        ObservableCollection<LocalizedString> DisplayNameCollection = new();
        ObservableCollection<LocalizedString> DescriptionCollection = new();
        ObservableCollection<DTPropertyInfo> PropertiesCollection = new();
        ObservableCollection<DTRelationshipInfo> RelationshipsCollection = new();
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
            FormHeader.Text = "Interface: ";
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
                FormHeader.Text = $"Interface: {SelectedInterface}";
                DtmiTextBlock.Text = SelectedInterface.Id.AbsoluteUri;

                foreach(DTInterfaceInfo iface in SelectedInterface.Extends)
                {
                    ExtendsCollection.Add(iface);
                }

                foreach(var displayName in SelectedInterface.DisplayName)
                {
                    DisplayNameCollection.Add(new LocalizedString(displayName.Key, displayName.Value));
                }

                foreach (var description in SelectedInterface.Description)
                {
                    DescriptionCollection.Add(new LocalizedString(description.Key, description.Value));
                }

                foreach (DTPropertyInfo property in SelectedInterface.Properties())
                {
                    PropertiesCollection.Add(property);
                }

                foreach (DTRelationshipInfo relationship in SelectedInterface.Relationships())
                {
                    RelationshipsCollection.Add(relationship);
                }
            }
        }
    }

    public record LocalizedString(string Language, string Value);
}
