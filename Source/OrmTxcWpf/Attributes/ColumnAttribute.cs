using System;

namespace OrmTxcWpf.Attributes {

    /// <summary>
    /// カラム属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ColumnAttribute : Attribute {

        /// <summary>
        /// カラム名を取得します。
        /// </summary>
        public string ColumnName { get; private set; }

        public ColumnAttribute(string name) {
            this.ColumnName = name;
        }

    }

}
