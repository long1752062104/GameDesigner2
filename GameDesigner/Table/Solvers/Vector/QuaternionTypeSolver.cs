namespace Net.Table.Solvers
{
    public class QuaternionTypeSolver : TypeSolver<Quaternion>
    {
        public override Quaternion As(object excelValue)
        {
            if (excelValue == null)
                return default;
            if (excelValue is Quaternion value)
                return value;
            var text = excelValue.ToString();
            if (string.IsNullOrEmpty(text))
                return default;
            var vectors = text.Split(',');
            if (vectors.Length < 4)
                return default;
            float.TryParse(vectors[0], out var x);
            float.TryParse(vectors[1], out var y);
            float.TryParse(vectors[2], out var z);
            float.TryParse(vectors[3], out var w);
            return new Quaternion(x, y, z, w);
        }
    }
}