using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using IBM.Data.Db2;
using OrmTxcWpf.Attributes;
using OrmTxcWpf.Db2.Entities;
using OrmTxcWpf.Db2.Sql.Data;
using OrmTxcWpf.Sql.Daos;
using OrmTxcWpf.Sql.Data;
using OrmTxcWpf.Utils;

namespace OrmTxcWpf.Db2.Sql.Daos
{

    /// <summary>
    /// Daoの基底クラス。BaseEntityのサブクラスに対してInsert, UpdateByPk, FindByPkを実装済み。
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <remarks>
    /// 同時実行排他制御による更新時の異常等はDB2Server#ExecuteNonQuery()で処理し、
    /// このクラスでは例外処理等は実施しない。
    /// </remarks>
    public abstract class DB2Dao<TEntity> : AbstractDao<TEntity, DB2Command, DB2DataAdapter>
        where TEntity : DB2Entity, new()
    {

        protected override int ExecuteNonQuery(DB2Command command, TEntity entity, bool enableOptimisticConcurrency = true)
        {
            //
            // コマンドを実行する。
            int count = DB2Server.ExecuteNonQuery(command, enableOptimisticConcurrency);
            //
            // 結果を戻す。
            return count;
        }

        /// <summary>
        /// 新規登録する。（１件）
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns></returns>
        public override int Insert(TEntity entity)
        {
            //
            // コマンドの準備に必要なオブジェクトを生成する。
            DB2Command command = this.Command;
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
        protected virtual void BuildInsertCommand(DB2Command command, TEntity entity)
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
                    DB2Server.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
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
                    DB2Server.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
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
            DB2Command command = this.Command;
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
        protected virtual void BuildUpdateByPkCommand(DB2Command command, TEntity entity)
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
                    DB2Server.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
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
                    DB2Server.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // 共通項目についての処理：
                builder.Append(this.GetCommonFieldForUpdateCondition("x", true));
            }
            //
            this.BuildUpdateCommand(command, entity, builder.ToString());
        }
        private void BuildUpdateCommand(DB2Command command, TEntity entity, string filter)
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
                    DB2Server.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
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
                    DB2Server.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
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
            DB2Command command = this.Command;
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
        protected virtual void BuildSelectByPkCommand(DB2Command command, TEntity entity)
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
                    DB2Server.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
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
                    DB2Server.AddParameterOrReplace(command, parameterName, entity, propertyInfo);
                }
                // 共通項目についての処理：
                builder.Append(this.GetCommonFieldForSelectCondition("x", true));
            }
            //
            this.BuildSelectCommand(command, entity, builder.ToString());
        }
        private void BuildSelectCommand(DB2Command command, TEntity entity, string filter)
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
            builder.Append(this.GetCommonFieldForSelect("x", false));
            builder.Append(columnNames.Aggregate(new StringBuilder(), (sb, next) => sb.Append(String.Format(" , x.{0}", next)), sb => sb.ToString()));
            builder.Append(" from ").Append(tableName).Append(" as x");
            builder.Append(" "); // fool-proof
            builder.Append(filter);
            //
            // コマンドテキストを設定する。
            command.CommandText = builder.ToString();
            // データソースにコマンドを準備する。
            command.Prepare();
        }


        #region"BaseEntity（共通項目）に関するプロパティや処理"

        /// <summary>
        /// テーブル内の相対行番号（Relative Record Number）を取得する際のカラム名です。
        /// </summary>
        protected static string RRNColumnName
        {
            get => DB2Entity.RRNColumnName;
        }

        protected override string GetCommonFieldForSelect(string tableAlias, bool appendDelimiter = true)
        {
            var builder = new StringBuilder();
            //
            // 開発者向けコメント（2021.04.14田上）：
            // ・DB2のRRN関数（相対行番号）の引数には、テーブル別名を指定する必要がある。
            // ・ドキュメント上はテーブル名でも可と読み取れるが、実際に指定してみるとエラーとなった。
            // ・参考：https://www.ibm.com/docs/ja/i/7.4?topic=functions-rrn
            // ・そのため、テーブル別名が指定されている場合のみ相対行番号を取得するようにしている。
            //
            if (!String.IsNullOrWhiteSpace(tableAlias))
            {
                if (appendDelimiter)
                {
                    builder.Append(" ,");
                }
                builder.Append(" ");
                // 相対行番号を追加する。
                builder.Append($"rrn( {tableAlias} ) as {RRNColumnName}");
            }
            // 結果を戻す。
            return builder.ToString();
        }

        #endregion

    }

}
