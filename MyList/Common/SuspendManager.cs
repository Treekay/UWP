using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace MyList
{
    public class SuspendManager
    {
        string PageName;
        public SuspendManager(string name)
        {
            PageName = name;
        }

        /*private bool ExistsImgFile(string name) =>
            ApplicationData.Current.LocalSettings.Values.ContainsKey(name) &&
                    StorageApplicationPermissions.FutureAccessList.ContainsItem(
                    (string)ApplicationData.Current.LocalSettings.Values[name]);
        */
        public void Resume(EditItem editItem)
        {
            var ViewModel = Common.Items;

            var composite = ApplicationData.Current.LocalSettings.Values[PageName] as ApplicationDataCompositeValue;
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("Current")/*ExistsImgFile("Current")*/)
            {
                //StorageFile file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(
                    //(string)ApplicationData.Current.LocalSettings.Values["Current"]);
                //string bitmap = await ImgManager.Opener(file);

                editItem.OnResuming(composite, (string)ApplicationData.Current.LocalSettings.Values["Current"]);
            }
            else
            {
                editItem.OnResuming(composite);
            }
            ApplicationData.Current.LocalSettings.Values.Remove(PageName);
            /*
            var completed = ApplicationData.Current.LocalSettings.Values["completed"] as ApplicationDataCompositeValue;
            ViewModel.SetCompleted(completed);
            ApplicationData.Current.LocalSettings.Values.Remove("completed");
            */
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey("selected"))
            {
                ViewModel.SelectedItem = ViewModel.AllItems[(int)ApplicationData.Current.LocalSettings.Values["selected"]];
                ApplicationData.Current.LocalSettings.Values.Remove("selected");
            }
        }

        public void Suspend(EditItem editItem)
        {
            var ViewModel = Common.Items;

            var composite = editItem.OnSuspending();
            ApplicationData.Current.LocalSettings.Values[PageName] = composite;
            ApplicationData.Current.LocalSettings.Values["Current"] = editItem.Current.path;
            /*
            var completed = ViewModel.GetCompleted();
            ApplicationData.Current.LocalSettings.Values["completed"] = completed;
            */
            if (ViewModel.SelectedItem != null)
            {
                ApplicationData.Current.LocalSettings.Values["selected"] = ViewModel.AllItems.IndexOf(ViewModel.SelectedItem);
            }
        }
    }
}
