using System;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using SQLitePCL;
using Windows.UI.Xaml.Controls;

namespace MyList
{
    public class ListItems : NotifyBase
    {
        private ObservableCollection<ListItem> Items = new ObservableCollection<ListItem>();
        public ObservableCollection<ListItem> AllItems { get { return Items;} }

        private ListItem selectedItem = null;
        public ListItem SelectedItem { get { return selectedItem; } set { selectedItem = value; NotifyPropertyChanged(); } }
        
        public ListItems()
        {
            var db = App.conn;
            using (var statement = db.Prepare("SELECT Title,Details,DueDate,Complete,Path,Id FROM ListItems ORDER BY Id ASC"))
            {
                while (SQLiteResult.ROW == statement.Step())
                {
                    this.AllItems.Insert(0, new ListItem(statement[0].ToString(), statement[1].ToString(), Convert.ToDateTime(statement[2].ToString()), Convert.ToBoolean(statement[3].ToString()), statement[4].ToString(), Convert.ToInt32(statement[5].ToString())));
                }
            }
        }

        public void AddListItem(ListItem poi)
        {
            Items.Insert(0, poi);
            var db = App.conn;
            Common.getMaxId();
            using (var TodoItem = db.Prepare("INSERT INTO ListItems (Title,Details,DueDate,Complete,Path,Id) VALUES(?,?,?,?,?,?);"))
            {
                TodoItem.Bind(1, poi.Title_);
                TodoItem.Bind(2, poi.Detail_);
                TodoItem.Bind(3, poi.Date_.Date.ToString("yyyy-MM-dd"));
                TodoItem.Bind(4, "false");
                TodoItem.Bind(5, poi.path);
                TodoItem.Bind(6, Common.uid);
                TodoItem.Step();
            }
        }

        public void RemoveListItem()
        {
            if (SelectedItem != null)
            {
                var db = App.conn;
                using (var TodoItem = db.Prepare("DELETE FROM ListItems WHERE Id = ? "))
                {
                    TodoItem.Bind(1, SelectedItem.id);
                    TodoItem.Step();
                }
                Items.Remove(SelectedItem);
            }
            SelectedItem = null;
        }

        public void UpdateListItem(ListItem poi)
        {
            if (SelectedItem == null) return;
            SelectedItem.Title_ = poi.Title_;
            SelectedItem.Detail_ = poi.Detail_;
            SelectedItem.Date_ = poi.Date_;
            SelectedItem.path = poi.path;
            SelectedItem.DateHandler();
            var db = App.conn;
            using (var TodoItem = db.Prepare("UPDATE ListItems SET Title = ?,Details = ?,DueDate = ?,Path=? WHERE Id = ?"))
            {
                TodoItem.Bind(1, poi.Title_);
                TodoItem.Bind(2, poi.Detail_);
                TodoItem.Bind(3, poi.Date_.Date.ToString("yyyy-MM-dd"));
                TodoItem.Bind(4, poi.path);
                TodoItem.Bind(5, SelectedItem.id);
                TodoItem.Step();
            }
            SelectedItem = null;
        }
        /*
        public ApplicationDataCompositeValue GetCompleted()
        {
            var completed = new ApplicationDataCompositeValue();
            int index = 0;
            for (int i = 0; i < Items.Count; ++i)
            {
                if (Items[i].isCompleted == true)
                {
                    completed[index.ToString()] = i;
                    ++index;
                }
            }
            return completed;
        }
        */
        /*
        public void SetCompleted(ApplicationDataCompositeValue completed)
        {
            foreach (int i in completed.Values)
            {
                Items[i].isCompleted = true;
            }
        }
        */
    }
}