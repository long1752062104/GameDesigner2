using Net.Table.Solvers;

namespace Net.Table
{
    public class VectorTypeSolverCollector : ITypeSolverCollector
    {
        public void Initialize()
        {
            TypeSolver.SolverTypes.Add("vector2", new Vector2TypeSolver());
            TypeSolver.SolverTypes.Add("vector3", new Vector3TypeSolver());
            TypeSolver.SolverTypes.Add("vector4", new Vector4TypeSolver());
            TypeSolver.SolverTypes.Add("quaternion", new QuaternionTypeSolver());
            TypeSolver.SolverTypes.Add("rect", new RectTypeSolver());
        }
    }
}