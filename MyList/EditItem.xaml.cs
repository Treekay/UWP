using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using MyList;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using SQLitePCL;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace MyList
{
    public sealed partial class EditItem : UserControl
    {
        public EditItem()
        {
            current = new ListItem(string.Empty, string.Empty, DateTime.Now, false, "Assets/dog.jpg");
            this.InitializeComponent();
            this.DataContext = this;
            Update_manager();
        }
        private ListItem current;
        public ListItem Current { get => current; set => current = value; }
        private ListItems ViewModel = Common.Items;

        private async void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            string ErrMsg = GetError();
            ContentDialog dialog;
            dialog = new ContentDialog()
            {
                Title = "提示",
                PrimaryButtonText = "确认",
                FullSizeDesired = false,
            };
            if (ErrMsg != string.Empty)
            {
                dialog.Content = ErrMsg;
                dialog.PrimaryButtonClick += (_s, _e) => { };
            }
            else
            {
                if (ViewModel.SelectedItem == null)
                {
                    Common.getMaxId();
                    ViewModel.AddListItem(new ListItem(current.Title_, current.Detail_, current.Date_, false, current.path, Common.uid));
                    dialog.Content = "创建成功";
                }
                else
                {
                    ViewModel.UpdateListItem(new ListItem(current.Title_, current.Detail_, current.Date_, current.isCompleted, current.path));
                    dialog.Content = "修改成功";
                }
                Reset();
                dialog.PrimaryButtonClick += (_s, _e) => {
                    if (Window.Current.Content is Frame root && root.CanGoBack)
                    {
                        root.GoBack();
                    }
                };
            }
            await dialog.ShowAsync();
            Update_manager();
        }

        private async void SelectPictureButton_Click(object sender, RoutedEventArgs e)
        {
            string imgPath = await ImgManager.Picker();
            if (imgPath != string.Empty)
            {
                current.path = imgPath;
            }
        }

        private string GetError()
        {
            string ErrMsg = string.Empty;

            if (current.Title_ == string.Empty)
            {
                ErrMsg = "标题不能为空";
            }
            else if (current.Detail_ == string.Empty)
            {
                ErrMsg = "内容不能为空";
            }
            else if (current.Date_ < DateTime.Now.Date)
            {
                ErrMsg = "日期不能早于当前时间";
            }
            return ErrMsg;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedItem != null)
            {
                current.Title_ = ViewModel.SelectedItem.Title_;
                current.Detail_ = ViewModel.SelectedItem.Detail_;
                current.Date_ = ViewModel.SelectedItem.Date_;
                current.path = ViewModel.SelectedItem.path;
                current.isCompleted = ViewModel.SelectedItem.isCompleted;
            }
            else
            {
                Reset();
            }
        }

        public void Reset()
        {
            CreateButton.Content = "Create";
            current.Title_ = string.Empty;
            current.Detail_ = string.Empty;
            current.Date_ = DateTime.Now;
            current.path = "Assets/dog.jpg";
            slider.Value = 80;
        }

        public ApplicationDataCompositeValue OnSuspending()
        {
            ApplicationDataCompositeValue composite = new ApplicationDataCompositeValue
            {
                ["title"] = Current.Title_,
                ["detail"] = Current.Detail_,
                ["duedate"] = Current.Date_
            };

            return composite;
        }

        public void OnResuming(ApplicationDataCompositeValue composite, string bitmap = "")
        {
            Current.Title_ = (string)composite["title"];
            Current.Detail_ = (string)composite["detail"];
            Current.Date_ = (DateTimeOffset)composite["duedate"];
            if (bitmap != "")
            {
                Current.path = bitmap;
            }
        }

        public void SetInfo(string t, string d, DateTimeOffset dateTimeOffset, bool? complete, string path)
        {
            current.Title_ = t;
            current.Detail_ = d;
            current.Date_ = dateTimeOffset;
            current.isCompleted = complete;
            current.path = path;
            CreateButton.Content = "Update";
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
    }
}