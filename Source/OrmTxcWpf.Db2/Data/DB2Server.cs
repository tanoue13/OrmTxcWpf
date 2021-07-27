using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using IBM.Data.Db2;
using OrmTxcWpf.Data;

namespace OrmTxcWpf.Db2.Data
{

    public class DB2Server : DbServer<DB2Connection>
    {

        private static IParameterValueConverter ParameterValueConverter { get; set; } = new DB2ParameterValueConverter();

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
            DB2Type dbType = DB2Type.VarChar; // default
            if (new Type[] { typeof(decimal), typeof(decimal?) }.Contains(propertyType))
            {
                dbType = DB2Type.Numeric;
            }
            else if (new Type[] { typeof(int), typeof(int?) }.Contains(propertyType))
            {
                dbType = DB2Type.Numeric;
            }
            else
            {
                // fool-proof
                dbType = DB2Type.VarChar;
            }
            // パラメータに設定する値を取得する。
            object value = property.GetValue(obj);
            //
            // nullを考慮し、下のメソッド経由で設定する。
            DB2Server.AddParameterOrReplace(command, parameterName, dbType, value);
        }
        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換える。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="value"></param>
        /// <remarks></remarks>
        public static void AddParameterOrReplace(IDbCommand command, string parameterName, DB2Type dbType, object value)
        {
            IDataParameter parameter = new DB2Parameter(parameterName, dbType);
            parameter.Value = DB2Server.ParameterValueConverter.Convert(value, dbType.GetType(), dbType);
            DbServer<DB2Connection>.AddParameterOrReplace(command, parameter);
        }

        /// <summary>
        /// ［拡張］接続のオープン、クローズのみ管理する。
        /// </summary>
        public void Connect(Action<DB2Connection> action)
        {
            using (var connection = new DB2Connection())
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
                catch (DbException ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
                    // 例外を投げる。（丸投げ）
                    throw;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Debug.WriteLine(ex.StackTrace);
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
        private void CloseConnection(DB2Connection connection)
        {
            // 接続を閉じる。
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        #region "更新系処理に関する処理"
        public static int ExecuteNonQuery(DB2Command command, bool enableOptimisticConcurrency = true)
        {
            try
            {
                int count = DbServer<DB2Connection>.ExecuteNonQuery(command, enableOptimisticConcurrency);
                return count;
            }
            catch (DB2Exception ex)
            {
                switch (ex.ErrorCode)
                {
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
