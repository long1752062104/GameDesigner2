#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS || UNITY_WSA || UNITY_WEBGL
using UnityEngine;
using Net.AOI;
using Grid = Net.AOI.Grid;

namespace Net.Component
{
    public class AOIManager : SingleCase<AOIManager>
    {
        public GridWorld world = new GridWorld();
        public float xPos = -500f;
        public float zPos = -500f;
        public uint xMax = 50;
        public uint zMax = 50;
        public int width = 20;
        public int height = 20;
#if UNITY_EDITOR
        public bool showText;
        public UnityEngine.Color planeColor = UnityEngine.Color.cyan;
        public UnityEngine.Color textColor = UnityEngine.Color.white;
#endif

        protected override void Awake()
        {
            base.Awake();
            InitAOI();
        }

        public void InitAOI()
        {
            world.Init(xPos, zPos, xMax, zMax, width, height);
        }

        private void Update()
        {
            world.UpdateHandler();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = planeColor;
            GUI.color = textColor;
            for (int i = 0; i < world.grids.Length; i++)
            {
                Draw(world.grids[i]);
            }
        }

        private void Draw(Grid grid)
        {
            var pos = grid.rect.center;
            var size = grid.rect.size;
            if (world.gridType == GridType.Horizontal)
                Gizmos.DrawWireCube(new UnityEngine.Vector3(pos.x, 0f, pos.y), new UnityEngine.Vector3(size.x, 0, size.y));
            else
                Gizmos.DrawWireCube(new UnityEngine.Vector3(pos.x, pos.y, 0f), new UnityEngine.Vector3(size.x, size.y, 0));
            if (showText)
            {
                if (world.gridType == GridType.Horizontal)
                    UnityEditor.Handles.Label(new UnityEngine.Vector3(grid.rect.x + 0.5f, 1f, grid.rect.y + 1.5f), grid.rect.position.ToString());
                else
                    UnityEditor.Handles.Label(new UnityEngine.Vector3(grid.rect.x + 0.5f, grid.rect.y + 1.5f, 0f), grid.rect.position.ToString());
            }
        }
#endif
    }
}
#endif