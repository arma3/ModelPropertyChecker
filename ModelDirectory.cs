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
                await Task.WhenAny(tasks).ContinueWith(t =>
                {
                    tasks.Remove(t.Result);
                    var model = t.Result.Result;
                    PropertyVerifier.verifyModel(ref model);
                    return model;
                }).ContinueWith(t =>
                {
                    models.Add(t.Result);
                }, uiThread);
            } 
        }


    }
}
