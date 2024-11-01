namespace Net.Table.Solvers
{
    public class RectTypeSolver : TypeSolver<Rect>
    {
        public override Rect As(object excelValue)
        {
            if (excelValue == null)
                return default;
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
            return new Rect(x, y, z, w);
        }
    }
}