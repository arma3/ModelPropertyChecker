using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
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
                    var MainThread = TaskScheduler.FromCurrentSynchronizationContext();

                    Task.Run(() => directory.LoadFromDirectory(fbd.FileName, MainThread));
                }
            }
        }

        private void Button_ExportIssues_OnClick(object sender, RoutedEventArgs e)
        {
            using (var fbd = new SaveFileDialog())
            {
                fbd.CreatePrompt = true;
                fbd.AddExtension = true;
                fbd.DefaultExt = "ini";
                fbd.Filter = "Ini File (*.ini)|*.*";
                fbd.Title = "Output filename";
                var result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.FileName))
                {

                    using (System.IO.StreamWriter file =
                        new System.IO.StreamWriter(fbd.FileName))
                    {
                        foreach (var model in directory.models)
                        {
                            if (model.exceptionCount == 0) continue;
                            file.WriteLine(model.totalPath);

                            foreach (var lod in model.lods)
                            {
                                if (lod.Value.exceptionCount == 0) continue;
                                file.WriteLine("\t" + lod.Value.resolution);

                                foreach (var exception in lod.Value.propertyExceptions)
                                {
                                    file.WriteLine("\t\t" + exception.propertyName + "=" + exception.Message);
                                }
                            }
                        }
                    }

                }
            }

        }
    }
}
