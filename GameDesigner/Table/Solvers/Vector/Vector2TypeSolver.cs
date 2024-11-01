namespace Net.Table.Solvers
{
    public class Vector2TypeSolver : TypeSolver<Vector2>
    {
        public override Vector2 As(object excelValue)
        {
            if (excelValue == null)
                return default;
            var text = excelValue.ToString();
            if (string.IsNullOrEmpty(text))
                return default;
            var vectors = text.Split(',');
            if (vectors.Length < 2)
                return default;
            float.TryParse(vectors[0], out var x);
            float.TryParse(vectors[1], out var y);
            return new Vector2(x, y);
        }
    }
}