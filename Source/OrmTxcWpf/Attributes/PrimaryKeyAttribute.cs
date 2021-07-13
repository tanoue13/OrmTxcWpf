using System;

namespace OrmTxcWpf.Attributes {

    /// <summary>
    /// 主キー属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PrimaryKeyAttribute : Attribute {

        public PrimaryKeyAttribute() {
        }

    }

}
