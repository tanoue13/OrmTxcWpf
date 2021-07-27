using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OrmTxcWpf.Utils;

namespace OrmTxcWpf.Entities
{

    /// <summary>
    /// AbstractEntityの拡張メソッドを定義します。（静的クラス）
    /// </summary>
    internal static class AbstractEntityExtensions
    {

        /// <summary>
        /// エンティティが等価かどうかを判定する。
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns></returns>
        /// <remarks>
        /// このメソッドにおける「等価」とは、エンティティのメタ情報（DBにおける共通項目）以外の値が等しいという意味である。
        /// つまり、「等価」なエンティティでUpdateByPk(E)を実行した場合、共通項目以外が更新されない。
        /// </remarks>
        internal static bool IsEquivalentTo<TEntity>(this TEntity obj1, TEntity obj2) where TEntity : AbstractEntity
        {
            PropertyInfo[] properties = EntityUtils.GetColumnAttributes<TEntity>().ToArray();
            // 比較対象の属性値を取得する。
            IEnumerable<object> values1 = properties.Select(prop => prop.GetValue(obj1));
            IEnumerable<object> values2 = properties.Select(prop => prop.GetValue(obj2));
            // 比較する。
            bool equivalent = Enumerable.SequenceEqual(values1, values2, new AttributeEqualityComparer());
            // 結果を戻す。
            return equivalent;
        }

        /// <summary>
        /// 属性の等価比較を行う IEqualityComparer 。
        /// </summary>
        private class AttributeEqualityComparer : IEqualityComparer<object>
        {
            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                if ((x == null) && (y == null))
                {
                    // どちらも null の場合、「等価」と判定する。
                    return true;
                }
                else if ((x == null) && (y != null))
                {
                    // 「等価」でないと判定する。
                    return false;
                }
                else if ((x != null) && (y == null))
                {
                    // 「等価」でないと判定する。
                    return false;
                }
                else
                {
                    // どちらも null でない場合、比較結果を戻す。
                    return x.Equals(y);
                }
            }
            int IEqualityComparer<object>.GetHashCode(object obj)
            {
                return obj.GetHashCode();
            }
        }

    }

}
