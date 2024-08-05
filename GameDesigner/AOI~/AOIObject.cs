#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
namespace Net.Component
{
    using Net.AOI;
    using UnityEngine;
    using UnityEngine.Events;
    using Grid = AOI.Grid;

    public class AOIObject : MonoBehaviour, IGridBody
    {
        public int ID { get; set; }
        public int Identity { get; set; }
        public Net.Vector3 Position { get; set; }
        public Grid Grid { get; set; }
        public bool MainRole { get; set; }

        public bool IsLocal;

#if UNITY_EDITOR
        public bool DrawWire = true;
        public bool ShowText = true;
        public Color planeColor = Color.green;
        public Color textColor = Color.white;
#endif

        /// <summary>
        /// 当主角进入这个物体所在区域触发
        /// </summary>
        public UnityEvent OnMainRoleEnter;
        /// <summary>
        /// 当主角离开这个物体的区域触发
        /// </summary>
        public UnityEvent OnMainRoleExit;

        // Start is called before the first frame update
        void Start()
        {
            MainRole = IsLocal;
            Position = transform.position;
            AOIManager.I.world.Insert(this);
            if (Grid != null)
            {
                if (!IsLocal)//如果是其他玩家
                {
                    bool hasLocal = false;
                    var gridBodies = Grid.GetGridBodiesAll();
                    foreach (var body in gridBodies)
                    {
                        var node = body as AOIObject;
                        if (node == null)
                            continue;
                        if (node.IsLocal)//如果在这里9宫格范围有本机玩家, 显示出来
                        {
                            hasLocal = true;
                            break;
                        }
                    }
                    if (hasLocal)
                        OnMainRoleEnter.Invoke();
                    else
                        OnMainRoleExit.Invoke();
                }
            }
        }

        void OnDestroy()
        {
            var instance = AOIManager.Instance;
            if (instance == null)
                return;
            instance.world.Remove(this);
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (!IsLocal)
                return;
            Draw();
        }

        private void OnDrawGizmosSelected()
        {
            if (IsLocal)
                return;
            if (!Application.isPlaying)
                return;
            if (AOIManager.I.world == null)
                return;
            Draw();
        }

        private void Draw()
        {
            if (Grid == null)
                return;
            Gizmos.color = planeColor;
            GUI.color = textColor;
            for (int i = 0; i < Grid.grids.Length; i++)
            {
                Draw(Grid.grids[i]);
            }
        }

        private void Draw(Grid grid)
        {
            var pos = grid.rect.center;
            var size = grid.rect.size;
            if (AOIManager.I.world.gridType == GridType.Horizontal)
            {
                if (DrawWire)
                    Gizmos.DrawWireCube(new Vector3(pos.x, 0.5f, pos.y), new Vector3(size.x, 0.5f, size.y));
                else
                    Gizmos.DrawCube(new Vector3(pos.x, 0.1f, pos.y), new Vector3(size.x, 0.01f, size.y));
                if (ShowText) UnityEditor.Handles.Label(new Vector3(grid.rect.x + 0.5f, 0.5f, grid.rect.y + 1.5f), grid.rect.position.ToString());
            }
            else
            {
                if (DrawWire)
                    Gizmos.DrawWireCube(new Vector3(pos.x, pos.y, 0), new Vector3(size.x, size.y, 1f));
                else
                    Gizmos.DrawCube(new Vector3(pos.x, pos.y, 0), new Vector3(size.x, size.y, 1f));
                if (ShowText) UnityEditor.Handles.Label(new Vector3(grid.rect.x + 0.5f, grid.rect.y + 1.5f, -0.5f), grid.rect.position.ToString());
            }
        }
#endif

        public void OnInit()
        {
        }

        public void OnBodyUpdate()
        {
            Position = transform.position;
        }

        public void OnEnter(IGridBody body)
        {
            if (!MainRole)
                return;
            var node = body as AOIObject;
            node.OnMainRoleEnter.Invoke();
        }

        public void OnExit(IGridBody body)
        {
            if (!MainRole)
                return;
            var node = body as AOIObject;
            node.OnMainRoleExit.Invoke();
        }
    }
}
#endif