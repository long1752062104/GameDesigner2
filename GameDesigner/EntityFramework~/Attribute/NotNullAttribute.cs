using System;

namespace Net.EntityFramework
{
    internal class NotNullAttribute : Attribute
    {
        public string Exception { get; set; }

        public NotNullAttribute(string exception = "ArgumentNullException")
        {
            this.Exception = exception;
        }
    }
}