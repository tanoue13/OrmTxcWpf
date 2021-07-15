using System;
using IBM.Data.Db2;
using OrmTxcWpf.Sql.Data;

namespace OrmTxcWpf.Db2.Sql.Data
{

    /// <summary>
    /// DB2Parameterの値に変換するコンバータ。
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class DB2ParameterValueConverter : IParameterValueConverter
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter">DB2Typeを指定する。<paramref name="value"/>がNullの場合、DB2Typeに応じた初期値を使用する。</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter)
        {
            if (value == null)
            {
                // nullの場合、DBNullを戻す。
                return DB2ParameterValueConverter.GetDataSourceNullValue(targetType, parameter);
            }
            //
            Type valueType = value.GetType();
            if (typeof(DBNull).Equals(valueType))
            {
                // DBNullを戻す。
                return DB2ParameterValueConverter.GetDataSourceNullValue(targetType, parameter);
            }
            else if (typeof(string).Equals(valueType))
            {
                // string型を変換する。
                var sValue = value as string;
                if (sValue != null)
                {
                    return value;
                }
                else
                {
                    return DB2ParameterValueConverter.GetDataSourceNullValue(targetType, parameter);
                }
            }
            else if (typeof(int?).Equals(valueType))
            {
                // int型を変換する。
                int? iValue = value as int?;
                if (iValue.HasValue)
                {
                    return value;
                }
                else
                {
                    return DB2ParameterValueConverter.GetDataSourceNullValue(targetType, parameter);
                }
            }
            else if (typeof(decimal?).Equals(valueType))
            {
                // int型を変換する。
                decimal? dValue = value as decimal?;
                if (dValue.HasValue)
                {
                    return value;
                }
                else
                {
                    return DB2ParameterValueConverter.GetDataSourceNullValue(targetType, parameter);
                }
            }
            else if (typeof(DateTime?).Equals(valueType))
            {
                // DateTime型を変換する。
                DateTime? dValue = value as DateTime?;
                if (dValue.HasValue)
                {
                    return value;
                }
                else
                {
                    return DB2ParameterValueConverter.GetDataSourceNullValue(targetType, parameter);
                }
            }
            // fool-proof
            return value;
        }
        /// <summary>
        /// データソースにおいて、nullとして扱われる値を取得する。（データ型に応じた値が戻される）
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        private static object GetDataSourceNullValue(Type targetType, object parameter)
        {
            switch (parameter)
            {
                case DB2Type.Numeric:
                    {
                        return Decimal.Zero;
                    }
                case DB2Type.Decimal:
                    {
                        return Decimal.Zero;
                    }
                case DB2Type.Char:
                    {
                        return String.Empty;
                    }
                case DB2Type.VarChar:
                    {
                        return String.Empty;
                    }
                case DB2Type.NChar:
                    {
                        return String.Empty;
                    }
                case DB2Type.NVarChar:
                    {
                        return String.Empty;
                    }
                default:
                    {
                        string message = "文字列／数値以外の型が存在します。";
                        var exception = new NotSupportedException(message);
                        throw exception;
                    }
            }
        }

    }

}
