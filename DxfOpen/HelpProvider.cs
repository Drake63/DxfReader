using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace DxfOpener
{
    static class HelpProvider
    {
        public static readonly DependencyProperty HelpStringProperty;
        public static readonly DependencyProperty HelpNavigatorProperty;

        static HelpProvider()
        {
            HelpStringProperty = DependencyProperty.RegisterAttached("HelpString", typeof(string), typeof(HelpProvider));
            HelpNavigatorProperty = DependencyProperty.RegisterAttached("HelpNavigator", typeof(HelpNavigator), typeof(HelpProvider));
            CommandManager.RegisterClassCommandBinding(typeof(FrameworkElement),
                new CommandBinding(ApplicationCommands.Help, new ExecutedRoutedEventHandler(Executed), new CanExecuteRoutedEventHandler(CanExecute)));
        }

        private static void CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (HelpProvider.GetHelpString(sender as FrameworkElement) != null)
                e.CanExecute = true;
        }

        private static void Executed(object sender, ExecutedRoutedEventArgs e)
        {
            
            Help.ShowHelp(null, Properties.Resources.helpFileName, HelpProvider.GetHelpNavigator(sender as FrameworkElement), 
                HelpProvider.GetHelpString(sender as FrameworkElement));
        }

        public static HelpNavigator GetHelpNavigator(DependencyObject obj)
        {
            return (HelpNavigator)obj.GetValue(HelpNavigatorProperty);
        }

        public static void SetHelpNavigator(DependencyObject obj, HelpNavigator value)
        {
            obj.SetValue(HelpNavigatorProperty, value);
        }
        public static string GetHelpString(DependencyObject obj)
        {
            return (string)obj.GetValue(HelpStringProperty);
        }

        public static void SetHelpString(DependencyObject obj, string value)
        {
            obj.SetValue(HelpStringProperty, value);
        }
    }
}
