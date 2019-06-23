using System;
using System.Windows;
using System.Windows.Controls;
using DxfReader;
using System.Management;
using System.Windows.Media.Imaging;

namespace DxfOpener
{
    /// <summary>
    /// Логика взаимодействия для SetProfile.xaml
    /// </summary>
    public partial class ProfileDialog : Window
    {
        string profileName;
        int precision;
        bool lwPolyline;
        bool conPolyline;
        bool ignoreLines;
        bool zero;
        bool convEllipse;
        bool isR13;
        
        DxfColors colorPoint;

        public string ProfileName
        {
            get { return profileName; }
            set { profileName = value; }
        }

        public int Precision
        {
            get { return precision; }
            set { precision = value; }
        }

        public bool LWPolyline
        {
            get { return lwPolyline; }
            set { lwPolyline = value; }
        }

        public bool ConvertPolyline
        {
            get { return conPolyline; }
            set { conPolyline = value; }
        }

        public bool IgnoreOtherLines
        {
            get { return ignoreLines; }
            set { ignoreLines = value; }
        }

        public bool MoveToZero
        {
            get { return zero; }
            set { zero = value; }
        }

        public bool ConvertEllipse
        {
            get { return convEllipse; }
            set { convEllipse = value; }
        }

        public bool CreateR13
        {
            get => isR13;
            set => isR13 = value;
        }

        public bool DelPoints { get { return delPoints.IsChecked == true ? true : false; } }

        public DxfColors Color { get { return colorPoint; } }

        public ProfileDialog(bool create, Profile profile)
        {
            InitializeComponent();

            mainBorder.Visibility = Visibility.Visible;
            aboutGrid.Visibility = Visibility.Collapsed;
            setProfile.Visibility = Visibility.Visible;
            pointsSetting.Visibility = Visibility.Collapsed;

            if (profile != null && (profile.Name == Properties.Resources.profileBase_1 || profile.Name == Properties.Resources.profileBase_2 || profile.Name == Properties.Resources.profileBase_3))
            {
                btnOK.Visibility = Visibility.Hidden;
                btnCancel.Content = Properties.Resources.dialogButtonOk;
                Title = string.Format(Properties.Resources.dialogTitleView, profile.Name);
                HelpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
                HelpProvider.SetHelpString(this, Properties.Resources.topicInner);

                nameProfile.Text = profile.Name;
                nameProfile.IsReadOnly = true;
                LwPolyline.IsChecked = profile.LWPolyline;
                LwPolyline.IsEnabled = false;
                convertPolyline.IsChecked = profile.ConvertPolyline;
                convertPolyline.IsEnabled = false;
                ignoreOtherLines.IsChecked = profile.IgnoreLineType;
                ignoreOtherLines.IsEnabled = false;
                moveToZero.IsChecked = profile.MoveToZero;
                moveToZero.IsEnabled = false;
                convertEllipse.IsChecked = profile.ConvertEllipse;
                convertEllipse.IsEnabled = false;
                precisionValue.Value = profile.Precision;
                precisionValue.IsEnabled = false;
                createR13.IsChecked = profile.IsR13;
                createR13.IsEnabled = false;
            }
            else if (create)
            {
                btnOK.Content = Properties.Resources.dialogButtonCreate;
                Title = Properties.Resources.dialogTitleNew;
                HelpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
                HelpProvider.SetHelpString(this, Properties.Resources.topicCustom);
            }
            else
            {
                btnOK.Content = Properties.Resources.dialogButtonModify;
                Title = string.Format(Properties.Resources.dialogTitleModify, profile.Name);

                nameProfile.Text = profileName = profile.Name;
                nameProfile.Focusable = false;
                LwPolyline.IsChecked = lwPolyline = profile.LWPolyline;
                convertPolyline.IsChecked = conPolyline = profile.ConvertPolyline;
                ignoreOtherLines.IsChecked = ignoreLines = profile.IgnoreLineType;
                moveToZero.IsChecked = zero = profile.MoveToZero;
                //convertEllipse.IsChecked = precisionValue.Focusable = precisionValue.IsEnabled = convEllipse = profile.ConvertEllipse;
                convertEllipse.IsChecked = convEllipse = profile.ConvertEllipse;
                precisionValue.Value = profile.Precision;
                createR13.IsChecked = isR13 = profile.IsR13;
                HelpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
                HelpProvider.SetHelpString(this, Properties.Resources.topicInfo);
            }
        }

        public ProfileDialog(int pointsCount)
        {
            InitializeComponent();

            mainBorder.Visibility = Visibility.Visible;
            aboutGrid.Visibility = Visibility.Collapsed;
            setProfile.Visibility = Visibility.Collapsed;
            pointsSetting.Visibility = Visibility.Visible;
            color.IsEditable = false;
            color.IsEnabled = false;
            info.Content = IntToString(pointsCount);
            btnOK.Content = Properties.Resources.dialogButtonRun;
            Title = Properties.Resources.dialogTitleModifyPoints;
            HelpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            HelpProvider.SetHelpString(this, Properties.Resources.topicPoinsDialog);
        }

        public ProfileDialog()
        {
            InitializeComponent();

            mainBorder.Visibility = Visibility.Collapsed;
            aboutGrid.Visibility = Visibility.Visible;
            btnOK.Visibility = Visibility.Hidden;
            btnCancel.Content = Properties.Resources.dialogButtonOk;
            btnCancel.Focus();
            Title = Properties.Resources.dialogTitleAbout;
            Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Info.ico", UriKind.RelativeOrAbsolute));
            HelpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            HelpProvider.SetHelpString(this, Properties.Resources.topicAboutDialog);

            machineName.Content = Environment.MachineName;

            ManagementObjectSearcher sercher = new ManagementObjectSearcher("root\\CIMV2", "Select * From Win32_OperatingSystem");
            foreach(ManagementObject obj in sercher.Get())
            {
                osName.Content = obj["Caption"].ToString();
                if(obj["ServicePackMajorVersion"].ToString() != "0")
                {
                    osName.Content += " ServicePack " + obj["ServicePackMajorVersion"].ToString() +
                        "." + obj["ServicePackMinorVersion"].ToString();
                }
                osVersion.Content = obj["Version"].ToString();
                userName.Content = obj["RegisteredUser"].ToString();
            }
        }

        private string IntToString(int pointsCount)
        {
            string str = "";
            switch(pointsCount % 10)
            {
                case 1:
                    if (pointsCount > 100 && pointsCount % 100 == 11)
                    {
                        str = string.Format(Properties.Resources.dialogFoundPoints, Properties.Resources.dialogFoundPointsContains_2,
                            pointsCount.ToString(), Properties.Resources.dialogStringPoints2);
                        break;
                    }
                    if(pointsCount == 11)
                    {
                        str = string.Format(Properties.Resources.dialogFoundPoints, Properties.Resources.dialogFoundPointsContains_2, 
                            pointsCount.ToString(), Properties.Resources.dialogStringPoints2);
                        break;
                    }
                    str = string.Format(Properties.Resources.dialogFoundPoints, Properties.Resources.dialogFoundPointsContains_1,
                            pointsCount.ToString(), Properties.Resources.dialogStringPoint);
                    break;
                case 2:
                case 3:
                case 4:
                    if(pointsCount > 100 && pointsCount % 100 > 11 && pointsCount % 100 < 15)
                    {
                        str = string.Format(Properties.Resources.dialogFoundPoints, Properties.Resources.dialogFoundPointsContains_2,
                                            pointsCount.ToString(), Properties.Resources.dialogStringPoints2);
                        break;
                    }
                    if (pointsCount > 11 && pointsCount < 15)
                    {
                        str = string.Format(Properties.Resources.dialogFoundPoints, Properties.Resources.dialogFoundPointsContains_2,
                                            pointsCount.ToString(), Properties.Resources.dialogStringPoints2);
                        break; 
                    }
                    str = string.Format(Properties.Resources.dialogFoundPoints, Properties.Resources.dialogFoundPointsContains_2,
                            pointsCount.ToString(), Properties.Resources.dialogStringPoints);
                    break;
                default:
                    str = string.Format(Properties.Resources.dialogFoundPoints, Properties.Resources.dialogFoundPointsContains_2,
                            pointsCount.ToString(), Properties.Resources.dialogStringPoints2);
                    break;
            }
            return str;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Title != Properties.Resources.dialogTitleModifyPoints)
            {
                if (string.IsNullOrEmpty(profileName))
                {
                    MessageBox.Show(Properties.Resources.msgProfNameEmpty, Properties.Resources.msgCreateProfErrorTitle,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    nameProfile.Focus();
                    return;
                }
                else if (convEllipse)
                {
                    if (precisionValue.Value < 2)
                    {
                        MessageBox.Show(Properties.Resources.msgSmallPrecision, Properties.Resources.msgErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                        precisionValue.Focus();
                        return;
                    }
                    precision = precisionValue.Value;
                } 
            }

            DialogResult = true;
        }

        private void nameProfile_TextChanged(object sender, TextChangedEventArgs e)
        {
            profileName = nameProfile.Text;
        }

        private void LwPolyline_Click(object sender, RoutedEventArgs e)
        {
            lwPolyline = LwPolyline.IsChecked == true ? true : false;
        }

        private void convertPolyline_Click(object sender, RoutedEventArgs e)
        {
            conPolyline = convertPolyline.IsChecked == true ? true : false;
        }

        private void ignoreOtherLines_Click(object sender, RoutedEventArgs e)
        {
            ignoreLines = ignoreOtherLines.IsChecked == true ? true : false;
        }

        private void moveToZero_Click(object sender, RoutedEventArgs e)
        {
            zero = moveToZero.IsChecked == true ? true : false;
        }

        private void convertEllipse_Click(object sender, RoutedEventArgs e)
        {
            convEllipse = convertEllipse.IsChecked == true ? true : false;
        }

        private void setType_Click(object sender, RoutedEventArgs e)
        {
            color.IsEnabled = setType.IsChecked == true ? true : false;
        }

        private void color_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (color.SelectedIndex == 0)
                colorPoint = DxfColors.LineOfMarking;
            else
                colorPoint = DxfColors.NotClosedLine;
        }

        private void createR13_Click(object sender, RoutedEventArgs e)
        {
            isR13 = createR13.IsChecked == true ? true : false;
        }
    }
}
