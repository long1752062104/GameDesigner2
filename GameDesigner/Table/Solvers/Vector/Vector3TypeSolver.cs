namespace Net.Table.Solvers
{
    public class Vector3TypeSolver : TypeSolver<Vector3>
    {
        public override Vector3 As(object excelValue)
        {
            if (excelValue == null)
                return default;
            var text = excelValue.ToString();
            if (string.IsNullOrEmpty(text))
                return default;
            var vectors = text.Split(',');
            if (vectors.Length < 3)
                return default;
            float.TryParse(vectors[0], out var x);
            float.TryParse(vectors[1], out var y);
            float.TryParse(vectors[2], out var z);
            return new Vector3(x, y, z);
        }
    }
}