namespace Net.Table.Solvers
{
    public class ColorTypeSolver : TypeSolver<Color>
    {
        public override Color As(object excelValue)
        {
            if (excelValue == null)
                return default;
            if (excelValue is Color value)
                return value;
            var text = excelValue.ToString();
            if (string.IsNullOrEmpty(text))
                return default;
            var vectors = text.Split(',');
            if (vectors.Length < 4)
                return default;
            float.TryParse(vectors[0], out var r);
            float.TryParse(vectors[1], out var g);
            float.TryParse(vectors[2], out var b);
            float.TryParse(vectors[3], out var a);
            return new Color(r, g, b, a);
        }
    }
}