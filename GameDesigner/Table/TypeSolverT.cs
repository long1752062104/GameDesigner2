using System;

namespace Net.Table
{
    public class TypeSolver<T> : TypeSolver, ITypeSolver
    {
        public static TypeSolver<T> Solver;
        public virtual Type DataType => typeof(T);

        public TypeSolver()
        {
            Solver = this;
        }

        public virtual T As(object excelValue)
        {
            throw new NotImplementedException();
        }

        object ITypeSolver.As(object excelValue)
        {
            return Solver.As(excelValue);
        }
    }
}