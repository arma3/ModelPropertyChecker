using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ModelPropertyChecker
{

    public class ModelDirectory: INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname));
        }

        public ObservableCollection<Model> models { get; set; } = new ObservableCollection<Model>();
        public bool IsLoading { get; set; }
        public bool IsNotLoading => !IsLoading;
        public int totalNumberOfModels { get; set; }
        public bool isIndeterminate => totalNumberOfModels == 0;
        public float completedPercentage => (float)models.Count / totalNumberOfModels;


        public async void LoadFromDirectory(string path, TaskScheduler uiThread)
        {

            await Task.Run(() => { }).ContinueWith(t => { models.Clear(); }, uiThread);

            IsLoading = true;
            OnPropertyChanged("IsLoading");
            OnPropertyChanged("IsNotLoading");
            totalNumberOfModels = 0;
            OnPropertyChanged("totalNumberOfModels");
            OnPropertyChanged("isIndeterminate");

            var tasks = ModelLoader.loadFromDirectory(path, true);
            totalNumberOfModels = tasks.Count;
            OnPropertyChanged("totalNumberOfModels");
            OnPropertyChanged("isIndeterminate");

            while (tasks.Count > 0)
            {
                var finishedTask = await Task.WhenAny(tasks);

                tasks.Remove(finishedTask);

    #pragma warning disable 4014 //Don't want await here resharper :U
                finishedTask.ContinueWith(t =>
                {
                    var model = t.Result;
                    PropertyVerifier.verifyModel(ref model);
                    return model;
                }).ContinueWith(t =>
                {
                    models.Add(t.Result);
                    OnPropertyChanged("models");
                    OnPropertyChanged("completedPercentage");
                }, uiThread);
    #pragma warning restore 4014
            }

            IsLoading = false;
            OnPropertyChanged("IsLoading");
            OnPropertyChanged("IsNotLoading");
        }


    }
}
