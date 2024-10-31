﻿using System;
using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class ArrayTypeSolver<T> : TypeSolver<T[]>
    {
        public static T[] SolverArray(string text, Func<string, T> func)
        {
            var list = new List<T>();
            if (string.IsNullOrEmpty(text))
                return new T[0];
            var items = text.Split(';');
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                list.Add(func(item));
            }
            return list.ToArray();
        }
    }
}