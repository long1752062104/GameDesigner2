//项目需要用到UTF8编码进行保存, 默认情况下是中文编码(GB2312), 如果更新MVC后发现脚本的中文乱码则需要处理一下
//以下是设置UTF8编码的Url:方法二 安装插件
//url:https://blog.csdn.net/hfy1237/article/details/129858976
public partial class RegisterPanel
{
    private void Start()
    {
        InitListener();
    }

    public void InitListener()
    {
        Btn_Close.onClick.AddListener(OnBtn_CloseClick);
        login.onClick.AddListener(OnloginClick);
        register.onClick.AddListener(OnregisterClick);
    }

    private void OnBtn_CloseClick()
    {
        Hide();
    }
    private void OnloginClick()
    {
        Hide();
    }
    private async void OnregisterClick()
    {
        if (acc.text.Length == 0 | pwd.text.Length == 0)
        {
            Global.UI.Message.ShowUI("注册提示", "请输入账号或密码!");
            return;
        }
        var code = await Global.Network[0].Request<int>((int)ProtoType.Register, acc.text, pwd.text);
        if (code == 0)
        {
            Global.UI.Message.ShowUI("注册提示", "注册成功!");
        }
        else
        {
            Global.UI.Message.ShowUI("注册提示", "注册失败!");
        }
    }
}