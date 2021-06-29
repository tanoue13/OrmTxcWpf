using System;
using System.Threading;
using System.Windows;

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

        protected override void OnStartup(StartupEventArgs e)
        {
            // 多重起動を行わないため、セマフォを作成する。
            App.Semaphore = new Semaphore(1, 1, ApplicationName, out bool createdNew);
            //
            if (!createdNew)
            {
                string message = String.Format("既に起動されています。[アプリケーション名：{0}]", ApplicationName);
                //var exception = new ApplicationException(message);
                //throw exception;
                MessageBox.Show(message);
                Application.Current.Shutdown();
            }
            else
            {
                // Windowを表示する。
                var window = new MainWindow();
                window.Show();
            }
        }

    }

}
