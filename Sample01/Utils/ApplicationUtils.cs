using System;
using System.Threading;

namespace Sample01.Utils
{

    public class ApplicationUtils
    {

        public static Semaphore CreateSemaphore(string applicationName, Action action)
        {
            // 多重起動を行わないため、セマフォを作成する。
            Semaphore semaphore = new Semaphore(1, 1, applicationName, out bool createdNew);
            //
            if (!createdNew)
            {
                string message = String.Format("既に起動されています。[アプリケーション名：{0}]", applicationName);
                var exception = new ApplicationException(message);
                throw exception;
            }
            else
            {
                // 処理を実行する。
                action();
            }
            //
            // 結果を戻す。
            return semaphore;
        }
    }
}
