using System;

namespace Net.SqlBuild
{
    public class SqlTable : Attribute
    {
        public string Name { get; set; }

        public SqlTable(string name)
        {
            Name = name;
        }
    }
}