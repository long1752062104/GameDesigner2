//项目需要用到UTF8编码进行保存, 默认情况下是中文编码(GB2312), 如果更新MVC后发现脚本的中文乱码则需要处理一下
//以下是设置UTF8编码的Url:方法二 安装插件
//url:https://blog.csdn.net/hfy1237/article/details/129858976
using GameCore;

public partial class LoginPanel
{
    private void Start()
    {
        InitListener();
    }

    public void InitListener()
    {
        login.onClick.AddListener(OnloginClick);
        signUp.onClick.AddListener(OnsignUpClick);
        Btn_Close.onClick.AddListener(OnBtn_CloseClick);
    }

    private async void OnloginClick()
    {
        if (acc.text.Length == 0 | pwd.text.Length == 0)
        {
            Global.UI.Message.ShowUI("登录提示", "请输入账号或密码!");
            return;
        }
        var code = await Global.Network[0].Request<int>((int)ProtoType.Login, acc.text, pwd.text);
        if (code == 0)
        {
            Global.UI.Tips.ShowUI("登录成功!");
        }
        else
        {
            Global.UI.Tips.ShowUI("账号或密码错误!");
        }
    }
    private void OnsignUpClick()
    {
        RegisterPanel.Show(null, UIMode.HideCurrUI);
    }
    private void OnBtn_CloseClick()
    {
    }
}
