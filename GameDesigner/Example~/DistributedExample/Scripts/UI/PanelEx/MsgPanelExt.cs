//项目需要用到UTF8编码进行保存, 默认情况下是中文编码(GB2312), 如果更新MVC后发现脚本的中文乱码则需要处理一下
//以下是设置UTF8编码的Url:方法二 安装插件
//url:https://blog.csdn.net/hfy1237/article/details/129858976
using System;

public partial class MsgPanel
{
    private void Start()
    {
        InitListener();
    }

    public void InitListener()
    {
        Btn_No.onClick.AddListener(OnBtn_NoClick);
        Btn_Yes.onClick.AddListener(OnBtn_YesClick);
        Btn_ClosePopup2.onClick.AddListener(OnBtn_ClosePopup2Click);
    }

    private void OnBtn_NoClick()
    {
        Hide();
    }
    private void OnBtn_YesClick()
    {
        Hide();
    }
    private void OnBtn_ClosePopup2Click()
    {
        Hide();
    }

    public override void OnShowUI(string title, string info, Action<bool> action)
    {
        Title.text = title;
        TextContent.text = info;
    }
}