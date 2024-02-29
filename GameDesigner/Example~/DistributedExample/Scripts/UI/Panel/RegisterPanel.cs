public partial class RegisterPanel : GameCore.UIBase<RegisterPanel>
{
    public MVC.View.FieldCollection collect;
    
    public UnityEngine.UI.InputField pwd { get => collect.Get<UnityEngine.UI.InputField>(0); set => collect.Set(0, value); }
    
    public UnityEngine.UI.InputField acc { get => collect.Get<UnityEngine.UI.InputField>(1); set => collect.Set(1, value); }
    
    public UnityEngine.UI.Button Btn_Close { get => collect.Get<UnityEngine.UI.Button>(2); set => collect.Set(2, value); }
    
    public UnityEngine.UI.Button login { get => collect.Get<UnityEngine.UI.Button>(3); set => collect.Set(3, value); }
    
    public UnityEngine.UI.Button register { get => collect.Get<UnityEngine.UI.Button>(4); set => collect.Set(4, value); }

    public void Init(MVC.View.FieldCollection collect)
    {
        this.collect = collect;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        var getComponentMethod = GetType().GetMethod("GetComponent", new System.Type[] { typeof(System.Type) }); //当处于热更新脚本, 不继承MonoBehaviour时处理
        if (getComponentMethod != null)
            collect = getComponentMethod.Invoke(this, new object[] { typeof(MVC.View.FieldCollection) }) as MVC.View.FieldCollection;
    }
#endif
}