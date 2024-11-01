using System;

namespace Net.Table.Solvers
{
    public class EnumTypeSolver<T> : TypeSolver<T>
    {
        public override T As(object excelValue)
        {
            if (excelValue == null)
                return default;
            var text = excelValue.ToString();
#if CORE
            if (Enum.TryParse(DataType, text, out var result))
                return (T)result;
#else
            try
            {
                var result = Enum.Parse(DataType, text);
                return (T)result;
            }
            finally { }
#endif
            if (int.TryParse(text, out var number))
                return (T)Enum.ToObject(DataType, number);
            throw new Exception($"枚举类型:{DataType}解析失败!");
        }
    }
}