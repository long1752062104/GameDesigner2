using Net.Table.Solvers;

namespace Net.Table
{
    public class ArrayTypeSolverCollector : ITypeSolverCollector
    {
        public void Initialize()
        {
            TypeSolver.SolverTypes.Add("boolArray", new BoolArrayTypeSolver());
            TypeSolver.SolverTypes.Add("byteArray", new ByteArrayTypeSolver());
            TypeSolver.SolverTypes.Add("sbyteArray", new SbyteArrayTypeSolver());
            TypeSolver.SolverTypes.Add("charArray", new CharArrayTypeSolver());
            TypeSolver.SolverTypes.Add("shortArray", new ShortArrayTypeSolver());
            TypeSolver.SolverTypes.Add("ushortArray", new UshortArrayTypeSolver());
            TypeSolver.SolverTypes.Add("intArray", new IntArrayTypeSolver());
            TypeSolver.SolverTypes.Add("uintArray", new UintArrayTypeSolver());
            TypeSolver.SolverTypes.Add("floatArray", new FloatArrayTypeSolver());
            TypeSolver.SolverTypes.Add("longArray", new LongArrayTypeSolver());
            TypeSolver.SolverTypes.Add("ulongArray", new UlongArrayTypeSolver());
            TypeSolver.SolverTypes.Add("doubleArray", new DoubleArrayTypeSolver());
            TypeSolver.SolverTypes.Add("decimalArray", new DecimalArrayTypeSolver());
            TypeSolver.SolverTypes.Add("dateTimeArray", new DateTimeArrayTypeSolver());
            TypeSolver.SolverTypes.Add("stringArray", new StringArrayTypeSolver());
        }
    }
}