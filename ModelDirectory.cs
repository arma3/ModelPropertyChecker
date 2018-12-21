using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ModelPropertyChecker
{

    public class ModelDirectory
    {
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
                await finishedTask.ContinueWith(t =>
                {
                    var model = t.Result;
                    PropertyVerifier.verifyModel(ref model);
                    return model;
                }).ContinueWith(t =>
                {
                    models.Add(t.Result);
                }, TaskScheduler.FromCurrentSynchronizationContext());
            } 
        }


    }
}
