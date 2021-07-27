using Npgsql;
using OrmTxcWpf.Data;

namespace OrmTxcWpf.Npgsql.Data
{

    /// <summary>
    /// プロパティの接続文字列を戻すデータソース。
    /// </summary>
    public class NpgsqlConnectionStringBuilderDataSource : DataSource
    {

        public NpgsqlConnectionStringBuilder ConnectionStringBuilder { get; set; }

        public override string GetConnectionString()
            => this.ConnectionStringBuilder.ToString();

    }

}
