using Cysharp.Threading.Tasks;
using GameCore;
using System;

/// <summary>
/// Global扩展类
/// </summary>
public class Global : GameCore.Global
{
    //扩展你的Global管理器代码
    public UIBase[] panels;

    protected override void Awake()
    {
        base.Awake();
        foreach (var panel in panels)
            UI.AddForm(panel);
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