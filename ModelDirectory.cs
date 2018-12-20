using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelPropertyChecker
{

    public class ModelDirectory : INotifyPropertyChanged   
    {

        public event PropertyChangedEventHandler PropertyChanged;  
  
        public void OnPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }  
        public ObservableCollection<Model> models { get; set; } = new ObservableCollection<Model>();

        public async void LoadFromDirectory(string path)
        {
            models.Clear();

            var tasks = ModelLoader.loadFromDirectory(path, true);

            while (tasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(tasks);

                tasks.Remove(finishedTask);

                // Await the completed task.

                var completedModel = await finishedTask;

                PropertyVerifier.verifyModel(ref completedModel);

                models.Add(completedModel);
            }
            OnPropertyChanged("models");  
        }


    }
}
