using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OrmTxcWpf.DataModels
{

    public class BindableDataModel : INotifyPropertyChanged
    {

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// ・プロパティの変更通知が大量に発生するとPropertyChangedEventArgsのインスタンスが大量に生成されてしまいます。
        /// 　これを防ぐためにstaticなところに避難させます。
        /// 
        /// ＜不具合対応＞
        /// ・System.Collections.Generic.Dictionary<TKey,TValue> クラスでは、
        /// 　値を設定する際（内部的にSystem.Collections.Generic.Dictionary`2.Insert(TKey key, TValue value, Boolean add)を実行する際）に
        /// 　NullReferenceExceptionが投げられることがある。
        /// ・Webの情報によると、この問題はスレッドに関係しているとの情報あり。（参考URLでは、lock構文による対応が紹介されている）
        /// ・System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue> クラスが、スレッドセーフなディクショナリと説明あるため、
        /// 　ここでは、ConcurrentDictionaryを使用することで対応する。
        /// </remarks>
        /// <see cref="https://blog.okazuki.jp/entry/2015/05/09/124333"/>
        /// <see cref="https://ja.coder.work/so/c%23/1655833"/>
        /// <see cref="https://stackoverflow.com/questions/1320621/throw-a-nullreferenceexception-while-calling-the-set-item-method-of-a-dictionary"/>
        private static IDictionary<string, PropertyChangedEventArgs> Dictionary { get; } = new ConcurrentDictionary<string, PropertyChangedEventArgs>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string name = "")
        {
            // スレッドセーフ化のため、ローカル変数に代入する。
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                // ディクショナリにPropertyChangedEventArgsが未登録の場合、登録する。
                if (!BindableDataModel.Dictionary.ContainsKey(name))
                {
                    BindableDataModel.Dictionary[name] = new PropertyChangedEventArgs(name);
                }
                // ディクショナリからPropertyChangedEventArgsを取得する。
                PropertyChangedEventArgs args = BindableDataModel.Dictionary[name];
                // イベントハンドラを処理する。
                handler(this, args);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="storage"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns>プロパティの値が変更された場合、true。変更されなかった場合、false。（同じ値の場合、値は変更されないためfalseとなる。</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = "")
        {
            // 元の値と等しい場合、変更を通知しない。
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            // プロパティに値を設定する。
            storage = value;
            // プロパティの変更を通知する。
            this.RaisePropertyChanged(propertyName);
            // 結果を戻す。
            return true;
        }

    }

}
