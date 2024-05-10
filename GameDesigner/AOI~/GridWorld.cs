using System;
using System.Collections.Generic;
using System.Linq;
using Net.System;

namespace Net.AOI
{
    /// <summary>
    /// 格子类型 -- 支持2D和3D游戏
    /// </summary>
    public enum GridType
    {
        /// <summary>
        /// 水平方式的格子, 用于3D游戏
        /// </summary>
        Horizontal,
        /// <summary>
        /// 垂直方式的格子, 适用2D游戏
        /// </summary>
        Vertical
    }

    /// <summary>
    /// 九宫格世界
    /// </summary>
    [Serializable]
    public class GridWorld
    {
        public Grid[] grids = new Grid[0];
        public FastListSafe<IGridBody> gridBodies = new FastListSafe<IGridBody>();
        public Rect worldSize;
        public GridType gridType = GridType.Horizontal;
        /// <summary>
        /// 当越界提示事件
        /// </summary>
        public Action<IGridBody> OnOverflow;

        /// <summary>
        /// 初始化九宫格
        /// </summary>
        /// <param name="xPos">x开始位置</param>
        /// <param name="zPos">z开始位置</param>
        /// <param name="xMax">x列最大值</param>
        /// <param name="zMax">z列最大值</param>
        /// <param name="width">格子长度</param>
        /// <param name="height">格子高度</param>
        public void Init(float xPos, float zPos, uint xMax, uint zMax, int width, int height)
        {
            grids = new Grid[xMax * zMax];
            worldSize = new Rect(xPos, zPos, width * xMax, height * zMax);
            int index = 0;
            for (int z = 0; z < zMax; z++)
            {
                float xPos1 = xPos;
                for (int x = 0; x < xMax; x++)
                {
                    var rect = new Rect(xPos1, zPos, width, height);
                    grids[index++] = new Grid() { rect = rect };
                    xPos1 += width;
                }
                zPos += height;
            }
            for (int i = 0; i < grids.Length; i++)
            {
                var grid = grids[i];
                grid.Id = i;
                var rect = new Rect(grid.rect.x - width, grid.rect.y - height, width * 3, height * 3);
                for (int j = 0; j < grids.Length; j++)
                {
                    var grid1 = grids[j];
                    if (rect.Contains(grid1.rect.position))
                        ArrayExtend.Add(ref grid.grids, grid1);
                }
            }
        }

        /// <summary>
        /// 插入物体到九宫格感兴趣区域
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public bool Insert(IGridBody body)
        {
            if (body.Grid != null) //防止多次插入
                return false;
            body.OnInit();
            for (int i = 0; i < grids.Length; i++)
            {
                var grid = grids[i];
                if (Contains(grid, body))
                {
                    grid.gridBodies.Add(body);
                    body.Grid = grid;
                    body.ID = gridBodies.Count;
                    gridBodies.Add(body);
                    for (int j = 0; j < grid.grids.Length; j++)
                    {
                        var grid1 = grid.grids[j];
                        for (int k = 0; k < grid1.gridBodies.Count; k++) //通知新格子, 此物体进来了
                        {
                            var item2 = grid1.gridBodies[k];
                            EnterHandler(body, item2);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        public bool Contains(Grid grid, IGridBody body)
        {
            if (gridType == GridType.Horizontal)
                return grid.rect.ContainsXZ(body.Position);
            return grid.rect.Contains(body.Position);
        }

        /// <summary>
        /// 获取物体的感兴趣九宫格区域
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public Grid TryGetGrid(IGridBody body)
        {
        JMP:
            var currGrid = body.Grid;
            if (currGrid == null)
            {
                if (gridType == GridType.Horizontal)
                {
                    if (!worldSize.ContainsXZ(body.Position))
                        goto J;
                }
                else
                {
                    if (!worldSize.Contains(body.Position))
                        goto J;
                }
                for (int i = 0; i < grids.Length; i++)
                {
                    var grid = grids[i];
                    if (Contains(grid, body))
                    {
                        grid.gridBodies.Add(body);
                        body.Grid = grid;
                        for (int j = 0; j < grid.grids.Length; j++)
                        {
                            var grid1 = grid.grids[j];
                            for (int k = 0; k < grid1.gridBodies.Count; k++) //通知新格子, 此物体进来了
                            {
                                var item2 = grid1.gridBodies[k];
                                EnterHandler(body, item2);
                            }
                        }
                        return grid;
                    }
                }
                goto J;
            }
            if (Contains(currGrid, body))
                return currGrid;
            var oldGrids = currGrid.grids;
            for (int i = 0; i < oldGrids.Length; i++) //遍历当前物体所在的九宫格
            {
                var grid = oldGrids[i];
                if (Contains(grid, body))//如果物体还在九宫格的其中一个格子
                {
                    var newGrids = grid.grids;
                    for (int j = 0; j < newGrids.Length; j++) //遍历当前所在的九宫格,如果是新进来的格子, 则通知新格子此物体进来了
                    {
                        var grid1 = newGrids[j];
                        if (!oldGrids.Contains(grid1))//如果当前为新格子, 则通知
                        {
                            for (int k = 0; k < grid1.gridBodies.Count; k++) //通知新格子, 此物体进来了
                            {
                                var item2 = grid1.gridBodies[k];
                                EnterHandler(body, item2);
                            }
                        }
                    }
                    for (int k = 0; k < oldGrids.Length; k++) //遍历当前所在的九宫格,如果已经离开的九宫格则调用离开方法
                    {
                        var grid1 = oldGrids[k];
                        if (!newGrids.Contains(grid1))//如果已经离开了旧的格子
                        {
                            for (int j = 0; j < grid1.gridBodies.Count; j++) //通知离开的格子的所有物体, 此物体离开了
                            {
                                var item2 = grid1.gridBodies[j];
                                ExitHandler(body, item2);
                            }
                        }
                    }
                    //更新物体所在的格子
                    currGrid.gridBodies.Remove(body);
                    grid.gridBodies.Add(body);
                    body.Grid = grid;
                    return grid;
                }
            }
            for (int i = 0; i < oldGrids.Length; i++) //拖拽瞬移过程
            {
                var grid = oldGrids[i];
                for (int j = 0; j < grid.gridBodies.Count; j++) //通知离开的格子的所有物体, 此物体离开了
                {
                    var item2 = grid.gridBodies[j];
                    ExitHandler(body, item2);
                }
            }
            currGrid.gridBodies.Remove(body);
            body.Grid = null;
            goto JMP;
            J:
            if (OnOverflow != null)
                OnOverflow(body);
            else
                Event.NDebug.LogError($"{body.ID}越界了,位置:{body.Position}");
            return null;
        }

        /// <summary>
        /// 移除感兴趣物体
        /// </summary>
        /// <param name="body"></param>
        public void Remove(IGridBody body)
        {
            var currGrid = body.Grid;
            if (currGrid == null)
                return;
            var grids = currGrid.grids;
            for (int i = 0; i < grids.Length; i++)
            {
                var grid = grids[i];
                for (int j = 0; j < grid.gridBodies.Count; j++) //通知离开的格子的所有物体, 此物体离开了
                {
                    var item2 = grid.gridBodies[j];
                    ExitHandler(body, item2);
                }
            }
            currGrid.gridBodies.Remove(body);
            gridBodies.Remove(body);
            body.Grid = null; //防止执行两次
        }

        /// <summary>
        /// 当一个物体进入另外一个物体处理
        /// </summary>
        /// <param name="body"></param>
        /// <param name="other"></param>
        private static void EnterHandler(IGridBody body, IGridBody other)
        {
            if (body == other | body.Identity == other.Identity) //当插入时会偶尔发生相同的玩家通知问题
                return;
            if (body.MainRole & other.MainRole) //如果两个都是主角, 则相互通知
            {
                body.OnEnter(other);
                other.OnEnter(body);
            }
            else if (body.MainRole) //当以自己为主角时, 这样通知
            {
                body.OnEnter(other);
            }
            else other.OnEnter(body);
        }

        /// <summary>
        /// 当一个物体退出另外一个物体处理
        /// </summary>
        /// <param name="body"></param>
        /// <param name="other"></param>
        private static void ExitHandler(IGridBody body, IGridBody other)
        {
            if (body == other | body.Identity == other.Identity) //当插入时会偶尔发生相同的玩家通知问题
                return;
            if (body.MainRole & other.MainRole) //如果两个都是主角, 则相互通知
            {
                body.OnExit(other);
                other.OnExit(body);
            }
            else if (body.MainRole) //当以自己为主角时, 这样通知
            {
                body.OnExit(other);
            }
            else other.OnExit(body);
        }

        /// <summary>
        /// 更新感兴趣的移除和添加物体
        /// </summary>
        public void UpdateHandler()
        {
            for (int i = 0; i < gridBodies._size; i++)
            {
                var body = gridBodies._items[i];
                TryGetGrid(body);
                body.OnBodyUpdate(); //如果OnBodyUpdate放在前面并且调用Remove方法会出现问题
            }
        }
    }
}
