using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using OrmTxcWpf.Sql.Daos;

namespace OrmTxcWpf.Sql.Data
{

    public abstract class DbServer<TConnection> where TConnection : IDbConnection, new()
    {

        /// <summary>
        /// データソースを取得または設定します。
        /// </summary>
        public virtual DataSource DataSource { protected get; set; } = new ConnectionStringDataSource();

        public void Execute(IDao dao, Action action)
        {
            this.Execute(new IDao[] { dao }, action);
        }
        public void Execute(IEnumerable<IDao> daos, Action action)
        {
            this.Execute(daos, tx => { action(); });
        }

        public void Execute(IDao dao, Action<IDbTransaction> action)
        {
            this.Execute(new IDao[] { dao }, action);
        }
        public virtual void Execute(IEnumerable<IDao> daos, Action<IDbTransaction> action)
        {
            using (var connection = new TConnection())
            {
                connection.ConnectionString = this.DataSource.GetConnectionString();
                connection.Open();
                using (var tx = connection.BeginTransaction())
                {
                    // 前処理：コマンドに接続とトランザクションを設定する。
                    foreach (IDao dao in daos)
                    {
                        IEnumerable<IDbCommand> commands = dao.Commands;
                        if (commands != null)
                        {
                            foreach (IDbCommand command in commands)
                            {
                                // 接続を設定する。
                                command.Connection = connection;
                                // トランザクションを設定する。
                                command.Transaction = tx;
                            }
                        }
                    }
                    //
                    // メイン処理：実装クラスのexecute()を実行する。
                    try
                    {
                        // メイン処理を実行する。
                        action(tx);
                        //
                        // トランザクションをコミットする。
                        this.Commit(tx);
                    }
                    catch (DbException ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine(ex.StackTrace);
                        // トランザクションをロールバックする。
                        this.Rollback(tx);
                        //
                        // 例外を投げる。（丸投げ）
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine(ex.StackTrace);
                        // トランザクションをロールバックする。
                        this.Rollback(tx);
                        //
                        // 例外を投げる。（丸投げ）
                        throw;
                    }
                }
                // 
                // 接続を閉じる。
                this.CloseConnection(connection);
            }
        }

        /// <summary>
        /// トランザクションをコミットする。
        /// </summary>
        /// <param name="tx"></param>
        private void Commit(IDbTransaction tx)
        {
            try
            {
                // トランザクションをコミットする。
                tx.Commit();
            }
            catch (InvalidOperationException ex)
            {
                // トランザクションは、既にコミットまたはロールバックされています。
                // または、接続が切断されています。
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
            catch (Exception ex)
            {
                // トランザクションのコミット中にエラーが発生しました。
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
        /// <summary>
        /// トランザクションをロールバックする。
        /// </summary>
        /// <param name="tx"></param>
        private void Rollback(IDbTransaction tx)
        {
            try
            {
                // トランザクションをロールバックする。
                tx.Rollback();
            }
            catch (InvalidOperationException ex)
            {
                // トランザクションは、既にコミットまたはロールバックされています。
                // または、接続が切断されています。
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
            catch (Exception ex)
            {
                // トランザクションのロールバック中にエラーが発生しました。
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }
        /// <summary>
        /// 接続を閉じる。
        /// </summary>
        /// <param name="connection"></param>
        private void CloseConnection(IDbConnection connection)
        {
            // 接続を閉じる。
            if (connection.State == ConnectionState.Open)
            {
                connection.Close();
            }
        }

        /// <summary>
        /// トランザクションを設定する。
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="dao"></param>
        public void SetTransaction(IDbTransaction tx, IDao dao)
        {
            IEnumerable<IDbCommand> commands = dao.Commands;
            if (commands != null)
            {
                foreach (IDbCommand command in commands)
                {
                    command.Connection = tx.Connection;
                    command.Transaction = tx;
                }
            }
        }

        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換える。
        /// </summary>
        /// <param name="command">command</param>
        /// <param name="parameter">parameter</param>
        /// <remarks></remarks>
        protected static void AddParameterOrReplace(IDbCommand command, IDataParameter parameter)
        {
            DbServer<TConnection>.AddParameter(command, parameter, true);
        }
        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、置き換えない。
        /// </summary>
        /// <param name="command">command</param>
        /// <param name="parameter">parameter</param>
        /// <remarks></remarks>
        protected static void AddParameterIfNotExists(IDbCommand command, IDataParameter parameter)
        {
            DbServer<TConnection>.AddParameter(command, parameter, false);
        }
        /// <summary>
        /// コマンドにパラメータを追加する。パラメータが存在する場合、overwriteIfExistsに従い処理する。
        /// </summary>
        /// <param name="command">command</param>
        /// <param name="parameter">parameter</param>
        /// <remarks></remarks>
        private static void AddParameter(IDbCommand command, IDataParameter parameter, bool overwriteIfExists = true)
        {
            string parameterName = parameter.ParameterName;
            if (command.Parameters.Contains(parameterName))
            {
                // 存在する場合、かつ、上書き可能の場合、パラメータを上書きする。
                if (overwriteIfExists)
                {
                    // 開発者向けコメント（2021.01.28田上）：パラメータ値の上書きに関するロジックを見直し。
                    // （廃止ロジック）既存のIDataParameterオブジェクトをコレクションから削除し、新しいIDataParameterオブジェクトをコレクションに追加する。
                    //command.Parameters.RemoveAt(parameterName);
                    //command.Parameters.Add(parameter);
                    //
                    //
                    IDataParameter dataParameter = command.Parameters[parameterName] as IDataParameter;
                    dataParameter.Value = parameter.Value;
                    // 開発者向けコメント（2021.01.28田上）：
                    // ・かつては、既存のIDataParameterオブジェクトをコレクションから削除し、新しいIDataParameterオブジェクトをコレクションに追加していた。
                    // ・しかし、この方法では、IDbCommand.Prepare()を呼び出した後にパラメータを上書き（オブジェクトを置換）した場合に、
                    // 　サーバ側に新しいオブジェクトの値が反映されずにSQLが実行されることが判明した。反映するためには、置換後に再度IDbCommand.Prepare()を呼び出す必要がある。
                    // ・この挙動により、予期せぬ不具合を引き起こす可能性がある。DbServer内でSQLパラメータの値を上書きしても、IDbCommand.Prepare()の呼び出しが実施されなければ反映されないため。
                    // 　- 例１：共通項目で使用するシステムID、プログラムIDなどの値
                    // 　- 例２：UPDATE文を実行する際のバージョンNo.（既存レコードのバージョンNo.フィールドの値を設定する）
                    // ・したがって、パラメータ値の上書きは、既存のIDataParameterオブジェクトを取得し、そのオブジェクトの値を上書きするように変更した。
                }
            }
            else
            {
                // 存在しない場合、パラメータを追加する。
                command.Parameters.Add(parameter);
            }
        }

        #region "更新系処理に関する処理"
        protected static int ExecuteNonQuery(IDbCommand command, bool enableOptimisticConcurrency = true)
        {
            try
            {
                // TODO: ログを出力する。
                // LogUtils.LogSql(command);
                //
                // SQLを実行する。
                int count = command.ExecuteNonQuery();
                // 楽観的同時実行排他制御が有効、かつ、更新件数が０件の場合、例外を投げる。
                if (enableOptimisticConcurrency && (count == 0))
                {
                    string message = DbServer<TConnection>.GetDBConcurrencyExceptionMessage(command, count);
                    // 例外を投げる。
                    var exception = new DBConcurrencyException(message);
                    throw exception;
                }
                // 結果を戻す。
                return count;
            }
            catch (DbException ex)
            {
                // TODO: ログを出力する。
                // LogUtils.GetErrorLogger().Error(ex);
                //
                // 例外を投げる。（丸投げ）
                throw;
            }
        }
        protected static string GetDBConcurrencyExceptionMessage(IDbCommand command, int? numberOfRowsAffected = null)
        {
            string commandText = command.CommandText;
            string parameters = DbServer<TConnection>.GetParametersString(command.Parameters);
            var builder = new StringBuilder();
            builder.Append("同時実行排他制御エラーが発生しました。");
            builder.Append(" SQL: ").Append(commandText).Append(" ;");
            builder.Append(" PARAMETERS: ").Append(parameters);
            if (numberOfRowsAffected.HasValue)
            {
                builder.Append(" ;");
                builder.Append(" The number of rows affected: ").Append(numberOfRowsAffected);
            }
            // 結果を戻す。
            return builder.ToString();
        }
        #endregion

        #region "便利機能"
        /// <summary>
        /// SQLパラメータの文字列を戻す。
        /// </summary>
        /// <param name="parameterCollection">SQLパラメータのコレクション</param>
        /// <param name="delimiter">区切り文字</param>
        /// <returns>SQLパラメータの文字列</returns>
        /// <remarks>
        /// １行に全SQLパラメータを出力する場合、delimiterの指定は不要。デフォルトの区切り文字", "を使用した文字列を戻します。
        /// １行に１パラメータずつ出力したい場合は、区切り文字に改行文字を設定してください。
        /// </remarks>
        public static string GetParametersString(IDataParameterCollection parameterCollection, string delimiter = ", ")
        {
            //
            // パラメータ文字列
            var builder = new StringBuilder();
            //
            IEnumerator<IDataParameter> enumerator = (IEnumerator<IDataParameter>)parameterCollection.GetEnumerator();
            if (enumerator.MoveNext())
            {
                // １件目の処理
                {
                    IDataParameter parameter = enumerator.Current;
                    // パラメータ名、値を追加する。
                    builder.Append(DbServer<TConnection>.GetParameterNameValue(parameter));
                }
                // ２件目以降の処理
                while (enumerator.MoveNext())
                {
                    IDataParameter parameter = enumerator.Current;
                    // デリミタ、パラメータ名、値を追加する。
                    builder.Append(delimiter);
                    builder.Append(DbServer<TConnection>.GetParameterNameValue(parameter));
                }
            }
            // 結果を戻す。
            return builder.ToString();
        }
        private static string GetParameterNameValue(IDataParameter parameter)
        {
            string name = parameter.ParameterName;
            object value = parameter.Value;
            string nameValue = String.Format("{0}={1}", new object[] { name, value });
            return nameValue;
        }
        #endregion

    }

}
