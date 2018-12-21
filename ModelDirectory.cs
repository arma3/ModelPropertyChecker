using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ModelPropertyChecker
{

    public class ModelDirectory
    {
        public ObservableCollection<Model> models { get; set; } = new ObservableCollection<Model>();

        public async void LoadFromDirectory(string path, TaskScheduler uiThread)
        {

            await Task.Run(() => { }).ContinueWith(t =>
            {
                models.Clear();

            }, uiThread);

            

            var tasks = ModelLoader.loadFromDirectory(path, true);

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
                }, uiThread);
#pragma warning restore 4014
            }
        }


    }
}
