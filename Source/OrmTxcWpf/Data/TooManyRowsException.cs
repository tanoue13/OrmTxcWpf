using System;
using System.Data.Common;
using System.Runtime.Serialization;

namespace OrmTxcWpf.Data
{

    /// <summary>
    /// 単一レコードしか許されない部分で複数行処理戻されている場合に投げられる例外クラス。
    /// </summary>
    /// <see cref="https://www.shift-the-oracle.com/plsql/exception/predefined-exception.html"/>
    /// <see cref="https://www.ibm.com/support/knowledgecenter/ja/SSEPGG_9.7.0/com.ibm.db2.luw.apdv.plsql.doc/doc/c0053876.html"/>
    public class TooManyRowsException : DbException
    {
        public TooManyRowsException() : base()
        {
        }
        public TooManyRowsException(string message) : base(message)
        {
        }
        public TooManyRowsException(string message, Exception innerException) : base(message, innerException)
        {
        }
        public TooManyRowsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        public TooManyRowsException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }

}
