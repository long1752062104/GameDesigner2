﻿using System;
using System.Collections.Generic;

namespace Net.Table.Solvers
{
    public class ListTypeSolver<T> : TypeSolver<List<T>>
    {
        public static List<T> SolverList(string text, Func<string, T> func)
        {
            var list = new List<T>();
            if (string.IsNullOrEmpty(text))
                return list;
            string[] items;
            if (text.Contains(";"))
                items = text.Split(';');
            else
                items = text.Split('\n');
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                list.Add(func(item));
            }
            return list;
        }
    }
}