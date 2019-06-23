using System;
using System.ComponentModel;
using DxfReader;
using System.Windows.Media;
using System.Windows;

namespace DxfOpener
{
    public class Document : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        DxfDocument document = new DxfDocument();

        int precision;
        Size visualSize;
        bool isSaved;

        public ushort Flags
        {
            get => document.Flags;
            set
            {
                document.Flags = value;
                Refresh();
            }
        }

        public string FileName
        {
            get { return document.FileName; }
            set
            {
                if(value != document.FileName)
                {
                    document.Create(value);
                    NotifyPropertyChanged("FileName");
                    NotifyPropertyChanged("Data");
                    NotifyPropertyChanged("LineCount");
                    NotifyPropertyChanged("EntitiesCount");
                    NotifyPropertyChanged("Size");
                    NotifyPropertyChanged("VisualData");
                    NotifyPropertyChanged("ContainsSplines");
                    IsSaved = document.StringCount == LineCount && !document.GetFlag(DxfFlags.Changed);
                }
            }
        }

        public Size Size
        {
            get { return document.Size; }
            set { }
        }
        public string Data
        {
            get { return document.ToString(); }
            set
            {
                document.Create((value as object));
                NotifyPropertyChanged("Data");
                NotifyPropertyChanged("LineCount");
                NotifyPropertyChanged("EntitiesCount");
                NotifyPropertyChanged("Size");
                NotifyPropertyChanged("VisualData");
                NotifyPropertyChanged("ContainsSplines");
                IsSaved = false;
            }
        }

        public int LineCount
        {
            get
            {
                string[] strs = Data.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                return strs.Length;
            }
        }

        public DrawingVisual VisualData
        {
            get { return document.CreateVisual(visualSize); }
            set { }
        }

       public Size VisualSize
        {
            get { return visualSize; }
            set
            {
                if(value != visualSize)
                {
                    visualSize = value;
                    if (IsCreated)
                        NotifyPropertyChanged("VisualData");
                }
            }
        }

        public int Precision
        {
            get { return precision; }
            set { precision = value; }
        }

        public int EntitiesCount
        {
            get { return document.EntitiesCount; }
            set { }
        }

        public int SplinePrecision { get; set; }

        public bool ContainsSplines
        {
            get => document.ContainsSplines;
            set
            {
                if (!value && document.ContainsSplines)
                    document.SplineToPolyline(SplinePrecision);

                if (!document.ContainsSplines)
                {
                    NotifyPropertyChanged("Data");
                    NotifyPropertyChanged("VisualData");
                    NotifyPropertyChanged("LineCount");
                    NotifyPropertyChanged("ContainsSplines");
                    IsSaved = false;
                }
            }
        }

        public bool IsPoints { get { return document.IsPoints; } }

        public int PointsCount { get { return document.PointsCount; } }

        public void ChangePointsColor(DxfColors color)
        {
            document.ChangePointsColor(color);
            NotifyPropertyChanged("Data");
            NotifyPropertyChanged("VisualData");
            IsSaved = false;
        }

        public void DeletePoints()
        {
            document.DeletePoints();
            NotifyPropertyChanged("Data");
            NotifyPropertyChanged("LineCount");
            NotifyPropertyChanged("EntitiesCount");
            NotifyPropertyChanged("VisualData");
            IsSaved = false;
        }

        public bool IsCreated
        {
            get { return document.IsCreated; }
            set { }
        }

        public bool IsSaved
        { get { return isSaved; }
            set
            {
                if(value != isSaved)
                {
                    isSaved = value;
                    NotifyPropertyChanged("IsSaved");
                }
            }
        }
         
        public bool IsPointsAsMainLine { get { return document.IsPointsAsMainLine; } }

        public void Refresh()
        {
            try
            {
                if (!IsCreated) return;                
                document.Create(document.FileName);
                NotifyPropertyChanged("Data");
                NotifyPropertyChanged("LineCount");
                NotifyPropertyChanged("EntitiesCount");
                NotifyPropertyChanged("Size");
                NotifyPropertyChanged("VisualData");
                IsSaved = document.StringCount == LineCount && !document.GetFlag(DxfFlags.Changed);
            }
            catch (Exception ex)
            {
                Clear();
                MessageBox.Show(ex.Message, Properties.Resources.ErrorHeader, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Clear()
        {
            document.Clear();
            FileName = "";
            NotifyPropertyChanged("FileName");
            NotifyPropertyChanged("Data");
            NotifyPropertyChanged("LineCount");
            NotifyPropertyChanged("EntitiesCount");
            NotifyPropertyChanged("Size");
            NotifyPropertyChanged("VisualData");
            IsSaved = document.StringCount == LineCount;
        }

        public DrawingVisual CreateVisual(Size size)
        {
            return document.CreateVisual(size);
        }
    }
}
