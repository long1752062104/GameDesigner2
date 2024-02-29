public partial class LoginPanel : GameCore.UIBase<LoginPanel>
{
    public MVC.View.FieldCollection collect;
    /// <summary>账号输入字段</summary>
    public UnityEngine.UI.InputField acc { get => collect.Get<UnityEngine.UI.InputField>(0); set => collect.Set(0, value); }
    /// <summary>密码输入字段</summary>
    public UnityEngine.UI.InputField pwd { get => collect.Get<UnityEngine.UI.InputField>(1); set => collect.Set(1, value); }
    /// <summary>登录按钮</summary>
    public UnityEngine.UI.Button login { get => collect.Get<UnityEngine.UI.Button>(2); set => collect.Set(2, value); }
    /// <summary>注册按钮</summary>
    public UnityEngine.UI.Button signUp { get => collect.Get<UnityEngine.UI.Button>(3); set => collect.Set(3, value); }
    /// <summary>关闭按钮</summary>
    public UnityEngine.UI.Button Btn_Close { get => collect.Get<UnityEngine.UI.Button>(4); set => collect.Set(4, value); }

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