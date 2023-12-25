using System;
using System.Collections.Generic;
using Net.Serialize;

namespace Binding
{
    public class BindingType : IBindingType
    {
        public int SortingOrder { get; private set; }
        public Dictionary<Type, Type> BindTypes { get; private set; }
        public BindingType()
        {
            SortingOrder = 0;
            BindTypes = new Dictionary<Type, Type>
            {
                { typeof(Net.Vector2), typeof(NetVector2Bind) },
                { typeof(Net.Vector2[]), typeof(NetVector2ArrayBind) },
                { typeof(List<Net.Vector2>), typeof(SystemCollectionsGenericListNetVector2Bind) },
                { typeof(UnityEngine.Vector2), typeof(UnityEngineVector2Bind) },
                { typeof(UnityEngine.Vector2[]), typeof(UnityEngineVector2ArrayBind) },
                { typeof(List<UnityEngine.Vector2>), typeof(SystemCollectionsGenericListUnityEngineVector2Bind) },
                { typeof(UnityEngine.Vector2Int), typeof(UnityEngineVector2IntBind) },
                { typeof(UnityEngine.Vector2Int[]), typeof(UnityEngineVector2IntArrayBind) },
                { typeof(List<UnityEngine.Vector2Int>), typeof(SystemCollectionsGenericListUnityEngineVector2IntBind) },
                { typeof(Net.Vector3), typeof(NetVector3Bind) },
                { typeof(Net.Vector3[]), typeof(NetVector3ArrayBind) },
                { typeof(List<Net.Vector3>), typeof(SystemCollectionsGenericListNetVector3Bind) },
                { typeof(UnityEngine.Vector3), typeof(UnityEngineVector3Bind) },
                { typeof(UnityEngine.Vector3[]), typeof(UnityEngineVector3ArrayBind) },
                { typeof(List<UnityEngine.Vector3>), typeof(SystemCollectionsGenericListUnityEngineVector3Bind) },
                { typeof(UnityEngine.Vector3Int), typeof(UnityEngineVector3IntBind) },
                { typeof(UnityEngine.Vector3Int[]), typeof(UnityEngineVector3IntArrayBind) },
                { typeof(List<UnityEngine.Vector3Int>), typeof(SystemCollectionsGenericListUnityEngineVector3IntBind) },
                { typeof(Net.Vector4), typeof(NetVector4Bind) },
                { typeof(Net.Vector4[]), typeof(NetVector4ArrayBind) },
                { typeof(List<Net.Vector4>), typeof(SystemCollectionsGenericListNetVector4Bind) },
                { typeof(UnityEngine.Vector4), typeof(UnityEngineVector4Bind) },
                { typeof(UnityEngine.Vector4[]), typeof(UnityEngineVector4ArrayBind) },
                { typeof(List<UnityEngine.Vector4>), typeof(SystemCollectionsGenericListUnityEngineVector4Bind) },
                { typeof(Net.Quaternion), typeof(NetQuaternionBind) },
                { typeof(Net.Quaternion[]), typeof(NetQuaternionArrayBind) },
                { typeof(List<Net.Quaternion>), typeof(SystemCollectionsGenericListNetQuaternionBind) },
                { typeof(UnityEngine.Quaternion), typeof(UnityEngineQuaternionBind) },
                { typeof(UnityEngine.Quaternion[]), typeof(UnityEngineQuaternionArrayBind) },
                { typeof(List<UnityEngine.Quaternion>), typeof(SystemCollectionsGenericListUnityEngineQuaternionBind) },
                { typeof(Net.Rect), typeof(NetRectBind) },
                { typeof(Net.Rect[]), typeof(NetRectArrayBind) },
                { typeof(List<Net.Rect>), typeof(SystemCollectionsGenericListNetRectBind) },
                { typeof(UnityEngine.Rect), typeof(UnityEngineRectBind) },
                { typeof(UnityEngine.Rect[]), typeof(UnityEngineRectArrayBind) },
                { typeof(List<UnityEngine.Rect>), typeof(SystemCollectionsGenericListUnityEngineRectBind) },
                { typeof(UnityEngine.RectInt), typeof(UnityEngineRectIntBind) },
                { typeof(UnityEngine.RectInt[]), typeof(UnityEngineRectIntArrayBind) },
                { typeof(List<UnityEngine.RectInt>), typeof(SystemCollectionsGenericListUnityEngineRectIntBind) },
                { typeof(Net.Color), typeof(NetColorBind) },
                { typeof(Net.Color[]), typeof(NetColorArrayBind) },
                { typeof(List<Net.Color>), typeof(SystemCollectionsGenericListNetColorBind) },
                { typeof(Net.Color32), typeof(NetColor32Bind) },
                { typeof(Net.Color32[]), typeof(NetColor32ArrayBind) },
                { typeof(List<Net.Color32>), typeof(SystemCollectionsGenericListNetColor32Bind) },
                { typeof(UnityEngine.Color), typeof(UnityEngineColorBind) },
                { typeof(UnityEngine.Color[]), typeof(UnityEngineColorArrayBind) },
                { typeof(List<UnityEngine.Color>), typeof(SystemCollectionsGenericListUnityEngineColorBind) },
                { typeof(UnityEngine.Color32), typeof(UnityEngineColor32Bind) },
                { typeof(UnityEngine.Color32[]), typeof(UnityEngineColor32ArrayBind) },
                { typeof(List<UnityEngine.Color32>), typeof(SystemCollectionsGenericListUnityEngineColor32Bind) },
                { typeof(Net.Share.Operation), typeof(NetShareOperationBind) },
                { typeof(Net.Share.Operation[]), typeof(NetShareOperationArrayBind) },
                { typeof(List<Net.Share.Operation>), typeof(SystemCollectionsGenericListNetShareOperationBind) },
                { typeof(Net.Share.OperationList), typeof(NetShareOperationListBind) },
                { typeof(Net.Share.OperationList[]), typeof(NetShareOperationListArrayBind) },
                { typeof(List<Net.Share.OperationList>), typeof(SystemCollectionsGenericListNetShareOperationListBind) },
            };
        }
    }
}
