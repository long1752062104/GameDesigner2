using Net.Table.Solvers;

namespace Net.Table
{
    public class ListTypeSolverCollector : ITypeSolverCollector
    {
        public void Initialize()
        {
            TypeSolver.SolverTypes.Add("boolList", new BoolListTypeSolver());
            TypeSolver.SolverTypes.Add("byteList", new ByteListTypeSolver());
            TypeSolver.SolverTypes.Add("sbyteList", new SbyteListTypeSolver());
            TypeSolver.SolverTypes.Add("charList", new CharListTypeSolver());
            TypeSolver.SolverTypes.Add("shortList", new ShortListTypeSolver());
            TypeSolver.SolverTypes.Add("ushortList", new UshortListTypeSolver());
            TypeSolver.SolverTypes.Add("intList", new IntListTypeSolver());
            TypeSolver.SolverTypes.Add("uintList", new UintListTypeSolver());
            TypeSolver.SolverTypes.Add("floatList", new FloatListTypeSolver());
            TypeSolver.SolverTypes.Add("longList", new LongListTypeSolver());
            TypeSolver.SolverTypes.Add("ulongList", new UlongListTypeSolver());
            TypeSolver.SolverTypes.Add("doubleList", new DoubleListTypeSolver());
            TypeSolver.SolverTypes.Add("decimalList", new DecimalListTypeSolver());
            TypeSolver.SolverTypes.Add("dateTimeList", new DateTimeListTypeSolver());
            TypeSolver.SolverTypes.Add("stringList", new StringListTypeSolver());
        }
    }
}