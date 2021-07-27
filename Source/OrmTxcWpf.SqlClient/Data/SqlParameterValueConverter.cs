using System;
using OrmTxcWpf.Data;

namespace OrmTxcWpf.SqlClient.Data
{

    /// <summary>
    /// SqlParameterの値に変換するコンバータ。
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class SqlParameterValueConverter : IParameterValueConverter
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
                string sValue = value as string;
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
                    return this.SpecifyKindLocalIfUnspecified(dValue.Value);
                }
                else
                {
                    return DBNull.Value;
                }
            }
            else if (typeof(DateTime).Equals(valueType))
            {
                // DateTime型を変換する。
                if (value is DateTime dValue)
                {
                    return this.SpecifyKindLocalIfUnspecified(dValue);
                }
                else
                {
                    return value;
                }
            }
            // fool-proof
            return value;
        }

        /// <summary>
        /// DateTime.KindがUnspecifiedの場合、
        /// 指定された DateTime と同じティック数、および、Kind値がLocalを持つ新しい DateTime オブジェクトを戻す。
        /// </summary>
        /// <param name="value">DateTimeオブジェクト</param>
        /// <returns></returns>
        private DateTime SpecifyKindLocalIfUnspecified(DateTime value)
        {
            if (DateTimeKind.Unspecified == value.Kind)
            {
                DateTime dateTimeLocal = DateTime.SpecifyKind(value, DateTimeKind.Local);
                return dateTimeLocal;
            }
            //
            // fool-proof
            return value;
        }

    }

}
