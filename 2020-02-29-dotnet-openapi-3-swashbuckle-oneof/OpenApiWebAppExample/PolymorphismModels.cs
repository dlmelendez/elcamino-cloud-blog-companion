using System;

namespace OpenApiWebAppExample
{
    public class DynamicType
    {
        public string DataType { get; set; }
    }

    public class DynamicType<T> : DynamicType
    {      
         public T Value { get; set; }
    }
}
