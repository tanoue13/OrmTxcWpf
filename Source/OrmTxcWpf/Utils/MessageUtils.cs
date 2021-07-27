using System;

namespace OrmTxcWpf.Utils
{

    public class MessageUtils
    {
        /// <summary>
        /// コンストラクタ（ダミー）
        /// </summary>
        private MessageUtils()
        {
            // no-op
        }

        /// <summary>
        /// Enumに不適切な値が設定されている場合のエラーメッセージを返す。
        /// </summary>
        /// <param name="enumValue">指定された列挙体の値</param>
        /// <returns>エラーメッセージ</returns>
        public static string GetInvalidEnumArgumentExceptionMessage<TEnum>(int enumValue) where TEnum : struct
        {
            // メッセージ形式を定義する。
            string format = "{0}に予期せぬ値（{1}）が設定されました。";
            //
            // Enumの名前を取得する。
            string name = typeof(TEnum).Name;
            // Enumの値を取得する。デフォルト値として、値をそのまま設定する。
            string value = enumValue.ToString();
            if (Enum.IsDefined(typeof(TEnum), enumValue))
            {
                // Enumに既定の値があれば対応する文字列を設定する。
                value = Enum.GetName(typeof(TEnum), enumValue);
            }
            //
            // 結果を戻す。
            string message = String.Format(format, name, value);
            return message;
        }

    }

}
