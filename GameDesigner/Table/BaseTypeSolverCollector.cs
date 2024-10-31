using Net.Table.Solvers;

namespace Net.Table
{
    public class BaseTypeSolverCollector : ITypeSolverCollector
    {
        public void Initialize()
        {
            TypeSolver.SolverTypes.Add("bool", new BoolTypeSolver());
            TypeSolver.SolverTypes.Add("byte", new ByteTypeSolver());
            TypeSolver.SolverTypes.Add("sbyte", new SbyteTypeSolver());
            TypeSolver.SolverTypes.Add("char", new CharTypeSolver());
            TypeSolver.SolverTypes.Add("short", new ShortTypeSolver());
            TypeSolver.SolverTypes.Add("ushort", new UshortTypeSolver());
            TypeSolver.SolverTypes.Add("int", new IntTypeSolver());
            TypeSolver.SolverTypes.Add("uint", new UintTypeSolver());
            TypeSolver.SolverTypes.Add("float", new FloatTypeSolver());
            TypeSolver.SolverTypes.Add("long", new LongTypeSolver());
            TypeSolver.SolverTypes.Add("ulong", new UlongTypeSolver());
            TypeSolver.SolverTypes.Add("double", new DoubleTypeSolver());
            TypeSolver.SolverTypes.Add("decimal", new DecimalTypeSolver());
            TypeSolver.SolverTypes.Add("dateTime", new DateTimeTypeSolver());
            TypeSolver.SolverTypes.Add("string", new StringTypeSolver());
        }
    }
}