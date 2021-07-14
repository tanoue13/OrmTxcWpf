using System;

namespace OrmTxcWpf.Sql.Data
{

    public interface  IParameterValueConverter
    {

        public object Convert(object value, Type targetType, object parameter);
    
    }

}
