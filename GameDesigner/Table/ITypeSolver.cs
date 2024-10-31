using System;

namespace Net.Table
{
    /// <summary>
    /// Excel类型求解器
    /// </summary>
    public interface ITypeSolver
    {
        Type DataType { get; }
        object As(object excelValue);
    }
}