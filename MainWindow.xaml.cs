﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BIS.Core.Streams;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ModelPropertyChecker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged   
    {

        public event PropertyChangedEventHandler PropertyChanged;  
  
        public void OnPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }  
 
        public ModelDirectory directory { get; } = new ModelDirectory();
        public LOD currentLod { get; set; }
        public BindingList<PropertyException> currentErrors { get; set; }= new BindingList<PropertyException>();

        public MainWindow()
        {
            InitializeComponent();
            //directory.LoadFromDirectory(@"C:\dev\projects\ACE3ganthe\addons");
            //directory.LoadFromDirectory(@"I:\ACE3");

            //fail Name = "F:\\Steam\\SteamApps\\common\\Arma 3\\Curator\\Addons\\modules_f_curator\\a3\\modules_f_curator\\CAS\\surfaceGun.p3d"
        }


        private void SelectionChanged(object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            currentErrors.Clear();
            currentLod = e.NewValue as LOD;
            OnPropertyChanged("currentLod");
            if (e.NewValue is LOD lod)
            {
                foreach (var exception in lod.propertyExceptions) { currentErrors.Add(exception); }
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            using(var fbd = new CommonOpenFileDialog())
            {
                fbd.IsFolderPicker = true;
                var result = fbd.ShowDialog();

                if (result == CommonFileDialogResult.Ok && !string.IsNullOrWhiteSpace(fbd.FileName))
                {
                    directory.LoadFromDirectory(fbd.FileName);
                }
            }
        }
    }
}
