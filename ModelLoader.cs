using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using BIS.Core.Streams;

namespace ModelPropertyChecker
{
    static class ModelLoader
    {

        //https://docs.microsoft.com/en-us/dotnet/api/system.io.directory.getfiles?redirectedfrom=MSDN&view=netframework-4.7.2#System_IO_Directory_GetFiles_System_String_
        class RecursiveFileProcessor
        {

            private readonly bool recursive;
            private readonly string basePath;
            static int totalCount;
            static List<Task<Model>> tasks = new List<Task<Model>>();
            public RecursiveFileProcessor(string path, bool recurse = false)
            {
                basePath = path;
                recursive = recurse;
            }


            // Process all files in the directory passed in, recurse on any directories 
            // that are found, and process the files they contain.
            void ProcessDirectory(string targetDirectory)
            {
                if (targetDirectory.Contains("!")) return;
                if (targetDirectory.Contains("@")) return;

                // Process the list of files found in the directory.
                string[] fileEntries = Directory.GetFiles(targetDirectory,"*.p3d", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                foreach (string fileName in fileEntries)
                    ProcessFile(fileName);
            }

            Model loadModel(string path)
            {

                try
                {
                        using (BinaryReaderEx b = new BinaryReaderEx(
                            //File.Open("P:\\Jbad_most_stred30.p3d", FileMode.Open)
                            //File.Open("P:\\mim_104.p3d", FileMode.Open)
                            //File.Open("T:\\a3\\weapons_f\\acc\\acco_aco_f.p3d", FileMode.Open, FileAccess.Read)
                            File.Open(path, FileMode.Open, FileAccess.Read)
                        )
                    )
                    {
                        b.UseLZOCompression = true;
                        try
                        {
                            Model x = new Model {totalPath = path, subPath = path.Substring(basePath.Length + 1)};
                            try {
                                x.load(b);
                            } catch (Exception ex) {


                                Console.WriteLine(path+"\n"+ex.ToString());
                                //MessageBox.Show(path+"\n"+ex.ToString());
                            }





                           
                            //models.Add(x);
                            Interlocked.Increment(ref totalCount);
                            Console.WriteLine($@"{totalCount} {path}");
                            return x;
                        } catch (ArgumentOutOfRangeException ex)
                        {
                            return null;
                        }
                    }
                }
                catch (System.IO.IOException ex)
                {
                    //MessageBox.Show(path + "\n" + ex.ToString());
                    return null;
                }

            }


            // Insert logic for processing found files here.
            void ProcessFile(string path)
            {
                tasks.Add(
                    Task.Run(() => loadModel(path))
                );
            }

            public List<Task<Model>> Run()
            {
                ProcessDirectory(basePath);
                return tasks;
            }
        }

        public static List<Task<Model>> loadFromDirectory(string path, bool recursive = false)
        {
            var processor = new RecursiveFileProcessor(path, recursive);
            return processor.Run();
        }

    }
}
