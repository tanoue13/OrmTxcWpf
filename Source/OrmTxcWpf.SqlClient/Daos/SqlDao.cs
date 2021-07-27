using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using OrmTxcWpf.Attributes;
using OrmTxcWpf.Daos;
using OrmTxcWpf.Data;
using OrmTxcWpf.SqlClient.Data;
using OrmTxcWpf.SqlClient.Entities;
using OrmTxcWpf.Utils;

namespace OrmTxcWpf.SqlClient.Daos
{

    /// <summary>
    /// SQL Server用のdao
    /// </summary>
    public abstract class SqlDao<TEntity> : AbstractDao<TEntity, SqlCommand, SqlDataAdapter>
        where TEntity : SqlEntity, new()
    {

        protected override int ExecuteNonQuery(SqlCommand command, TEntity entity, bool enableOptimisticConcurrency = true)
            => SqlServer.ExecuteNonQuery(command, enableOptimisticConcurrency);

        /// <summary>
        /// 新規登録する。（１件）
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns></returns>
        public override int Insert(TEntity entity)
        {
            //
            // コマンドの準備に必要なオブジェクトを生成する。
            SqlCommand command = this.Command;
            //
            // コマンドを準備する。
            this.BuildInsertCommand(command, entity);
            //
            // コマンドを実行する。
            int result = this.ExecuteNonQuery(command, entity);
            //
            // 結果を戻す。
            return result;
        }
        protected virtual void BuildInsertCommand(SqlCommand command, TEntity entity)
        {
            // ディクショナリ（カラム名→プロパティ）を生成する。
            Dictionary<string, PropertyInfo> dictionary = entity.GetColumnAttributes()
                .ToDictionary(prop => prop.GetCustomAttribute<ColumnAttribute>(false).ColumnName);
            // テーブル名を取得する。
            string tableName = entity.GetTableName();
            //
            // コマンドの準備に必要なオブジェクトを生成する。
            var columnStringBuilder = new StringBuilder();
            var valueStringBuilder = new StringBuilder();
            //
            IEnumerator<KeyValuePair<string, PropertyInfo>> pairs = dictionary.GetEnumerator();
            if (pairs.MoveNext())
            {
                // １件目についての処理：
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName;
                    // 項目名を追加する。
                    columnStringBuilder.Append(columnName);
                    // 項目値（SQLパラメータ）を追加する。
                    valueStringBuilder.Append(parameterName);
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // ２件目以降についての処理：
                while (pairs.MoveNext())
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName;
                    // 項目名を追加する。
                    columnStringBuilder.Append(" , ").Append(columnName);
                    // 項目値（SQLパラメータ）を追加する。
                    valueStringBuilder.Append(" , ").Append(parameterName);
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
            }
            //
            // コマンドテキストを生成する。
            var builder = new StringBuilder();
            builder.Append(" insert into ").Append(tableName).Append(" (");
            builder.Append(columnStringBuilder.ToString());
            builder.Append(this.GetCommonFieldForInsert(true));
            builder.Append(" ) values (");
            builder.Append(valueStringBuilder.ToString());
            builder.Append(this.GetCommonFieldForInsertValue(true));
            builder.Append(" )");
            //
            // コマンドテキストを設定する。
            command.CommandText = builder.ToString();
            // データソースにコマンドを準備する。
            command.Prepare();
        }

        /// <summary>
        /// 更新する。（１件）
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns></returns>
        public override int UpdateByPk(TEntity entity)
        {
            //
            // コマンドの準備に必要なオブジェクトを生成する。
            SqlCommand command = this.Command;
            //
            // コマンドを準備する。
            this.BuildUpdateByPkCommand(command, entity);
            //
            // コマンドを実行する。
            int result = this.ExecuteNonQuery(command, entity);
            //
            // 結果を戻す。
            return result;
        }
        protected virtual void BuildUpdateByPkCommand(SqlCommand command, TEntity entity)
        {
            // ディクショナリ（カラム名→プロパティ）を生成する。（主キー属性ありのカラムのみ）
            Dictionary<string, PropertyInfo> dictionary = entity.GetColumnAttributes()
                .Where(prop => prop.GetCustomAttribute<PrimaryKeyAttribute>(false) != null)
                .ToDictionary(prop => prop.GetCustomAttribute<ColumnAttribute>(false).ColumnName);
            //
            // コマンドの準備に必要なオブジェクトを生成する。
            var builder = new StringBuilder();
            //
            IEnumerator<KeyValuePair<string, PropertyInfo>> pairs = dictionary.GetEnumerator();
            if (pairs.MoveNext())
            {
                // １件目についての処理：
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName + "_org";
                    // 項目名を追加する。
                    builder.Append(String.Format(" where x.{0} = {1}", columnName, parameterName));
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // ２件目以降についての処理：
                while (pairs.MoveNext())
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName + "_org";
                    // 項目名を追加する。
                    builder.Append(String.Format(" and x.{0} = {1}", columnName, parameterName));
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // 共通項目についての処理：
                builder.Append(this.GetCommonFieldForUpdateCondition("x", true));
            }
            //
            this.BuildUpdateCommand(command, entity, builder.ToString());
        }
        private void BuildUpdateCommand(SqlCommand command, TEntity entity, string filter)
        {
            // ディクショナリ（カラム名→プロパティ）を生成する。（主キー属性なしのカラムのみ）
            Dictionary<string, PropertyInfo> dictionary = entity.GetColumnAttributes()
                .Where(prop => prop.GetCustomAttribute<PrimaryKeyAttribute>(false) == null)
                .ToDictionary(prop => prop.GetCustomAttribute<ColumnAttribute>(false).ColumnName);
            // テーブル名を取得する。
            string tableName = entity.GetTableName();
            //
            // コマンドの準備に必要なオブジェクトを生成する。
            var columnStringBuilder = new StringBuilder();
            // 更新列とSQLパラメータを設定する。
            IEnumerator<KeyValuePair<string, PropertyInfo>> pairs = dictionary.GetEnumerator();
            if (pairs.MoveNext())
            {
                // １件目についての処理：
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName;
                    // 項目名を追加する。
                    columnStringBuilder.Append(String.Format(" {0} = {1}", columnName, parameterName));
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // ２件目以降についての処理：
                while (pairs.MoveNext())
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName;
                    // 項目名を追加する。
                    columnStringBuilder.Append(String.Format(" , {0} = {1}", columnName, parameterName));
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // 共通項目についての処理：
                columnStringBuilder.Append(this.GetCommonFieldForUpdate(true));
            }
            //
            // コマンドテキストを生成する。
            // 開発者向けコメント：（fool-proof：条件の前に、空白を１つはさむ）
            // OK: update table_name set a = @a, b = @b, c = @c where x = @x
            // NG: update table_name set a = @a, b = @b, c = @cwhere x = @x
            var builder = new StringBuilder();
            builder.Append(" update ").Append(tableName).Append(" as x");
            builder.Append(" set ");
            builder.Append(columnStringBuilder.ToString());
            builder.Append(" "); // fool-proof
            builder.Append(filter);
            //
            // コマンドテキストを設定する。
            command.CommandText = builder.ToString();
            // データソースにコマンドを準備する。
            command.Prepare();
        }

        /// <summary>
        /// 検索する。（１件）
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override TEntity SelectByPk(TEntity entity)
        {
            //
            // コマンドの準備に必要なオブジェクトを生成する。
            SqlCommand command = this.Command;
            //
            // コマンドを準備する。
            this.BuildSelectByPkCommand(command, entity);
            //
            // コマンドを実行する。
            DataTable dt = this.GetDataTable(command);
            //
            // 結果を戻す。
            switch (dt.Rows.Count)
            {
                case 0:
                    {
                        // 検索結果が０件の場合、nullを戻す。
                        return null;
                    }
                case 1:
                    {
                        // 検索結果が１件の場合、オブジェクトに変換する。
                        TEntity result = dt.AsEnumerable()
                            .Select(dataRow => EntityUtils.Create<TEntity>(dataRow))
                            .Single();
                        // 結果を戻す。
                        return result;
                    }
                default:
                    {
                        // fool-proof
                        // 検索結果が２件以上の場合、例外を投げる。
                        var exception = new TooManyRowsException();
                        throw exception;
                    }
            }
        }
        protected virtual void BuildSelectByPkCommand(SqlCommand command, TEntity entity)
        {
            // ディクショナリ（カラム名→プロパティ）を生成する。（主キー属性ありのカラムのみ）
            Dictionary<string, PropertyInfo> dictionary = entity.GetColumnAttributes()
                .Where(prop => prop.GetCustomAttribute<PrimaryKeyAttribute>(false) != null)
                .ToDictionary(prop => prop.GetCustomAttribute<ColumnAttribute>(false).ColumnName);
            //
            // コマンドの準備に必要なオブジェクトを生成する。
            var builder = new StringBuilder();
            //
            IEnumerator<KeyValuePair<string, PropertyInfo>> pairs = dictionary.GetEnumerator();
            if (pairs.MoveNext())
            {
                // １件目についての処理：
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName;
                    // 項目名を追加する。
                    builder.Append(String.Format(" where x.{0} = {1}", columnName, parameterName));
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // ２件目以降についての処理：
                while (pairs.MoveNext())
                {
                    KeyValuePair<string, PropertyInfo> pair = pairs.Current;
                    string columnName = pair.Key;
                    string parameterName = "@" + columnName;
                    // 項目名を追加する。
                    builder.Append(String.Format(" and x.{0} = {1}", columnName, parameterName));
                    // SQLパラメータに登録値を設定する。
                    PropertyInfo propertyInfo = pair.Value;
                    SqlServer.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // 共通項目についての処理：
                builder.Append(this.GetCommonFieldForSelectCondition("x", true));
            }
            //
            this.BuildSelectCommand(command, entity, builder.ToString());
        }
        private void BuildSelectCommand(SqlCommand command, TEntity entity, string filter)
        {
            // テーブル名を取得する。
            string tableName = entity.GetTableName();
            // カラム名を取得する。
            string[] columnNames = entity.GetColumnAttributes()
                .Select(prop => prop.GetCustomAttribute<ColumnAttribute>(false).ColumnName)
                .ToArray();
            //
            // コマンドテキストを生成する。
            // 開発者向けコメント：（fool-proof：条件の前に、空白を１つはさむ）
            // OK: select a, b, c from table_name where x = @x
            // NG: select a, b, c from table_namewhere x = @x
            var builder = new StringBuilder();
            builder.Append(" select");
            builder.Append(String.Join(",", columnNames.Select(columnName => String.Format(" x.{0}", columnName))));
            builder.Append(this.GetCommonFieldForSelect("x"));
            builder.Append(" from ").Append(tableName).Append(" as x");
            builder.Append(" "); // fool-proof
            builder.Append(filter);
            //
            // コマンドテキストを設定する。
            command.CommandText = builder.ToString();
            // データソースにコマンドを準備する。
            command.Prepare();
        }

    }

}
