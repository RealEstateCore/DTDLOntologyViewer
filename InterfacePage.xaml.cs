﻿using DotNetRdfExtensions.Models;
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
using VDS.RDF;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BOE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class InterfacePage : Page
    {
        ObservableCollection<LocalizedString> DisplayNameCollection = new();
        ObservableCollection<LocalizedString> DescriptionCollection = new();
        ObservableCollection<DTDLInterface> ExtendsCollection = new();
        private MainWindow MainWindow
        {
            get => (Application.Current as App)!.Window;
        }
        private DTDLInterface? _selectedInterface;
        public DTDLInterface? SelectedInterface
        {
            get => _selectedInterface;
            set
            {
                _selectedInterface = value;
                PopulateFields();
            }
        }

        public InterfacePage()
        {
            this.InitializeComponent();
        }

        private void PopulateFields()
        {
            if (SelectedInterface != null)
            {
                DtmiTextBlock.Text = SelectedInterface.Dtmi;

                ExtendsCollection.Clear();
                foreach(DTDLInterface iface in SelectedInterface.Extends)
                {
                    ExtendsCollection.Add(iface);
                }

                DisplayNameCollection.Clear();
                LocalizedString displayNameEnglish = new("En", "Agent");
                LocalizedString displayNameItalian = new("It", "Agento");
                DisplayNameCollection.Add(displayNameEnglish);
                DisplayNameCollection.Add(displayNameItalian);

                DescriptionCollection.Clear();
                LocalizedString descriptionEnglish = new("En", "An organization of any sort(e.g., a business, association, project, consortium, tribe, etc.)");
                LocalizedString descriptionSwedish = new("Se", "En organisation av något slag (exempelvis företag, förening, projekt, konsortium, stam, etc.)");
                DescriptionCollection.Add(descriptionEnglish);
                DescriptionCollection.Add(descriptionSwedish);
            }
        }
    }
}
