using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using MyList;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using SQLitePCL;

namespace MyList
{
    public class NotifyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Common : NotifyBase
    {
        private static ListItems items = new ListItems();
        public static ListItems Items { get { return items; } set { items = value;} }

        static public int uid;
        static public void getMaxId()
        {
            var db = App.conn;
            using (var statement = db.Prepare("SELECT MAX(Id) FROM ListItems"))
            {
                while (SQLiteResult.ROW == statement.Step())
                {
                    if (statement[0] != null) uid = Convert.ToInt32(statement[0].ToString());
                    else uid = 0;
                    uid++;
                }
            }
        }
    }
}
