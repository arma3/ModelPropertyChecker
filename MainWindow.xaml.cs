using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
using BIS.Core.Streams;

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
            if (targetDirectory.Contains("!")) return;
            if (targetDirectory.Contains("@")) return;

            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }
        static public int totalCount = 0;
        static public List<Task> tasks = new List<Task>();


        public static Model loadModel(string path)
        {
            using (BinaryReaderEx b = new BinaryReaderEx(
                    //File.Open("P:\\Jbad_most_stred30.p3d", FileMode.Open)
                    //File.Open("P:\\mim_104.p3d", FileMode.Open)
                    //File.Open("T:\\a3\\air_f\\heli_light_01\\heli_light_01_civil_f.p3d", FileMode.Open, FileAccess.Read)
                    File.Open(path, FileMode.Open, FileAccess.Read)
                )
            )
            {
                b.UseLZOCompression = true;
                try
                {
                    Model x = new Model();
                    x.load(b);
                    //models.Add(x);
                    Interlocked.Increment(ref totalCount);
                    Console.WriteLine("{0} {1}", totalCount, path);
                    return x;
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    return null;
                }
            }
        }


        // Insert logic for processing found files here.
        public static void ProcessFile(string path)
        {
            if (!path.EndsWith("p3d")) return;

            tasks.Add(
                Task.Run(() => loadModel(path))
                );


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
            //RecursiveFileProcessor.ProcessDirectory("F:\\Steam\\SteamApps\\common\\Arma 3\\Orange\\Addons"); 
            //RecursiveFileProcessor.ProcessDirectory("F:\\Steam\\SteamApps\\common\\Arma 3\\Jets\\Addons"); 
            //RecursiveFileProcessor.ProcessDirectory("P:\\");
            Task.WaitAll(RecursiveFileProcessor.tasks.ToArray());
            //fail Name = "F:\\Steam\\SteamApps\\common\\Arma 3\\Curator\\Addons\\modules_f_curator\\a3\\modules_f_curator\\CAS\\surfaceGun.p3d"
        }
    }
}
