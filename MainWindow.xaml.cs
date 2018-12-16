using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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

namespace ModelPropertyChecker
{

    //https://docs.microsoft.com/en-us/dotnet/api/system.io.directory.getfiles?redirectedfrom=MSDN&view=netframework-4.7.2#System_IO_Directory_GetFiles_System_String_
    public class RecursiveFileProcessor
    {
        public static List<Model> models = new List<Model>();

        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        public static void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        // Insert logic for processing found files here.
        public static void ProcessFile(string path)
        {
            if (path.EndsWith("p3d"))
            {
                using (BinaryReader b = new BinaryReader(
                        //File.Open("P:\\Jbad_most_stred30.p3d", FileMode.Open)
                        //File.Open("P:\\mim_104.p3d", FileMode.Open)
                        File.Open(path, FileMode.Open,FileAccess.Read)
                            )
                )
                {
                    Model x = new Model();
                    x.load(b);
                    models.Add(x);
                }
            }
        }
    }









    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            RecursiveFileProcessor.ProcessDirectory("T:\\");



        }
    }
}
