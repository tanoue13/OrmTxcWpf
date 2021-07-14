using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OrmTxcWpf.Attributes;
using OrmTxcWpf.Entities;
using OrmTxcWpf.Utils;

namespace OrmTxcWpf.Db2.Entities
{

    /// <summary>
    /// NOK ERPシステム（AS/400）の共通項目を保持するエンティティクラス。
    /// </summary>
    public abstract class DB2Entity : AbstractEntity
    {

        /// <summary>
        /// テーブル内の相対行番号（Relative Record Number）を取得する際のカラム名です。
        /// </summary>
        internal static string RRNColumnName
        {
            get => DB2Entity._rrnColumnName;
        }
        private readonly static string _rrnColumnName = "relative_record_number";

        /// <summary>
        /// テーブル内の相対行番号（Relative Record Number）を取得または設定します。
        /// </summary>
        public int? RelativeRecordNumber
        {
            get => this._relativeRecordNumber;
            set => this.SetProperty(ref this._relativeRecordNumber, value);
        }
        private int? _relativeRecordNumber;

        public override void SetProperties(DataRow dataRow)
        {
            // 基底クラスの処理を置き換える。
            // base.SetProperties(dataRow);
            //
            // DataTable.Columnsを取得する。
            DataColumnCollection dataColumnCollection = dataRow.Table.Columns;
            // DataTable.Columnsに存在するカラム属性のみを取得する。（共通項目を含む）
            IEnumerable<PropertyInfo> properties = this.GetColumnAttributes()
                .Concat(EntityUtils.GetColumnAttributes<DB2Entity>())
                .Where(prop => dataColumnCollection.Contains(prop.GetCustomAttribute<ColumnAttribute>(false).ColumnName));
            //
            // プロパティを設定する。
            foreach (PropertyInfo property in properties)
            {
                this.SetProperty(dataRow, property);
            }
            //
            // テーブル内の相対行番号を設定する。
            if (dataRow.Table.Columns.Contains(RRNColumnName))
            {
                this.RelativeRecordNumber = (int?)(dataRow[RRNColumnName] as decimal?);
            }
        }
        protected override void SetProperty(DataRow dataRow, PropertyInfo propertyInfo)
        {
            // 基底クラスの処理を置き換える。
            // base.SetProperty(dataRow, propertyInfo);
            //
            // dataRowから値を取得する。
            object value = this.GetValue(dataRow, propertyInfo);
            // 値を設定する。
            propertyInfo.SetValue(this, value);
        }
        private object GetValue(DataRow dataRow, PropertyInfo propertyInfo)
        {
            string columnName = propertyInfo.GetCustomAttribute<ColumnAttribute>(false).ColumnName;
            // DBNullの場合、nullを設定する。
            if (dataRow.IsNull(columnName))
            {
                return null;
            }
            //
            object value = dataRow[columnName];
            //
            // <例外処理>
            if (value is decimal decValue)
            {
                // プロパティの型にあわせて変換する。
                Type type = propertyInfo.PropertyType;
                if (new Type[] { typeof(int), typeof(int?) }.Contains(type))
                {
                    return (int)decValue;
                }
                else
                {
                    return decValue;
                }
            }
            // </例外処理>
            //
            // fool-proof
            return value;
        }

        #region"BaseDB2Entity用のプロパティ（定数）"

        /// <summary>
        /// レコード状態
        /// </summary>
        public class Xxxrjt
        {
            /// <summary>
            /// 有効行を表す値を取得する。
            /// </summary>
            public static readonly string Active = "A";
            [Obsolete("Xxxrjtフィールドの値の比較には、Activeを使用してください。", true)]
            public static readonly string Dead = "D";
        }

        /// <summary>
        /// 廃却サイン
        /// </summary>
        public class Xxxded
        {
            [Obsolete("Xxxdedフィールドの値の比較には、Deadを使用してください。", true)]
            public static readonly string Active = " ";
            /// <summary>
            /// 無効行（廃却済み）を表す値を取得する。
            /// </summary>
            public static readonly string Dead = "D";
        }

        #endregion

    }

}
