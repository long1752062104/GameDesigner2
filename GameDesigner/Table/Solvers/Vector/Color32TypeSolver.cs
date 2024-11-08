namespace Net.Table.Solvers
{
    public class Color32TypeSolver : TypeSolver<Color32>
    {
        public override Color32 As(object excelValue)
        {
            if (excelValue == null)
                return default;
            if (excelValue is Color32 value)
                return value;
            var text = excelValue.ToString();
            if (string.IsNullOrEmpty(text))
                return default;
            var vectors = text.Split(',');
            if (vectors.Length < 4)
                return default;
            byte.TryParse(vectors[0], out var r);
            byte.TryParse(vectors[1], out var g);
            byte.TryParse(vectors[2], out var b);
            byte.TryParse(vectors[3], out var a);
            return new Color32(r, g, b, a);
        }
    }
}