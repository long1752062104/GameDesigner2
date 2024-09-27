using Cysharp.Threading.Tasks;
using GameCore;

/// <summary>
/// Global扩展类
/// </summary>
public class Global : GameCore.Global
{
    //扩展你的Global管理器代码

    protected override void Awake()
    {
        base.Awake();
    }

    public override async void OnInit()
    {
        float progress = 0f;
        while (progress < 1f)
        {
            progress += 0.1f;
            Global.UI.Loading.ShowUI($"正在加载...{progress * 100f}%", progress);
            await UniTask.Delay(100);
        }
        Global.UI.Loading.HideUI();
        OnLoadingDone();
    }

    private void OnLoadingDone()
    {
        LoginPanel.Show(formMode: UIMode.None);
    }
}