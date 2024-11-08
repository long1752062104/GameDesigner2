using System;

namespace Net.Table.Solvers
{
    public class DateTimeArrayTypeSolver : ArrayTypeSolver<DateTime>
    {
        public override DateTime[] As(object excelValue)
        {
            if (excelValue == null)
                return null;
            if (excelValue is DateTime[] dateTimes)
                return dateTimes;
            var text = excelValue.ToString();
            return SolverArray(text, item => ObjectConverter.AsDateTime(item));
        }
    }
}