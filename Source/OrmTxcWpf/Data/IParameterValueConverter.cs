using System;

namespace OrmTxcWpf.Data
{

    public interface  IParameterValueConverter
    {

        public object Convert(object value, Type targetType, object parameter);
    
    }

}
