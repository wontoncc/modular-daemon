using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace modular_daemon {
    public class NotifyIconCommand : ICommand {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            var currentWindow = Application.Current.Windows[0];
            currentWindow.Show();
            currentWindow.WindowState = WindowState.Normal;
        }
    }

    public class ExitAppCommand : ICommand {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter) {
            return true;
        }

        public void Execute(object parameter) {
            Application.Current.Windows[0].Close();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            string[] args = Environment.GetCommandLineArgs();
            string configPath = "config.xml";
            if (args.Length > 1) {
                configPath = args[1].Contains("config") ? args[1].Split('=')[1] : configPath;
            }

            var config = Config.Load(configPath);
            this.DataContext = config.Services;
            this.Title = config.Title;

            InitializeComponent();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
            foreach (var service in this.DataContext as List<Service>) {
                service.EndProcess();
                service.Close();
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            var textbox = sender as TextBox;
            textbox.Focus();
            textbox.CaretIndex = textbox.Text.Length;
            textbox.ScrollToEnd();
        }

        protected override void OnStateChanged(EventArgs e) {
            if (WindowState == WindowState.Minimized) {
                this.Hide();
            }
            base.OnStateChanged(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            var services = this.DataContext as List<Service>;
            services[commandTabs.SelectedIndex].Restart();
        }
    }
}
