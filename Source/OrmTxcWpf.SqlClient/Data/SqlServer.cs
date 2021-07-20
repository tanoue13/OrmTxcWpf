using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using OrmTxcWpf.Data;
using OrmTxcWpf.Utils;

namespace OrmTxcWpf.SqlClient.Data
{

    public class SqlServer : DbServer<SqlConnection>
    {

        private static IParameterValueConverter ParameterValueConverter { get; set; } = new SqlParameterValueConverter();

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
            SqlDbType dbType = SqlDbType.NVarChar; //default
            if (new Type[] { typeof(decimal), typeof(decimal?) }.Contains(propertyType))
            {
                dbType = SqlDbType.Decimal;
            }
            else if (new Type[] { typeof(int), typeof(int?) }.Contains(propertyType))
            {
                dbType = SqlDbType.Decimal;
            }
            else if (new Type[] { typeof(DateTime), typeof(DateTime?) }.Contains(propertyType))
            {
                dbType = SqlDbType.Timestamp;
            }
            else
            {
                // fool-proof
                dbType = SqlDbType.NVarChar;
            }
            // パラメータに設定する値を取得する。
            object value = property.GetValue(obj);
            //
            // nullを考慮し、下のメソッド経由で設定する。
            SqlServer.AddParameterOrReplace(command, parameterName, dbType, value);
        }

        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換える。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        /// <remarks>値がnullの場合、DBNull.Valueに変換して設定する。</remarks>
        public static void AddParameterOrReplace(IDbCommand command, string parameterName, SqlDbType dbType, object value)
        {
            IDataParameter parameter = CreateSqlParameter(parameterName, dbType, value);
            parameter.Value = SqlServer.ParameterValueConverter.Convert(value, null, null);
            DbServer<SqlConnection>.AddParameterOrReplace(command, parameter);
        }
        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換えない。
        /// </summary>
        /// <param name="command">command</param>
        /// <param name="parameter">parameter</param>
        /// <remarks></remarks>
        public static void AddParameterIfNotExists(IDbCommand command, string parameterName, SqlDbType dbType, object value)
        {
            IDataParameter parameter = CreateSqlParameter(parameterName, dbType, value);
            parameter.Value = SqlServer.ParameterValueConverter.Convert(value, null, null);
            DbServer<SqlConnection>.AddParameterIfNotExists(command, parameter);
        }
        /// <summary>
        /// 可変長データ型に応じたサイズ（Size, Precision, Scale）が設定済みのパラメータを生成する。
        /// /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="dataType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <remarks>
        /// ・SQLServerではPrepareを呼び出す際、可変長データ型のパラメータにおいてはサイズの設定が必要。
        /// <seealso cref="https://docs.microsoft.com/ja-jp/dotnet/api/system.data.sqlclient.sqlcommand.prepare?view=dotnet-plat-ext-3.1"/>
        /// </remarks>
        private static SqlParameter CreateSqlParameter(string parameterName, SqlDbType dataType, object value)
        {
            // パラメータを生成する。
            SqlParameter parameter = new SqlParameter(parameterName, dataType);
            //
            switch (dataType)
            {
                case SqlDbType.NChar:
                    {
                        // string型に変換する。
                        string sValue = value as string;
                        if (!String.IsNullOrEmpty(sValue))
                        {
                            parameter.Size = sValue.Length;
                        }
                        // nullや長さ0の文字列の場合でも、最低でも1以上のサイズ設定が必要。
                        if (0 == parameter.Size)
                        {
                            parameter.Size = 1;
                        }
                        //
                        break;
                    }
                case SqlDbType.NVarChar:
                    {
                        // string型に変換する。
                        string sValue = value as string;
                        if (!String.IsNullOrEmpty(sValue))
                        {
                            parameter.Size = sValue.Length;
                        }
                        // nullや長さ0の文字列の場合でも、最低でも1以上のサイズ設定が必要。
                        if (0 == parameter.Size)
                        {
                            parameter.Size = 1;
                        }
                        //
                        break;
                    }
                case SqlDbType.Decimal:
                    {
                        // 開発者向けコメント：有効桁数は最大値。小数点以下桁数は値から設定する。
                        // 参考：https://msdn.microsoft.com/ja-jp/library/ms187746(v=sql.120).aspx
                        parameter.Precision = 38;
                        parameter.Scale = (byte)GetScale((decimal)value);
                        break;
                    }
                case SqlDbType.DateTime2:
                    {
                        // 参考：https://stackoverflow.com/questions/29699253/trouble-writing-to-sql
                        // 参考：https://docs.microsoft.com/ja-jp/previous-versions/sql/sql-server-2012/bb677335(v=sql.110)
                        string datetime2format = "YYYY-MM-DD hh:mm:ss.0000000"; // 27桁
                        parameter.Size = datetime2format.Length;
                        break;
                    }
                default:
                    {
                        // fool-proof
                        string message = MessageUtils.GetInvalidEnumArgumentExceptionMessage<SqlDbType>((int)dataType);
                        throw new InvalidEnumArgumentException(message);
                    }
            }
            //
            return parameter;
        }
        /// <summary>
        /// Decimal型の数値の小数部分の桁数を取得する。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <seealso cref="https://docs.microsoft.com/ja-jp/dotnet/api/system.decimal.getbits?view=netcore-3.1"/>
        /// <seealso cref="https://qiita.com/chocolamint/items/80ca5271c6ce1a185430"/>
        private static int GetScale(decimal value)
        {
            int[] bits = Decimal.GetBits(value);
            // 添え字 0 ～ 2 は仮数部領域。添え字 3 の先頭 32 ビット領域が符号と指数部、そして未使用領域。
            int info = bits[3];
            // 下位16ビットは未使用（全部ゼロ）なので捨てる。（符号と指数部を取得）
            int signAndExponent = info >> 16;
            // 下位 8ビットを取得する。（指数部を取得）
            int exponent = signAndExponent & 0x00FF;
            // 結果を戻す。（指数部＝小数部分の桁数）
            return exponent;
        }

        /// <summary>
        /// ［拡張］接続のオープン、クローズのみ管理する。
        /// </summary>
        public void Connect(Action<SqlConnection> action)
        {
            using (var connection = new SqlConnection())
            {
                connection.ConnectionString = this.DataSource.GetConnectionString();
                connection.Open();
                //
                // メイン処理
                try
                {
                    // メイン処理を実行する。
                    action(connection);
                    //
                }
                catch (SqlException)
                {
                    // 例外を投げる。（丸投げ）
                    throw;
                }
                catch (Exception)
                {
                    // 例外を投げる。（丸投げ）
                    throw;
                }
                // 接続を閉じる。
                this.CloseConnection(connection);
            }
        }
        /// <summary>
        /// 接続を閉じる。
        /// </summary>
        /// <param name="connection"></param>
        private void CloseConnection(SqlConnection connection)
        {
            // 接続を閉じる。
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        #region "更新系処理に関する処理"
        public static int ExecuteNonQuery(SqlCommand command, bool enableOptimisticConcurrency = true)
        {
            try
            {
                int count = DbServer<SqlConnection>.ExecuteNonQuery(command, enableOptimisticConcurrency);
                return count;
            }
            catch (SqlException ex)
            {
                switch (ex.Number)
                {
                    case SqlErrorUniqueKeyViolation:
                        {
                            // 一意性インデックス違反
                            string message = DbServer<SqlConnection>.GetDBConcurrencyExceptionMessage(command);
                            // 例外を投げる。
                            var exception = new DBConcurrencyException(message);
                            throw exception;
                        }
                    case SqlErrorUniqueConstraintViolation:
                        {
                            // 一意性制約違反
                            string message = DbServer<SqlConnection>.GetDBConcurrencyExceptionMessage(command);
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

        #region "SQLServer SQL ERROR NO"

        ///＜参考＞
        /// URL: http://msdn.microsoft.com/ja-jp/library/cc645603.aspx
        ///
        ///select m.*
        ///from sys.messages m
        ///  inner join sys.syslanguages l
        ///    on  m.language_id = l.msglangid
        ///    and l.alias = 'Japanese'

        /// <summary>タイムアウト</summary>
        private const int SqlErrorTimeout = -2;

        /// <summary>一意性インデックス違反</summary>
        private const int SqlErrorUniqueKeyViolation = 2601;
        /// <summary>一意性制約違反</summary>
        private const int SqlErrorUniqueConstraintViolation = 2627;

        /// <summary>接続エラー</summary>
        private const int SqlErrorUnableToEstablishTheConnection = 53;

        /// <summary>ログイン失敗</summary>
        private const int SqlErrorLoginFailed = 18456;

        #endregion

    }

}
