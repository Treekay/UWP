using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using MyList;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Notifications;
using SQLitePCL;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace MyList
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NewPage : Page
    {
        public NewPage()
        {
            this.InitializeComponent();
            Update_manager();
            if (ViewModel.SelectedItem != null)
            {
                EditItemGrid.SetInfo(ViewModel.SelectedItem.Title_, ViewModel.SelectedItem.Detail_, ViewModel.SelectedItem.Date_, ViewModel.SelectedItem.isCompleted, ViewModel.SelectedItem.path);
            }
            ReadBackground();
        }

        async private void ReadBackground()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.TryGetItemAsync("background.jpg") as StorageFile;
            if (file != null) bg.ImageSource = new BitmapImage { UriSource = new Uri(localFolder.Path + "\\" + "background.jpg") };
        }

        private ListItems ViewModel = Common.Items;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                ApplicationData.Current.LocalSettings.Values.Remove("NewPage");
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("NewPage"))
                {
                    SuspendManager manager = new SuspendManager("NewPage");
                    manager.Resume(EditItemGrid);
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (((App)App.Current).issuspend)
            {
                SuspendManager manager = new SuspendManager("NewPage");
                manager.Suspend(EditItemGrid);
            }
            else
            {
                if (ViewModel.SelectedItem != null)
                {
                    ViewModel.SelectedItem = null;
                }
            }
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveListItem();
            EditItemGrid.Reset();
            Frame root = Window.Current.Content as Frame;
            if (root.CanGoBack)
            {
                root.GoBack();
            }
            Update_manager();
        }

        private void Update_message(int count)
        {
            if (count == 0) return;
            XmlDocument document = new XmlDocument();
            document.LoadXml(File.ReadAllText("AdaptiveTile.xml"));
            XmlNodeList textElements = document.GetElementsByTagName("text");
            textElements[0].InnerText = ViewModel.AllItems[count - 1].Title_;
            textElements[2].InnerText = ViewModel.AllItems[count - 1].Title_;
            textElements[4].InnerText = ViewModel.AllItems[count - 1].Title_;
            textElements[6].InnerText = ViewModel.AllItems[count - 1].Title_;

            textElements[7].InnerText = ViewModel.AllItems[count - 1].Detail_;

            textElements[1].InnerText = ViewModel.AllItems[count - 1].DateForm;
            textElements[3].InnerText = ViewModel.AllItems[count - 1].DateForm;
            textElements[5].InnerText = ViewModel.AllItems[count - 1].DateForm;
            textElements[8].InnerText = ViewModel.AllItems[count - 1].DateForm;

            XmlNodeList imgElements = document.GetElementsByTagName("image");
            var imgElement = imgElements[0] as Windows.Data.Xml.Dom.XmlElement;
            for (int i = 0; i < imgElements.Count; i++)
            {
                imgElement = imgElements[i] as Windows.Data.Xml.Dom.XmlElement;
                imgElement.SetAttribute("src", ViewModel.AllItems[count - 1].path);
            }

            var tileNotification = new TileNotification(document);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
        }

        private void Update_manager()
        {
            TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            for (int i = 1; i <= ViewModel.AllItems.Count; i++)
            {
                Update_message(i);
            }
        }

        private async void ChangeBackground(object sender, RoutedEventArgs e)
        {
            Windows.Storage.Pickers.FileOpenPicker picker
               = new Windows.Storage.Pickers.FileOpenPicker
               {
                   ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                   SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
               };

            picker.FileTypeFilter.Clear();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpeg");

            StorageFile imgFile = await picker.PickSingleFileAsync();

            if (imgFile != null)
            {
                using (IRandomAccessStream fileStream
                    = await imgFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);
                    bg.ImageSource = bitmapImage;

                    StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                    SoftwareBitmap softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                    
                    StorageFile localFile = await localFolder.CreateFileAsync("background.jpg", CreationCollisionOption.ReplaceExisting);
                    await ImgManager.SaveSoftwareBitmapToFile(softwareBitmap, localFile);
                }
            }
        }
    }
}
