using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Sample01.Utils;

namespace Sample01
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        /// <summary>
        /// セマフォ（多重起動防止のために使用する）
        /// </summary>
        private static Semaphore Semaphore = null;

        private static readonly string ApplicationName = "Sample01";

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // セマフォを取得し、actionを実行する。
            App.Semaphore = ApplicationUtils.CreateSemaphore(ApplicationName, StartUpAction);
        }
        private void StartUpAction()
        {
            // Windowを表示する。
            var window = new MainWindow();
            window.Show();
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, null, MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }

}
