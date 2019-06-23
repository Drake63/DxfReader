using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace DxfOpener
{
    [Serializable]
    public class Profiles : Collection<Profile>
    {
        int currentProfile;

        public event PropertyChangedEventHandler PropertyChanged;
        public event CollectionChangeEventHandler CollectionChanged;

        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
        public int CurrentProfile
        {
            get { return currentProfile = Items.IndexOf(Items.FirstOrDefault(i=>i.IsCurrent==true)); }
            set
            {
                if (value < 0) return;
                if (value != currentProfile)
                {
                    if (value >= Items.Count)
                        throw new ArgumentOutOfRangeException();
                    Items[currentProfile].IsCurrent = false;
                    Items[value].IsCurrent = true;
                    currentProfile = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentProfile"));
                }
            }
        }

        protected override void InsertItem(int index, Profile item)
        {
            base.InsertItem(index, item);

            item.PropertyChanged += new PropertyChangedEventHandler(onItemPropertyChanged);
            //CurrentProfile = Items.Count - 1;
            CollectionChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Add, item));
        }

        protected override void RemoveItem(int index)
        {
            CurrentProfile = 0;

            base.RemoveItem(index);

            CollectionChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Remove, null));            
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            CurrentProfile = 0;
            CollectionChanged?.Invoke(this, new CollectionChangeEventArgs(CollectionChangeAction.Remove, null));
        }
        private void onItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
    }
}
