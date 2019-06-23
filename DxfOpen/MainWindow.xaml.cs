using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;

namespace DxfOpener
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PrintDialog printDialog = new PrintDialog();

        //WindowMessageService srv;

        #region Методы WinApi для установки и получения положения и состояния окна


        const int SW_SHOWNORMAL = 1;
        const int SW_SHOWMINIMIZED = 2; 

        #endregion


        public static RoutedCommand ShowMenu = new RoutedCommand();
        public static RoutedCommand ShowToolbar = new RoutedCommand();
        public static RoutedCommand ShowText = new RoutedCommand();
        public static RoutedCommand ShowVisual = new RoutedCommand();
        public static RoutedCommand ShowStatusBar = new RoutedCommand();
        public static RoutedCommand ChandgeProfile = new RoutedCommand();
        public static RoutedCommand ChangePoint = new RoutedCommand();
        public static RoutedCommand Refresh = new RoutedCommand();
        public static RoutedCommand SplineToPolyline = new RoutedCommand();
        public static RoutedCommand ShowBaseProfile = new RoutedCommand();
        public static RoutedCommand AboutCommand = new RoutedCommand();
        public static RoutedCommand OnlyOneWindow = new RoutedCommand();

        static Document document = new Document();
        string fileName = string.Empty;
        public static Profiles profiles = new Profiles();

        #region Методы относящиеся к окну

        public MainWindow()
        {
            InitializeComponent();

            // зарегистрировать события для объектов документа и профилей
            profiles.PropertyChanged += new PropertyChangedEventHandler(onProfilesChange);
            profiles.CollectionChanged += new CollectionChangeEventHandler(onCollectionChanged);
            document.PropertyChanged += new PropertyChangedEventHandler(onDocumentChanged);

            TaskbarItemInfo = new TaskbarItemInfo();
        }
        
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            BarsPositions pos = Properties.Settings.Default.BarsPositions;
            if (pos != null)
                for (int i = 0; i < toolBar.ToolBars.Count; i++)
                {
                    toolBar.ToolBars[i].Band = pos[i].Band;
                    toolBar.ToolBars[i].BandIndex = pos[i].BandIndex;
                }

            profiles = Properties.Settings.Default.Profiles;
            if (profiles == null)
                CreatePofiles();

            // Создать меню выбора профилей
            CreateSelectMenu();

            document = (Document)FindResource("doc");   // найти документ как ресурс окна
            // применить текущий профиль к документу
            DocumentSetProfile(profiles.CurrentProfile);

        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(WndProc);

            base.OnSourceInitialized(e);

            try
            {
                WINDOWPLACEMENT wp = Properties.Settings.Default.WindowPlacement;

                wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                wp.flags = 0;
                wp.showCmd = wp.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : wp.showCmd;
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                WindowPlacement.SetPlacement(hWnd, ref wp);
            }
            catch { }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            if (msg == SendMessageHelper.WM_COPYDATA)
            {
                SendMessageHelper.COPYDATASTRUCT data = (SendMessageHelper.COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(SendMessageHelper.COPYDATASTRUCT));

                string newFileName = Marshal.PtrToStringAnsi(data.lpData);
                if(File.Exists(newFileName))
                {
                    if (document.IsCreated && !document.IsSaved)
                    {
                        if (MessageBox.Show(string.Format(Properties.Resources.msgNotSaveFile, Path.GetFileName(document.FileName)),
                            Properties.Resources.msgSaveFileTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            File.WriteAllText(document.FileName, document.Data);
                    }

                    Cursor = Cursors.Wait;
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;

                    OpenFile(newFileName);
                    if (WindowState == WindowState.Minimized)
                        WindowState = WindowState.Normal;

                    Cursor = Cursors.Arrow;
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                }
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            //Удалить события связанные с коллекцией профилей
            profiles.PropertyChanged -= onProfilesChange;
            profiles.CollectionChanged -= onCollectionChanged;

            //создать новый двоичный форматер
            try
            {
                Properties.Settings.Default.Profiles = profiles;
                BarsPositions pos = new BarsPositions();
                foreach (ToolBar bar in toolBar.ToolBars)
                    pos.Add(new ToolBarPosition(bar.Band, bar.BandIndex));
                Properties.Settings.Default.BarsPositions = pos;
                Properties.Settings.Default.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Properties.Resources.msgSaveProfErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
            IntPtr hWnd = new WindowInteropHelper(this).Handle;
            WindowPlacement.GetPlacement(hWnd, out wp);
            Properties.Settings.Default.WindowPlacement = wp;
            Properties.Settings.Default.Profiles = profiles;

            Properties.Settings.Default.Save();


            // если документ не был создан - закрыть приложение
            if (!document.IsCreated) e.Cancel = false;
            else
            {
                try
                {
                    // если файл был изменен и изменения не были сохранены, предложить сохранить их
                    if (!document.IsSaved)
                        switch (MessageBox.Show(Properties.Resources.msgSaveModifiedFile, Properties.Resources.msgSavaModFileTitle,
                                MessageBoxButton.YesNoCancel, MessageBoxImage.Question))
                        {
                            case MessageBoxResult.Yes:      // нажата кнопка Да - сохранить измененный файл с тем же именем
                                File.WriteAllText(fileName, document.Data, Encoding.ASCII);
                                e.Cancel = false;
                                break;
                            case MessageBoxResult.No:       // нажата кнопка Нет - закрыть приложение не сохраняя ни чего
                                e.Cancel = false;
                                break;
                            case MessageBoxResult.Cancel:   // нажата кнопа Отмена - оставить приложение открытым
                                e.Cancel = true;
                                break;
                        }
                }
                catch (Exception ex)
                {
                    e.Cancel = true;
                    MessageBox.Show(ex.Message, Properties.Resources.msgErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // если при запуске в программу переданы данные
            if (Application.Current.Properties["args"] != null && Application.Current.Properties["args"] is string)
            {
                Cursor = Cursors.Wait;
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
                // получить строку и обработать ее
                string path = Application.Current.Properties["args"] as string;

                // если файл не найден, показать сообщение об ошибке
                if (!File.Exists(path))
                {
                    MessageBox.Show(Properties.Resources.msgPathNotFind, Properties.Resources.msgOpenFileErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // если файл не является файлом DXF показать сообщение об ошибке
                if (System.IO.Path.GetExtension(path).ToLower() != ".dxf")
                {
                    MessageBox.Show(Properties.Resources.msgNotDxf, Properties.Resources.msgOpenFileErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // попытаться открыть заданный файл
                OpenFile(path);
                // сохранить имя файла в переменной
                fileName = path;

                Cursor = Cursors.Arrow;
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
            }
        }

        // Иницилизация события SizeChanged главного окна
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            // задать размеры элементу управления, отвечающему за отрисовку
            // содержимого DXF файла
            docVisual.Width = ActualWidth * .73;
            docVisual.Height = ActualHeight * .72;
            // передать новые размеры изображения документу, для создания 
            // объекта DrawinVisual с новыми размерами
            document.VisualSize = docVisual.Size;
        }

        #endregion
        #region Команды

        // Возможность выполнения какой либо команды
        private void Command_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;    // команда всегда доступна для выполнения
        }

        // Команда открытия файла
        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(document.IsCreated && !document.IsSaved)
            {
                if (MessageBox.Show(string.Format(Properties.Resources.msgNotSaveFile, Path.GetFileName(document.FileName)),
                    Properties.Resources.msgSaveFileTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    File.WriteAllText(document.FileName, document.Data);
            }
            // создать диалог открытия файла
            OpenFileDialog dialog = new OpenFileDialog();
            // добавить фильтр для файлов DXF
            dialog.Filter = Properties.Resources.openDialogFilters;

            Cursor = Cursors.Wait;
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;

            try
            {
                // Если нажата кнопка отмены, выйти из команды
                if (dialog.ShowDialog() == false) return;
                // получить имя выбранного файла
                fileName = dialog.FileName;
                OpenFile(fileName);
            }
            finally
            {
                Cursor = Cursors.Arrow;
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
            }
        }

        // Достyность команды сохранения файла
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = document.IsCreated && !document.IsSaved;
        }

        private void SaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = document.IsCreated;
        }

        // Команда сохранения файла
        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // пытаться сохранить файл с тем же именем
                File.WriteAllText(document.FileName, document.Data, Encoding.ASCII);
                // Перезагрузить файл
                document.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, string.Format(Properties.Resources.msgWritingFileErrorTitle, Path.GetFileName(document.FileName)),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Команда Сохранить как
        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // пытаться сохранить файл под другим именем
            try
            {
                SaveFileDialog dialog = new SaveFileDialog
                {
                    Filter = Properties.Resources.openDialogFilters,    // добавить фильтр для фалов DXF
                    FileName = Path.GetFileName(document.FileName)      // установить текущее имя файла для диалога
                };           // создать диалог сохранения файла

                if (dialog.ShowDialog() == false) return;               // если нажата кнопка отмены, выйти из выполнения команды

                // получить новое имя файла
                fileName = dialog.FileName;
                // сохранить файл с новым именем в кодировке ASCII
                File.WriteAllText(fileName, document.Data, Encoding.ASCII);
                document.FileName = fileName;
                //document.Refresh();
                Title = Properties.Resources.titleMainWindow + " - [" + Path.GetFileName(document.FileName) + "]";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, string.Format(Properties.Resources.msgWritingFileErrorTitle, Path.GetFileName(document.FileName)),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Команда закрытия приложения
        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        // Команда печати файла DXF
        private void Print_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!document.IsCreated)
                MessageBox.Show(Properties.Resources.msgPrintError, Properties.Resources.msgPrintErrorTitle,
                    MessageBoxButton.OK, MessageBoxImage.Error);

            if (docVisual.Visual.Children.Count > 0)
            {
                //PrintDialog dialog = new PrintDialog();

                if (printDialog.ShowDialog() == true)
                {
                    Size visualSize = new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
                    DxfReader.DocumentFrameworkElement visual = new DxfReader.DocumentFrameworkElement
                    {
                        Margin = new Thickness(15)
                    };

                    DrawingVisual image = document.CreateVisual(visualSize);
                    visual.AddVisual(image);
                    visual.LayoutTransform = new ScaleTransform { ScaleY = -1 };

                    visual.Measure(visualSize);
                    visual.Arrange(new Rect(0, 0, visualSize.Width, visualSize.Height));

                    printDialog.PrintVisual(visual, Path.GetFileName(document.FileName));
                }
            }
        }

        // Включение\отключение режима отображения только одного экземпляра приложения
        private void OnlyOneWindow_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            menuOnlyOneWindow.IsChecked = !menuOnlyOneWindow.IsChecked;  // переключить режим
            // если в профиле другое значение, изменить его на текущее и сохранить профиль
            if(Properties.Settings.Default.onlyOneWindow != menuOnlyOneWindow.IsChecked)    
            {
                Properties.Settings.Default.onlyOneWindow = menuOnlyOneWindow.IsChecked;
                Properties.Settings.Default.Save();
            }
        }

        // Команда скрыть/показать строку меню
        private void ShowMenu_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            menuShowMenu.IsChecked = !menuShowMenu.IsChecked;
            if (Properties.Settings.Default.showMenu != menuShowMenu.IsChecked)
                Properties.Settings.Default.showMenu = menuShowMenu.IsChecked;
        }

        // Команда скрыть/показать тулбар
        private void ShowToolBar_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            menuShowToolBar.IsChecked = !menuShowToolBar.IsChecked;
            if (Properties.Settings.Default.showToolBar != menuShowToolBar.IsChecked)
                Properties.Settings.Default.showToolBar = menuShowToolBar.IsChecked;
        }

        // Команда скрыть/показать текст файла DXF
        private void ShowText_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            menuShowText.IsChecked = !menuShowText.IsChecked;
            if (Properties.Settings.Default.showText != menuShowText.IsChecked)
                Properties.Settings.Default.showText = menuShowText.IsChecked;
        }

        // Команда скрыть/показать графическое изображение
        private void ShowVisual_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            menuShowVisual.IsChecked = !menuShowVisual.IsChecked;
            if (Properties.Settings.Default.showVisual != menuShowVisual.IsChecked)
                Properties.Settings.Default.showVisual = menuShowVisual.IsChecked;
        }

        // Команда скрыть/показать статус бар
        private void ShowStatusBar_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            menuShowStatusBar.IsChecked = !menuShowStatusBar.IsChecked;
            if (Properties.Settings.Default.showStatusBar != menuShowStatusBar.IsChecked)
                Properties.Settings.Default.showStatusBar = menuShowStatusBar.IsChecked;
        }

        // Команда создания нового профиля
        private void NewProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // создать диалоговое окно для настойки и создания профиля
            ProfileDialog dialog = new ProfileDialog(true, null);
            dialog.Owner = this;

            // если результат показа диалога равен true, создать новый профиль
            if(dialog.ShowDialog() == true)
            {
                Profile profile = new Profile();
                // установить выбранные значения в диалоге для нового профиля
                profile.Name = dialog.ProfileName;
                profile.Precision = dialog.Precision;
                profile.LWPolyline = dialog.LWPolyline;
                profile.ConvertPolyline = dialog.ConvertPolyline;
                profile.MoveToZero = dialog.MoveToZero;
                profile.IgnoreLineType = dialog.IgnoreOtherLines;
                profile.ConvertEllipse = dialog.ConvertEllipse;
                profile.IsR13 = dialog.CreateR13;

                // добавить профиль в коллекцию
                profiles.Add(profile);
                profiles.CurrentProfile = profiles.Count - 1;
            }
        }

        // Условия доступности команды удаления профиля
        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // если коллеция существует и выбан профиль не являющийся базовым, то его можно удалить
            if (profiles == null || profiles.Count <= 3 || profiles.CurrentProfile == 0 || profiles.CurrentProfile == 1 || profiles.CurrentProfile == 2)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        // Команда удаления профиля
        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // запросить подтвержнее на удаление профиля, если получено - удалить текущий профиль
            if (MessageBox.Show(string.Format(Properties.Resources.msgProfileDelete, profiles[profiles.CurrentProfile].Name),
                Properties.Resources.msgProfileDelTitle, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                profiles.RemoveAt(profiles.CurrentProfile);
        }

        // Команда изменения текущего профиля
        private void ChandgeProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // создать диалог создания и редактирования профиля
            ProfileDialog dialog = new ProfileDialog(false, profiles[profiles.CurrentProfile])
            {
                Owner = this
            };

            // показать его, если результатом является true, установить новые значения в текущем профиле
            if (dialog.ShowDialog() == true)
            {
                profiles[profiles.CurrentProfile].Precision = dialog.Precision;
                profiles[profiles.CurrentProfile].LWPolyline = dialog.LWPolyline;
                profiles[profiles.CurrentProfile].ConvertPolyline = dialog.ConvertPolyline;
                profiles[profiles.CurrentProfile].MoveToZero = dialog.MoveToZero;
                profiles[profiles.CurrentProfile].IgnoreLineType = dialog.IgnoreOtherLines;
                profiles[profiles.CurrentProfile].ConvertEllipse = dialog.ConvertEllipse;
                profiles[profiles.CurrentProfile].IsR13 = dialog.CreateR13;
            }
        }

        // Условия доступности комнады изменения точкек
        private void ChangePoint_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (document.IsPoints)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        // Команда изменения точек, если такие имеются в файле DXF
        private void ChangePoint_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // открыть диалог, предлагающий выбор действий с точками
            ProfileDialog dialog = new ProfileDialog(document.PointsCount);
            dialog.Owner = this;

            if (dialog.ShowDialog() == true)
            {
                // если выбрано удалить все точки, запустить метод удаления
                if (dialog.DelPoints)
                    document.DeletePoints();
                else
                    // если выбран цвет, то изменить цвет
                    document.ChangePointsColor(dialog.Color);
            }
        }

        // Условия доступности команды обновления документа
        private void Refresh_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // команда доступна, если документ создан и еще не сохранен
            //e.CanExecute = document.IsCreated && !document.IsSaved;
            e.CanExecute = true;
        }

        // Команда обновления документа
        private void Refresh_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DocumentRefresh();     // перезагрузить документ
        }

        private void AboutCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProfileDialog dialog = new ProfileDialog();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        private void SplineToPolyline_canExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = document.ContainsSplines;
        }

        private void SplineToPolyline_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(precisiontSpline.Value < 2)
            {
                MessageBox.Show(Properties.Resources.msgSmallPrecision, Properties.Resources.msgErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                precisiontSpline.Focus();
                return;
            }
            document.SplinePrecision = precisiontSpline.Value;
            document.ContainsSplines = false;
        }

        private void ShowBaseProfile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = profiles.CurrentProfile < 3;
        }

        private void ShowBaseProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProfileDialog dialog = new ProfileDialog(false, profiles[profiles.CurrentProfile]);
            dialog.Owner = this;

            dialog.ShowDialog();
        }

        #endregion
        #region События

        // Изменение выбора в выпадающем списке профилей
        private void profList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // присвоить новое значение параметру текущий профиль
            profiles.CurrentProfile = profList.SelectedIndex;
        }

        // Событие при изменении свойств документа
        private void onDocumentChanged(object sender, PropertyChangedEventArgs e)
        {
            // изменение визуального отображения документа
            if (e.PropertyName == "VisualData")
            {
                docVisual.Visual = document.VisualData;
            }
        }

        // Событие при изменении состава коллекции профилей
        private void onCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            // создать новое меню выбора профиля
            CreateSelectMenu();
        }

        // Щелчок на одном из пунктов меню выбора профиля
        private void selectMenuOnClick(object sender, RoutedEventArgs e)
        {
            // определить на каком из пунктов был сделан щелчок и установить новое значение текущего профиля
            for (int i = 0; i < profiles.Count; i++)
                if (profiles[i].Name == (sender as MenuItem).Header as string)
                    profiles.CurrentProfile = i;
        }

        // Событие изменения профиля
        private void onProfilesChange(object sender, PropertyChangedEventArgs e)
        {
            // если произошло изменение текущего профиля, изменить 
            // меню и выпадающий список в соответствии с этим
            if (e.PropertyName == "CurrentProfile")
                CreateSelectMenu();
            // применить новые изменения к документу
            DocumentSetProfile(profiles.CurrentProfile);
        }

        private void Image_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetSource((bool)e.NewValue, (System.Windows.Controls.Image)sender);
        }

        #region Контекстное меню окна, дублирует меню вид

        private void contextShowToolBar_Click(object sender, RoutedEventArgs e)
        {
            contextMenuShowToolBar.IsChecked = !contextMenuShowToolBar.IsChecked;
            if (Properties.Settings.Default.showToolBar != contextMenuShowToolBar.IsChecked)
                Properties.Settings.Default.showToolBar = contextMenuShowToolBar.IsChecked;
        }

        private void contextMenuShowText_Click(object sender, RoutedEventArgs e)
        {
            contextMenuShowText.IsChecked = !contextMenuShowText.IsChecked;
            if (Properties.Settings.Default.showText != contextMenuShowText.IsChecked)
                Properties.Settings.Default.showText = contextMenuShowText.IsChecked;
        }

        private void contextMenuShowMenu_Click(object sender, RoutedEventArgs e)
        {
            contextMenuShowMenu.IsChecked = !contextMenuShowMenu.IsChecked;
            if (Properties.Settings.Default.showMenu != contextMenuShowMenu.IsChecked)
                Properties.Settings.Default.showMenu = contextMenuShowMenu.IsChecked;
        }

        private void contextMenuShowVisual_Click(object sender, RoutedEventArgs e)
        {
            contextMenuShowVisual.IsChecked = !contextMenuShowVisual.IsChecked;
            if (Properties.Settings.Default.showVisual != contextMenuShowVisual.IsChecked)
                Properties.Settings.Default.showVisual = contextMenuShowVisual.IsChecked;
        }

        private void contextMenuShowStatusBar_Click(object sender, RoutedEventArgs e)
        {
            contextMenuShowStatusBar.IsChecked = !contextMenuShowStatusBar.IsChecked;
            if (Properties.Settings.Default.showStatusBar != contextMenuShowStatusBar.IsChecked)
                Properties.Settings.Default.showStatusBar = contextMenuShowStatusBar.IsChecked;
        } 

        #endregion

        #endregion
        #region Вспомогательные методы

        // Открытие файла
        private void OpenFile(string fileName)
        {
            try
            {
                // Временная метка начала открытия
                DateTime startTime = DateTime.Now;
                // передать имя файла 
                document.FileName = fileName;

                // добавить к заголовку главного окна имя открытого файла
                //Title = Properties.Resources.titleMainWindow + " - [" + Path.GetFileName(document.FileName) + "]";



                //textTime.Text = string.Format(Properties.Resources.timeText, time.TotalSeconds.ToString("N4"));

                FindPoints();
                // временная метка конца процедуры открытия файла
                DateTime endTime = DateTime.Now;
                // определить интревал времени за который был открыт файл и показать результат в статус баре
                TimeSpan time = endTime - startTime;
                //добавить к заголовку главного окна имя открытого файла
                Title = Properties.Resources.titleMainWindow + " - [" + Path.GetFileName(document.FileName) + "]";
                textTime.Text = string.Format(Properties.Resources.timeText, time.TotalSeconds.ToString("N4"));
            }
            catch (Exception ex)
            {
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
                MessageBox.Show(ex.Message, Properties.Resources.msgOpenFileErrorTitle + " " + Path.GetFileName(fileName),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FindPoints()
        {
            // если в документе есть точки не отмеченные как разметка или незамкнутый контур
            if (document.IsPointsAsMainLine)
            {
                // открыть диалог, предлагающий выбор, что сделать с точками
                ProfileDialog dialog = new ProfileDialog(document.PointsCount);
                dialog.Owner = this;

                if (dialog.ShowDialog() == true)
                {
                    // если выбрано удалить все точки, запустить метод удаления
                    if (dialog.DelPoints)
                        document.DeletePoints();
                    else
                        // если выбран цвет, то изменить цвет
                        document.ChangePointsColor(dialog.Color);
                }
            }
        }

        private void DocumentRefresh()
        {
            Cursor = Cursors.Wait;
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
            try
            {
                document.Refresh();
                FindPoints();
            }
            catch
            {
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
            }
            finally
            {
                Cursor = Cursors.Arrow;
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
            }
        }

        // Создание основных профилей
        private void CreatePofiles()
        {
            profiles = new Profiles
            {
                new Profile()
                {
                    Name = Properties.Resources.profileBase_1,
                    ConvertEllipse = false,
                    ConvertPolyline = false,
                    IgnoreLineType = false,
                    LWPolyline = true,
                    MoveToZero = true,
                    IsR13 = false, 
                    IsCurrent=true
                },
                new Profile()
                {
                    Name = Properties.Resources.profileBase_2,
                    ConvertEllipse = true,
                    ConvertPolyline = true,
                    IgnoreLineType = false,
                    LWPolyline = false,
                    MoveToZero = true,
                    IsR13 = false,
                    Precision = 50
                },
                new Profile
                {
                    Name = Properties.Resources.profileBase_3,
                    ConvertEllipse = false,
                    ConvertPolyline = false,
                    IgnoreLineType = false,
                    LWPolyline = true,
                    MoveToZero = true,
                    IsR13 = true
                },                
            };
        } 

        // Настройка документа согласно заданному профилю
        private void DocumentSetProfile(int index)
        {
            if (index < 0) return;

            document.Precision = profiles[index].Precision;
            document.Flags = profiles[index].Flags;
        }

        // Создание меню выбора профиля
        private void CreateSelectMenu()
        {
            // очистить существующее меню
            menuSelectProfile.Items.Clear();

            for (int i = 0; i < profiles.Count; i++)
            {
                // создать новый пункт меню
                MenuItem item = new MenuItem();
                item.Header = profiles[i].Name;     // текст пункта
                // если идекс текущего профиля совпадает с индексом 
                // пункта, отметить его как выбранный
                item.IsChecked = i == profiles.CurrentProfile ? true : false;
                // добавить событие щелчка к новому пункту
                item.Click += new RoutedEventHandler(selectMenuOnClick);
                // добавить его в меню
                menuSelectProfile.Items.Add(item);

            }

            // создать привязку выпадающему списку к коллекции профилей
            // отменить существующую и очистить список
            profList.ItemsSource = null;
            profList.Items.Clear();
            // задать новую привязку
            profList.ItemsSource = profiles;
            // установить свойство для отображения в списке
            profList.DisplayMemberPath = "Name";
            // сделать выбранным текущий профиль
            profList.SelectedIndex = profiles.CurrentProfile;
        }

        private void SetSource(bool isEnabled, System.Windows.Controls.Image image)
        {
            string path = image.Source.ToString();
            path.Trim();
            int index = 0;

            if (isEnabled && path.Contains("_"))
            {
                index = path.IndexOf('_');
                path = path.Remove(index, 1);
                image.Opacity = 1.0;
            }
            else
            {
                index = path.LastIndexOf('.');
                path = path.Insert(index, "_");
                image.Opacity = .4;
            }

            image.Source = new BitmapImage(new Uri(path));
        }

        #endregion

        private void textBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            document.Data = e.Text;
        }

        //private void OpenComplete(IAsyncResult result)
        //{
        //    AsyncResult ar = (AsyncResult)result;
        //    NewFile res = (NewFile)ar.AsyncDelegate;
        //    document = res.EndInvoke(result);
        //}
        //public delegate void NewFile(string fileName);
    }
}
