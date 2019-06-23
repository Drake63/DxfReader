using System;
using System.ComponentModel;
using DxfReader;

namespace DxfOpener
{
    [Serializable]
    public class Profile
    {
        string name;
        bool lwPolyline;
        bool moveToZero;
        bool convertPolyline;
        bool convertEllipse;
        bool ignoreLineType;
        bool isR13;
        int precision;
        ushort flags;

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        public ushort Flags
        {
            get => flags;
            //set => flags = value;
            //{
            //    flags = value;
            //    LWPolyline = GetFlag(DxfFlags.CreateLwPolyline);
            //    MoveToZero = GetFlag(DxfFlags.MoveToZero);
            //    ConvertPolyline = GetFlag(DxfFlags.CreateArc);
            //    ConvertEllipse = GetFlag(DxfFlags.PolylineFromEllipse);
            //    IgnoreLineType = GetFlag(DxfFlags.IgnoreOtherLineType);
            //    IsR13 = GetFlag(DxfFlags.CreateR13);
            //}
        }


        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        public bool LWPolyline
        {
            get => lwPolyline;
            set
            {
                if (value != lwPolyline)
                {
                    lwPolyline = value;
                    NotifyPropertyChanged("LWPolyline");
                    ChangeFlag(DxfFlags.CreateLwPolyline);
                }
            }
        }
        public bool MoveToZero
        {
            get => moveToZero;
            set
            {
                if (value != moveToZero)
                {
                    moveToZero = value;
                    ChangeFlag(DxfFlags.MoveToZero);
                    NotifyPropertyChanged("MoveToZero");
                }
            }
        }
        public bool ConvertPolyline
        {
            get => convertPolyline;
            set
            {
                if (value != convertPolyline)
                {
                    convertPolyline = value;
                    ChangeFlag(DxfFlags.CreateArc);
                    NotifyPropertyChanged("ConvertPolyline");
                }
            }
        }
        public bool ConvertEllipse
        {
            get => convertEllipse;
            set
            {
                if (value != convertEllipse)
                {
                    convertEllipse = value;
                    ChangeFlag(DxfFlags.PolylineFromEllipse);
                    NotifyPropertyChanged("ConvertEllipse");
                }
            }
        }
        public bool IgnoreLineType
        {
            get => ignoreLineType;
            set
            {
                if (value != ignoreLineType)
                {
                    ignoreLineType = value;
                    ChangeFlag(DxfFlags.IgnoreOtherLineType);
                    NotifyPropertyChanged("IgnoreLineType");
                }
            }
        }

        public bool IsR13
        {
            get => isR13;
            set
            {
                if (value != isR13)
                {
                    isR13 = value;
                    ChangeFlag(DxfFlags.CreateR13);
                    NotifyPropertyChanged("ConvertR13");
                }
            }
        }

        public int Precision
        {
            get { return precision; }
            set
            {
                if (value == precision) return;

                precision = value;
                NotifyPropertyChanged("Precision");
            }
        }

        public bool IsCurrent { get; set; }

        private bool GetFlag(DxfFlags mask)
        {
            return (flags & (ushort)mask) > 0;
        }

        private void ChangeFlag(DxfFlags mask)
        {
            flags ^= (ushort)mask;
        }

        private void CheckFlag(DxfFlags mask)
        {
            flags |= (ushort)mask;
        }

        private void UncheckFlag(DxfFlags mask)
        {
            flags &= (ushort)~mask;
        }
    }
}
