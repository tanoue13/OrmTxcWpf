using System.Collections.Generic;
using System.Data;

namespace OrmTxcWpf.Sql.Daos
{

    public interface IDao
    {

        /// <summary>
        /// Daoで使用するコマンド（IDbCommand）を取得します。
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        IEnumerable<IDbCommand> Commands { get; }

    }

}
