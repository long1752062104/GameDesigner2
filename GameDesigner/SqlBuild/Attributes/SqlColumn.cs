using System;

namespace Net.SqlBuild
{
    public class SqlColumn : Attribute
    {
        public string Name { get; set; }
        public bool IsPrimaryKey { get; set; }
        public int Length { get; set; }
        public bool NotNull { get; set; }
        public string Description { get; set; }
    }
}
