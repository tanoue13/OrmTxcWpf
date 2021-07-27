using System.Data.SqlClient;
using OrmTxcWpf.Data;

namespace OrmTxcWpf.SqlClient.Data
{

    /// <summary>
    /// プロパティの接続文字列を戻すデータソース。
    /// </summary>
    public class SqlConnectionStringBuilderDataSource : DataSource
    {

        public SqlConnectionStringBuilder ConnectionStringBuilder { get; set; }

        public override string GetConnectionString()
            => this.ConnectionStringBuilder.ToString();

    }

}
