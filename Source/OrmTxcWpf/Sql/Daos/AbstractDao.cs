using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using OrmTxcWpf.Entities;

namespace OrmTxcWpf.Sql.Daos
{

    public abstract class AbstractDao<TEntity, TDbCommand, TDbDataAdapter> : IDao
        where TEntity : AbstractEntity, new()
        where TDbCommand : DbCommand, new()
        where TDbDataAdapter : DbDataAdapter, new()
    {

        IEnumerable<IDbCommand> IDao.Commands { get => this.CommandList; }
        protected readonly IList<IDbCommand> CommandList = new List<IDbCommand>();

        protected TDbCommand Command { get; set; } = new TDbCommand();

        /// <summary>
        /// 文字列型のフィールドについて、末尾をトリムするかどうかを取得または設定します。
        /// </summary>
        public bool TrimEnd { protected get; set; } = true;

        /// <summary>
        /// コンストラクタ.
        /// </summary>
        public AbstractDao()
        {
            this.CommandList.Add(this.Command);
        }

        /// <summary>
        /// 新規登録する。（１件）
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns></returns>
        public abstract int Insert(TEntity entity);

        /// <summary>
        /// 更新する。（１件）
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns></returns>
        public abstract int UpdateByPk(TEntity entity);

        /// <summary>
        /// 検索する。（１件）
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public abstract TEntity SelectByPk(TEntity entity);

        /// <summary>
        /// 検索する。（複数件）
        /// </summary>
        /// <param name="entity">検索条件</param>
        /// <returns></returns>
        public abstract TEntity[] Select(TEntity entity);

        /// <summary>
        /// SELECT文実行時の共通処理。
        /// </summary>
        /// <param name="command">command</param>
        /// <returns></returns>
        protected DataTable GetDataTable(TDbCommand command)
        {
            // TODO: ログを出力する。
            //LogUtils.LogSql(command);
            //
            // コマンドを実行する。
            DataTable dt = new DataTable();
            using (var adapter = new TDbDataAdapter())
            {
                adapter.SelectCommand = command;
                adapter.Fill(dt);
            }
            //
            // 末尾をトリムする場合、トリムする。
            if (this.TrimEnd)
            {
                // カラムを順番に処理し、文字列型の場合はトリムする。
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    if (typeof(string).Equals(dataColumn.DataType))
                    {
                        foreach (DataRow dataRow in dt.Rows)
                        {
                            int ordinal = dataColumn.Ordinal;
                            if (!dataRow.IsNull(ordinal))
                            {
                                string value = dataRow[ordinal] as string;
                                dataRow[ordinal] = value.TrimEnd();
                            }
                        }
                    }
                }
            }
            // 内部処理での変更内容をコミットする。
            dt.AcceptChanges();
            //
            // 結果を戻す。
            return dt;
        }

        /// <summary>
        /// レコードの存在有無を確認し、INSERT文、または、UPDATE文を実行する。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public int InsertOrUpdate(TEntity entity)
        {
            TEntity result = this.SelectByPk(entity);
            if (result == null)
            {
                if (entity.IsEquivalentTo(new TEntity()))
                {
                    // 引数のエンティティが初期状態と等価な場合、INSERT文を実行しない。
                    return 0;
                }
                else
                {
                    // INSERT文を実行する。
                    return this.Insert(entity);
                }
            }
            else
            {
                if (entity.IsEquivalentTo(result))
                {
                    // 引数のエンティティが検索結果と等価な場合、UPDATE文を実行しない。
                    return 0;
                }
                else
                {
                    // UPDATE文を実行する。
                    return this.UpdateByPk(entity);
                }
            }
        }

        /// <summary>
        /// 更新系SQL（INSERT, UPDATE, DELETE）を実行し、影響を受ける行の数を戻します。
        /// </summary>
        /// <param name="command"></param>
        /// <param name="entity"></param>
        /// <param name="enableOptimisticConcurrency">楽観的排他制御を有効にする場合、true</param>
        /// <returns>影響を受けた行の数</returns>
        protected abstract int ExecuteNonQuery(TDbCommand command, TEntity entity, bool enableOptimisticConcurrency = true);

        #region"BaseEntity（共通項目）に関するプロパティや処理（メソッド名前を共通化するため、抽象メソッドとして定義）"

        /// <summary>
        /// SELECT文用の共通項目文字列を戻す。
        /// </summary>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        protected string GetCommonFieldForSelect(bool appendDelimiter = true)
        {
            return this.GetCommonFieldForSelect(String.Empty, appendDelimiter);
        }
        /// <summary>
        /// SELECT文用の共通項目文字列を戻す。
        /// </summary>
        /// <param name="tableAlias">テーブル別名</param>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        /// <remarks>
        /// 共通項目の前の区切り文字（, ）が必要な場合、引数（appendDelimiter）は設定不要です。
        /// 共通項目の前に区切り文字（, ）が不要な場合、引数（appendDelimiter）にfalseを設定してください。
        /// <example>
        /// 使用方法：
        /// <code>
        /// SELECT
        ///     field1
        ///   , field2
        ///   , field3
        ///   BaseDao.GetCommonFieldForSelect()
        /// WHERE
        ///   key_field1 = @key_value1
        ///   AND key_field2 = @key_value2
        ///   BaseDao.GetCommonFieldForSelectCondition()
        /// </code>
        /// </example>
        /// </remarks>
        protected virtual string GetCommonFieldForSelect(string tableAlias, bool appendDelimiter = true)
        {
            return String.Empty;
        }

        /// <summary>
        /// SELECT文用のWHERE句文字列を戻す。
        /// </summary>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        /// <remarks>
        /// 共通項目の前の区切り文字（ AND ）が必要な場合、引数（appendDelimiter）は設定不要です。
        /// 共通項目の前に区切り文字（ AND ）が不要な場合、引数（appendDelimiter）にfalseを設定してください。
        /// </remarks>
        protected string GetCommonFieldForSelectCondition(bool appendDelimiter = true)
        {
            return this.GetCommonFieldForSelectCondition(String.Empty, appendDelimiter);
        }
        /// <summary>
        /// SELECT文用のWHERE句文字列を戻す。
        /// </summary>
        /// <param name="tableAlias">テーブル別名</param>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        /// <remarks>
        /// 共通項目の前の区切り文字（ AND ）が必要な場合、引数（appendDelimiter）は設定不要です。
        /// 共通項目の前に区切り文字（ AND ）が不要な場合、引数（appendDelimiter）にfalseを設定してください。
        /// </remarks>
        protected virtual string GetCommonFieldForSelectCondition(string tableAlias, bool appendDelimiter = true)
        {
            return String.Empty;
        }

        /// <summary>
        /// INSERT文用の共通項目文字列を戻す。
        /// </summary>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        /// <remarks>
        /// 共通項目の前の区切り文字（, ）が必要な場合、引数（appendDelimiter）は設定不要です。
        /// 共通項目の前に区切り文字（, ）が不要な場合、引数（appendDelimiter）にfalseを設定してください。
        /// <example>
        /// 使用方法：
        /// <code>
        /// INSERT INTO table_name (
        ///     field1
        ///   , field2
        ///   , field3
        ///   BaseDao.GetCommonItemForInsert()
        /// ) VALUES (
        ///     @value1
        ///   , @value2
        ///   , @value3
        ///   BaseDao.GetCommonItemForInsertValue()
        /// )
        /// </code>
        /// </example>
        /// </remarks>
        protected virtual string GetCommonFieldForInsert(bool appendDelimiter = true)
        {
            return String.Empty;
        }
        /// <summary>
        /// INSERT文用のVALUE句文字列を戻す。
        /// </summary>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        /// <remarks>
        /// 共通項目の前の区切り文字（, ）が必要な場合、引数（appendDelimiter）は設定不要です。
        /// 共通項目の前に区切り文字（, ）が不要な場合、引数（appendDelimiter）にfalseを設定してください。
        /// </remarks>
        protected virtual string GetCommonFieldForInsertValue(bool appendDelimiter = true)
        {
            return String.Empty;
        }

        /// <summary>
        /// UPDATE文用の共通項目文字列を戻す。
        /// </summary>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        /// <remarks>
        /// 共通項目の前の区切り文字（, ）が必要な場合、引数（appendDelimiter）は設定不要です。
        /// 共通項目の前に区切り文字（, ）が不要な場合、引数（appendDelimiter）にfalseを設定してください。
        /// <example>
        /// 使用方法：
        /// <code>
        /// UPDATE table_name 
        /// SET
        ///     field1 = @value1
        ///   , field2 = @value2
        ///   , field3 = @value3
        ///   BaseDao.GetCommonItemForUpdate()
        /// WHERE
        ///   key_field1 = @key_value1
        ///   AND key_field2 = @key_value2
        ///   BaseDao.GetCommonItemForUpdateCondition()
        /// </code>
        /// </example>
        /// </remarks>
        protected virtual string GetCommonFieldForUpdate(bool appendDelimiter = true)
        {
            return String.Empty;
        }
        /// <summary>
        /// UPDATE文用のWHERE句文字列を戻す。
        /// </summary>
        /// <param name="appendDelimiter">区切り文字の付加</param>
        /// <returns></returns>
        /// <remarks>
        /// 共通項目の前の区切り文字（ AND ）が必要な場合、引数（appendDelimiter）は設定不要です。
        /// 共通項目の前に区切り文字（ AND ）が不要な場合、引数（appendDelimiter）にfalseを設定してください。
        /// </remarks>
        protected string GetCommonFieldForUpdateCondition(bool appendDelimiter = true)
        {
            return this.GetCommonFieldForUpdateCondition(String.Empty, appendDelimiter);
        }
        protected virtual string GetCommonFieldForUpdateCondition(string tableAlias, bool appendDelimiter = true)
        {
            return String.Empty;
        }

        #endregion

    }

}
