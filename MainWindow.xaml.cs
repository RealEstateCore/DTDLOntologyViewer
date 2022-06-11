using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BOE
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
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
            //InheritanceHierarchyView.
            //InheritanceHierarchyView
        }

        /*
        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            myButton.Content = "Clicked";
        }*/
    }
}
