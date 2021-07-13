using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using OrmTxcWpf.Attributes;
using OrmTxcWpf.Entities;

namespace OrmTxcWpf.Utils
{

    /// <summary>
    /// Entityに関するユーティリティクラス。（AbstractEntityの拡張メソッドを含む）
    /// </summary>
    public static class EntityUtils
    {

        /// <summary>
        /// テーブル名を取得する。
        /// </summary>
        /// <param name="abstractEntity"></param>
        /// <returns></returns>
        public static string GetTableName(this AbstractEntity abstractEntity)
        {
            Type type = abstractEntity.GetType();
            return EntityUtils.GetTableName(type);
        }
        /// <summary>
        /// テーブル名を取得する。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static string GetTableName<TEntity>() where TEntity : AbstractEntity
        {
            Type type = typeof(TEntity);
            return EntityUtils.GetTableName(type);
        }
        /// <summary>
        /// テーブル名を取得する。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string GetTableName(Type type)
        {
            // テーブル名を取得する。
            string tableName = type.GetCustomAttribute<TableAttribute>(false).TableName;
            // 結果を戻す。
            return tableName;
        }

        /// <summary>
        /// カラム属性を取得する。
        /// </summary>
        /// <param name="abstractEntity"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetColumnAttributes(this AbstractEntity abstractEntity)
        {
            Type type = abstractEntity.GetType();
            return EntityUtils.GetColumnAttributes(type);
        }
        /// <summary>
        /// カラム属性を取得する。
        /// </summary>
        /// <param name="abstractEntity"></param>
        /// <returns></returns>
        public static IEnumerable<PropertyInfo> GetColumnAttributes<TEntity>() where TEntity : AbstractEntity
        {
            Type type = typeof(TEntity);
            return EntityUtils.GetColumnAttributes(type);
        }
        /// <summary>
        /// カラム名を取得する。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerable<PropertyInfo> GetColumnAttributes(Type type)
        {
            // カラム属性を取得する。
            IEnumerable<PropertyInfo> attributes = type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(prop => prop.GetCustomAttribute<ColumnAttribute>(false) != null);
            // 結果を戻す。
            return attributes;
        }

        /// <summary>
        /// <paramref name="dataRow"/>の値を使用してEntityを生成する。
        /// </summary>
        /// <typeparam name="TEntity">生成するEntityの型</typeparam>
        /// <param name="dataRow">Entityに設定する値</param>
        /// <returns></returns>
        public static TEntity Create<TEntity>(DataRow dataRow) where TEntity : AbstractEntity, new()
        {
            // インスタンスを生成する。
            TEntity entity = new TEntity();
            // dataRowから値を取得し、エンティティに設定する。
            entity.SetProperties(dataRow);
            // 結果を戻す。
            return entity;
        }

    }

}
