using ConsistentHashRing;
using ConsistentHashRingApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ConsistentHashRingApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var node = (HashRingNode<string>)e.NewValue;
            var hrvm = (HashRingViewModel) DataContext;
            hrvm.CurrentNode = node;
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            var hrvm = (HashRingViewModel)DataContext;
            hrvm.DeleteSelectionCommand.Execute(null);
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = e.Source as ListBox;
            var node = listBox.SelectedItem as HashRingNode<string>;
            var hrvm = (HashRingViewModel)DataContext;
            hrvm.CurrentNode = node;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWin = new AboutWindow();
            aboutWin.ShowDialog();
        }
    }
}
