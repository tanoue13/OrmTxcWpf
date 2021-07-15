using System;
using OrmTxcWpf.Data;

namespace OrmTxcWpf.Npgsql.Data
{

    /// <summary>
    /// NpgsqlParameterの値に変換するコンバータ。
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class NpgsqlParameterValueConverter : IParameterValueConverter
    {

        public object Convert(object value, Type targetType, object parameter)
        {
            if (value == null)
            {
                // nullの場合、DBNullを戻す。
                return DBNull.Value;
            }
            //
            Type valueType = value.GetType();
            if (typeof(DBNull).Equals(valueType))
            {
                // DBNullを戻す。
                return DBNull.Value;
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
                    return DBNull.Value;
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
                    return DBNull.Value;
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
                    return DBNull.Value;
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
                    return DBNull.Value;
                }
            }
            // fool-proof
            return value;
        }

    }

}
