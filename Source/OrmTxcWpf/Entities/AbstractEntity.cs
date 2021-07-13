using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OrmTxcWpf.Attributes;
using OrmTxcWpf.DataModels;
using OrmTxcWpf.Utils;

namespace OrmTxcWpf.Entities
{

    /// <summary>
    /// Entityの基底クラス。
    /// </summary>
    public abstract class AbstractEntity : BindableDataModel
    {

        /// <summary>
        /// <paramref name="dataRow"/>から値を取得し、プロパティを設定する。
        /// </summary>
        /// <param name="dataRow"></param>
        public virtual void SetProperties(DataRow dataRow)
        {
            // DataTable.Columnsを取得する。
            DataColumnCollection dataColumnCollection = dataRow.Table.Columns;
            // DataTable.Columnsに存在するカラム属性のみを取得する。
            IEnumerable<PropertyInfo> properties = this.GetColumnAttributes()
                .Where(prop => dataColumnCollection.Contains(prop.GetCustomAttribute<ColumnAttribute>(false).ColumnName));
            //
            // プロパティを設定する。
            foreach (PropertyInfo property in properties)
            {
                this.SetProperty(dataRow, property);
            }
        }
        protected virtual void SetProperty(DataRow dataRow, PropertyInfo propertyInfo)
        {
            string columnName = propertyInfo.GetCustomAttribute<ColumnAttribute>(false).ColumnName;
            if (dataRow.IsNull(columnName))
            {
                // DBNullの場合、nullを設定する。
                propertyInfo.SetValue(this, null);
            }
            else
            {
                object value = dataRow[columnName];
                propertyInfo.SetValue(this, value);
            }
        }

    }

}
