﻿using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class UshortListTypeSolver : ListTypeSolver<ushort>
    {
        public override List<ushort> As(object excelValue)
        {
            if (excelValue == null)
                return null;
            var text = excelValue.ToString();
            return SolverList(text, item => ObjectConverter.AsUshort(item));
        }
    }
}