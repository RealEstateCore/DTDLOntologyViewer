using Microsoft.Azure.DigitalTwins.Parser.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private void PopulateFields()
        {
            if (SelectedInterface != null)
            {
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
