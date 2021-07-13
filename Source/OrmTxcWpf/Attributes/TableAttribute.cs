using System;

namespace OrmTxcWpf.Attributes
{

    /// <summary>
    /// テーブル属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {

        /// <summary>
        /// テーブル名を取得します。
        /// </summary>
        public string TableName { get; private set; }

        public TableAttribute(string name)
        {
            this.TableName = name;
        }

    }

}
