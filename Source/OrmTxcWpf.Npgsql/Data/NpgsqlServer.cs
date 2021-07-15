using System;
using System.Data;
using System.Linq;
using System.Reflection;
using Npgsql;
using NpgsqlTypes;
using OrmTxcWpf.Data;

namespace OrmTxcWpf.Npgsql.Data
{

    public class NpgsqlServer : DbServer<NpgsqlConnection>
    {

        private static IParameterValueConverter ParameterValueConverter { get; set; } = new NpgsqlParameterValueConverter();

        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換える。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="obj"></param>
        /// <param name="property"></param>
        /// <remarks></remarks>
        public static void AddParameterOrReplace(IDbCommand command, string parameterName, object obj, PropertyInfo property)
        {
            // パラメータのデータ型を取得する。
            Type propertyType = property.PropertyType;
            NpgsqlDbType dbType = NpgsqlDbType.Varchar; //default
            if (new Type[] { typeof(decimal), typeof(decimal?) }.Contains(propertyType))
            {
                dbType = NpgsqlDbType.Numeric;
            }
            else if (new Type[] { typeof(int), typeof(int?) }.Contains(propertyType))
            {
                dbType = NpgsqlDbType.Numeric;
            }
            else if (new Type[] { typeof(DateTime), typeof(DateTime?) }.Contains(propertyType))
            {
                dbType = NpgsqlDbType.TimestampTz;
            }
            else
            {
                // fool-proof
                dbType = NpgsqlDbType.Varchar;
            }
            // パラメータに設定する値を取得する。
            object value = property.GetValue(obj);
            //
            // nullを考慮し、下のメソッド経由で設定する。
            NpgsqlServer.AddParameterOrReplace(command, parameterName, dbType, value);
        }

        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換える。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        /// <remarks>値がnullの場合、DBNull.Valueに変換して設定する。</remarks>
        public static void AddParameterOrReplace(IDbCommand command, string parameterName, NpgsqlDbType dbType, object value)
        {
            IDataParameter parameter = new NpgsqlParameter(parameterName, dbType);
            parameter.Value = NpgsqlServer.ParameterValueConverter.Convert(value, null, null);
            DbServer<NpgsqlConnection>.AddParameterOrReplace(command, parameter);
        }
        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換えない。
        /// </summary>
        /// <param name="command">command</param>
        /// <param name="parameter">parameter</param>
        /// <remarks></remarks>
        public static void AddParameterIfNotExists(IDbCommand command, string parameterName, NpgsqlDbType dbType, object value)
        {
            IDataParameter parameter = new NpgsqlParameter(parameterName, dbType);
            parameter.Value = NpgsqlServer.ParameterValueConverter.Convert(value, null, null);
            DbServer<NpgsqlConnection>.AddParameterIfNotExists(command, parameter);
        }

        #region "更新系処理に関する処理"
        public static int ExecuteNonQuery(NpgsqlCommand command, bool enableOptimisticConcurrency = true)
        {
            try
            {
                int count = DbServer<NpgsqlConnection>.ExecuteNonQuery(command, enableOptimisticConcurrency);
                return count;
            }
            catch (NpgsqlException ex)
            {
                switch (ex.ErrorCode)
                {
                    case 23505:
                        {
                            // 一意性違反 unique_violation
                            string message = DbServer<NpgsqlConnection>.GetDBConcurrencyExceptionMessage(command);
                            // 例外を投げる。
                            var exception = new DBConcurrencyException(message);
                            throw exception;
                        }
                    default:
                        {
                            // 例外を投げる。（丸投げ）
                            throw;
                        }
                }

            }
        }
        #endregion

    }

}
