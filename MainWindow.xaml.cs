using System;
using System.ComponentModel;
using System.Windows;
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

        public MainWindow()
        {
            InitializeComponent();
            //directory.LoadFromDirectory(@"C:\dev\projects\ACE3ganthe\addons");
            //directory.LoadFromDirectory(@"I:\ACE3");

            //fail Name = "F:\\Steam\\SteamApps\\common\\Arma 3\\Curator\\Addons\\modules_f_curator\\a3\\modules_f_curator\\CAS\\surfaceGun.p3d"
        }


        private void SelectionChanged(object sender, RoutedPropertyChangedEventArgs<Object> e)
        {
            currentLod = e.NewValue as LOD;
            OnPropertyChanged("currentLod");
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
