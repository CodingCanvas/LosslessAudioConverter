using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System;
using System.Diagnostics;
using System.Windows;
using System.Security.AccessControl;
using System.Reflection;
using System.Collections.Concurrent;
using LosslessAudioConverter.Model;

namespace LosslessAudioConverter.ViewModel
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();
            var vm = new DirectorySelectionViewModel();

            this.DataContext = vm;
            this.Closing += vm.OnWindowClosing;

            
        }
    }
}
