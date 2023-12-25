using Net.System;
using System.Collections;
using System.Collections.Generic;

namespace Net.AOI
{
    /// <summary>
    /// 格子类
    /// </summary>
    public class Grid : IEnumerable<IGridBody>, IEnumerable
    {
        public int Id;
        public Rect rect;
        public Grid[] grids;//九宫格列表
        public FastListSafe<IGridBody> gridBodies = new FastListSafe<IGridBody>();//格子的物体

        public Grid() 
        {
            grids = new Grid[0]; //不能直接给9个格子, 因为边界格子没有九个
        }

        /// <summary>
        /// 获取九宫格的所有物体
        /// </summary>
        /// <returns></returns>
        public List<IGridBody> GetGridBodiesAll()
        {
            var gridBodies = new List<IGridBody>();
            foreach (var gridBody in this)
                gridBodies.Add(gridBody);
            return gridBodies;
        }

        public IEnumerator<IGridBody> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public struct Enumerator : IEnumerator<IGridBody>, IEnumerator
        {
            private readonly Grid grid;
            private int index;
            private int gridInx;
            private IGridBody current;
            public readonly IGridBody Current => current;
            readonly object IEnumerator.Current => Current;
            internal Enumerator(Grid grid)
            {
                this.grid = grid;
                index = 0;
                gridInx = 0;
                current = default;
            }
            public bool MoveNext()
            {
                var grid = this.grid;
            J: if (gridInx < grid.grids.Length)
                {
                    var gridBodies = grid.grids[gridInx].gridBodies;
                    if (index < gridBodies.Count)
                    {
                        current = gridBodies[index];
                        index++;
                        return true;
                    }
                    else
                    {
                        index = 0;
                        gridInx++;
                        goto J;
                    }
                }
                current = default;
                return false;
            }
            void IEnumerator.Reset()
            {
                index = 0;
                gridInx = 0;
                current = default;
            }
            public void Dispose()
            {
            }
        }
        public override string ToString()
        {
            return $"ID:{Id} Rect:{rect} Bodys:{gridBodies.Count}";
        }
    }
}
