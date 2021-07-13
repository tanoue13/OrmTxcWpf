namespace OrmTxcWpf.Sql.Data
{

    /// <summary>
    /// プロパティの接続文字列を戻すデータソース。
    /// </summary>
    public class ConnectionStringDataSource : DataSource
    {

        /// <summary>
        /// 接続文字列を取得または設定します。
        /// </summary>
        public string ConnectionString { get; set; }

        public override string GetConnectionString() => this.ConnectionString;

    }

}
