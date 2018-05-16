using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using MyList;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.AccessCache;
using Windows.Data.Xml.Dom;
using Windows.UI.Xaml.Shapes;
using SQLitePCL;
using System.Text;
using Windows.UI.Popups;
using Windows.Graphics.Imaging;
// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace MyList
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Update_manager();
            ReadBackground();
        }

        async private void ReadBackground()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.TryGetItemAsync("background.jpg") as StorageFile;
            if (file != null) bg.ImageSource = new BitmapImage { UriSource = new Uri(localFolder.Path + "\\" + "background.jpg") };
        }

        private ListItems ViewModel = Common.Items;
        
        private void AddNewPageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Window.Current.Bounds.Width <= 800)
            {
                ViewModel.SelectedItem = null;
                Frame root = Window.Current.Content as Frame;
                root.Navigate(typeof(NewPage));
            }
            InlineListItemViewGrid.Reset();
            ViewModel.SelectedItem = null;
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveListItem();
            InlineListItemViewGrid.Reset();
            ViewModel.SelectedItem = null;
            Update_manager();
        }

        private void ClickItem(object sender, ItemClickEventArgs e)
        {
            ViewModel.SelectedItem = (ListItem)(e.ClickedItem);
            if (Window.Current.Bounds.Width <= 800)
            {
                Frame root = Window.Current.Content as Frame;
                root.Navigate(typeof(NewPage));
            }
            else
            {
                InlineListItemViewGrid.SetInfo(ViewModel.SelectedItem.Title_, ViewModel.SelectedItem.Detail_, ViewModel.SelectedItem.Date_, ViewModel.SelectedItem.isCompleted, ViewModel.SelectedItem.path);
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                ApplicationData.Current.LocalSettings.Values.Remove("MainPage");
            }
            else
            {
                if (ApplicationData.Current.LocalSettings.Values.ContainsKey("MainPage"))
                {
                    SuspendManager manager = new SuspendManager("MainPage");
                    manager.Resume(InlineListItemViewGrid);
                }
            }
            DataTransferManager.GetForCurrentView().DataRequested += OnShareDataRequested;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (((App)App.Current).issuspend)
            {
                SuspendManager manager = new SuspendManager("MainPage");
                manager.Suspend(InlineListItemViewGrid);
            }
            DataTransferManager.GetForCurrentView().DataRequested -= OnShareDataRequested;
        }

        private void MenuDeleteClick(object sender, RoutedEventArgs e)
        {
            ListItem item = (sender as FrameworkElement).Tag as ListItem;
            ViewModel.SelectedItem = item;
            ViewModel.RemoveListItem();
            InlineListItemViewGrid.Reset();
        }

        private void MenuEditClick(object sender, RoutedEventArgs e)
        {
            ListItem item = (sender as FrameworkElement).Tag as ListItem;
            ViewModel.SelectedItem = item;
            if (InlineListItemViewGrid.Visibility.ToString() == "Collapsed")
            {
                Frame root = Window.Current.Content as Frame;
                root.Navigate(typeof(NewPage));
            }
            else
            {
                InlineListItemViewGrid.SetInfo(ViewModel.SelectedItem.Title_, ViewModel.SelectedItem.Detail_, ViewModel.SelectedItem.Date_, ViewModel.SelectedItem.isCompleted,ViewModel.SelectedItem.path);
            }
        }

        private void MenuShareClick(object sender, RoutedEventArgs e)
        {
            var s = sender as FrameworkElement;
            var item = (ListItem)s.DataContext;
            ViewModel.SelectedItem = item;

            DataTransferManager.ShowShareUI();
        }

        async void OnShareDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            DataRequestDeferral deferal = request.GetDeferral();
            request.Data.Properties.Title = ViewModel.SelectedItem.Title_;
            request.Data.Properties.Description = ViewModel.SelectedItem.Detail_;
            
            request.Data.SetText(ViewModel.SelectedItem.Detail_);
            var photoFile = await StorageFile.GetFileFromPathAsync(ViewModel.SelectedItem.path);
            request.Data.SetStorageItems(new List<StorageFile> { photoFile });
            //request.Data.SetBitmap(RandomAccessStreamReference.CreateFromUri(new Uri(ViewModel.SelectedItem.path)));
            deferal.Complete();
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

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            var s = sender as FrameworkElement;
            var item = (ListItem)s.DataContext;
            if (cb.IsChecked == true)
            {
                item.isCompleted = true;
                var db = App.conn;
                using (var TodoItem = db.Prepare("UPDATE ListItems SET Complete = ? WHERE Title = ?"))
                {
                    TodoItem.Bind(1, "true");
                    TodoItem.Bind(2, item.Title_);
                    TodoItem.Step();
                }
            }
            else
            {
                item.isCompleted = false;
                var db = App.conn;
                using (var TodoItem = db.Prepare("UPDATE ListItems SET Complete = ? WHERE Title = ?"))
                {
                    TodoItem.Bind(1, "false");
                    TodoItem.Bind(2, item.Title_);
                    TodoItem.Step();
                }
            }
        }

        private void RefreshList(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = string.Empty;
            refresh();
            var db = App.conn;
            using (var statement = db.Prepare("SELECT Title,Details,DueDate,Complete,Path,Id FROM ListItems"))
            {
                while (SQLiteResult.ROW == statement.Step())
                {
                    ViewModel.AllItems.Insert(0, new ListItem(statement[0].ToString(), statement[1].ToString(), Convert.ToDateTime(statement[2].ToString()), Convert.ToBoolean(statement[3].ToString()), statement[4].ToString(), Convert.ToInt32(statement[5].ToString())));
                }
            }
        }

        private void refresh()
        {
            for (int i = ViewModel.AllItems.Count - 1; i >= 0; i--)
            {
                ViewModel.AllItems.Remove(ViewModel.AllItems[i]);
            }
            ViewModel.SelectedItem = null;
        }

        private void Clear_Finish(object sender, RoutedEventArgs e)
        {
            refresh();
            var db = App.conn;
            using (var TodoItem = db.Prepare("DELETE FROM ListItems WHERE Complete = ?"))
            {
                TodoItem.Bind(1, "true");
                TodoItem.Step();
            }
            using (var statement = db.Prepare("SELECT Title,Details,DueDate,Complete,Path,Id FROM ListItems"))
            {
                while (SQLiteResult.ROW == statement.Step())
                {
                    ViewModel.AllItems.Insert(0, new ListItem(statement[0].ToString(), statement[1].ToString(), Convert.ToDateTime(statement[2].ToString()), Convert.ToBoolean(statement[3].ToString()), statement[4].ToString(), Convert.ToInt32(statement[5].ToString())));
                }
            }
            Update_manager();
        }

        private void Clear_All(object sender, RoutedEventArgs e)
        {
            refresh();
            var db = App.conn;
            using (var TodoItem = db.Prepare("DELETE FROM ListItems"))
            {
                TodoItem.Step();
            }
            Update_manager();
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            refresh();
            String result = String.Empty;
            StringBuilder DataQuery = new StringBuilder("%%");
            DataQuery.Insert(1,SearchBox.Text);
            var db = App.conn;
            using (var statement = db.Prepare("SELECT Title,Details,DueDate,Complete,Path,Id FROM ListItems WHERE Title LIKE ? OR Details LIKE ? OR DueDate LIKE ?")) {
                statement.Bind(1,DataQuery.ToString());
                statement.Bind(2,DataQuery.ToString());
                statement.Bind(3,DataQuery.ToString());
                while (SQLiteResult.ROW == statement.Step()) {
                    result += "Title: " + statement[0].ToString() + " ";
                    result += "   Details: " + statement[1].ToString() + " ";
                    result += "   DueDate: " + statement[2].ToString() + "\n";
                    ViewModel.AllItems.Insert(0, new ListItem(statement[0].ToString(), statement[1].ToString(), Convert.ToDateTime(statement[2].ToString()), Convert.ToBoolean(statement[3].ToString()), statement[4].ToString(), Convert.ToInt32(statement[5].ToString())));
                }
            }
            
            if (result == String.Empty)
            {
                var box1 = new MessageDialog("Not find").ShowAsync();
            }
            else
            {
                var box2 = new MessageDialog("Find Items:\n" + result).ShowAsync();
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
