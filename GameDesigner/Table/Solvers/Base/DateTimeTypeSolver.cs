using System;

namespace Net.Table.Solvers
{
    public class DateTimeTypeSolver : TypeSolver<DateTime>
    {
        public override DateTime As(object excelValue)
        {
            if (excelValue == null)
                return default;
            if (excelValue is DateTime value)
                return value;
            var text = excelValue.ToString();
            return ObjectConverter.AsDateTime(text);
        }
    }
}