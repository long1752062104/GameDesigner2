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
            return (T)Enum.ToObject(DataType, ObjectConverter.AsInt(text));
        }
    }
}