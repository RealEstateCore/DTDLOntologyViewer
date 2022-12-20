using Microsoft.Azure.DigitalTwins.Parser.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DTDLOntologyViewer.GUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InterfacePage : Page
    {
        ObservableCollection<KeyValuePair<string,string>> DisplayNameCollection = new();
        ObservableCollection<KeyValuePair<string, string>> DescriptionCollection = new();
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
                    MainWindow.ChangeInheritanceTreeSelection(_selectedInterface);
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
            DirectPropertiesCollection.Clear();
            InheritedPropertiesCollection.Clear();
            DirectRelationshipsCollection.Clear();
            InheritedRelationshipsCollection.Clear();
        }

        public static string GetSchemaString(DTSchemaInfo schema, int depth=0)
        {
            string tabs = new string('\t', depth);
            string indentedTabs = tabs + "\t";
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
                    string mapKeySchema = GetSchemaString(map.MapKey.Schema, depth+1);
                    string mapValueSchema = GetSchemaString(map.MapValue.Schema, depth+1);
                    return $"map (\n{indentedTabs}{mapKeySchema} -> {mapValueSchema}\n{tabs})";
                case DTArrayInfo array:
                    string arrayElementSchema = GetSchemaString(array.ElementSchema, depth+1);
                    return $"array (\n{indentedTabs}{arrayElementSchema}\n)";
                case DTEnumInfo enumSchema:
                    string enumOptions = string.Join($", \n{indentedTabs}", enumSchema.EnumValues.Select(enumValue => enumValue.Name));
                    return $"enum (\n{indentedTabs}{enumOptions}{tabs}\n)";
                case DTObjectInfo objectSchema:
                    string objectFields = string.Join($",\n{indentedTabs}",objectSchema.Fields.Select(field => $"{field.Name} ({GetSchemaString(field.Schema, depth + 1)}{indentedTabs})\n"));
                    return $"object (\n{indentedTabs}{objectFields}\n{tabs})";
                default:
                    return schema.ToString() ?? schema.Id.ToString();
            }
        }

        private void PopulateFields()
        {
            if (SelectedInterface != null)
            {
                // Populate source view
                SourceTextBox.Text = SelectedInterface.GetJsonLdText();

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
                DirectRelationshipsHeader.Visibility = DirectRelationshipsCollection.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

                foreach (DTRelationshipInfo relationship in SelectedInterface.InheritedRelationships())
                {
                    InheritedRelationshipsCollection.Add(relationship);
                }
                InheritedRelationshipsHeader.Visibility = InheritedRelationshipsCollection.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

                // TODO: Populate visulization view
            }
        }

        private void ExtendsListView_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (e.OriginalSource is TextBlock && ((TextBlock)e.OriginalSource).DataContext is DTInterfaceInfo)
            {
                DTInterfaceInfo iface = (DTInterfaceInfo)((TextBlock)e.OriginalSource).DataContext;
                SelectedInterface = iface;
            }
        }
    }

    public record LocalizedString(string Language, string Value);
}
