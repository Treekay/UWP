using System;
using System.ComponentModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using MyList;
using SQLitePCL;

namespace MyList
{
    public class ListItem : INotifyPropertyChanged
    {
        private int ID;
        public int id
        {
            set
            {
                ID = value;
                NotifyPropertyChanged("id");
            }
            get
            {
                return ID;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        private string title_;
        public string Title_
        {
            set
            {
                title_ = value;
                NotifyPropertyChanged("Title_");
            }
            get
            {
                return title_;
            }
        }
        private string detail_;
        public string Detail_
        {
            set
            {
                detail_ = value;
                NotifyPropertyChanged("Detail_");
            }
            get
            {
                return detail_;
            }
        }
        private DateTimeOffset date_;
        public DateTimeOffset Date_
        {
            set
            {
                date_ = value;
                NotifyPropertyChanged("date_");
            }
            get
            {
                return date_;
            }
        }
        private string dateform;
        public string DateForm
        {
            set
            {
                dateform = value;
                NotifyPropertyChanged("DateForm");
            }
            get
            {
                return dateform;
            }
        }
        private bool? iscompleted;
        public bool? isCompleted {
            set
            {
                iscompleted = value;
                NotifyPropertyChanged("isCompleted");
            }
            get
            {
                return iscompleted;
            }
        }
        private string Path;
        public string path
        {
            set
            {
                Path = value;
                NotifyPropertyChanged("path");
            }
            get
            {
                return Path;
            }
        }
        public ListItem(String T, String D, DateTimeOffset date, bool? complete = false, string path = "", int id = 0)
        {
            if (id != 0) this.id = id;
            this.Title_ = T;
            this.Detail_ = D;
            this.Date_ = date;
            this.path = path == "" ? "ms-appx:///Assets/dog.jpg" : path;
            this.isCompleted = complete;
            DateHandler();
        }

        public void DateHandler()
        {
            DateForm = string.Empty;
            this.DateForm += Date_.Year;
            this.DateForm += " / ";
            this.DateForm += Date_.Month;
            this.DateForm += " / ";
            this.DateForm += Date_.Day;
        }
    }
}
