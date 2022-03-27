using System.Runtime.InteropServices;
using System.Diagnostics;

public static class NotifyIconUtils
{
    #region 句柄实用工具

    [DllImport("User32.dll", EntryPoint = "FindWindow")]
    static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
    static extern IntPtr GetSystemMenu(IntPtr hWnd, IntPtr bRevert);

    [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
    static extern IntPtr RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

    [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
    public static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

    /// <summary>
    /// 控制台窗口句柄
    /// </summary>
    public static IntPtr ConsoleWindowHandle { get; set; }

    /// <summary>
    /// 自动寻找当前控制台句柄
    /// </summary>
    /// <returns></returns>
    public static IntPtr GetConsoleWindowHandle()
    {
        if (ConsoleWindowHandle == default)
        {
            ConsoleWindowHandle = FindWindow(null, Console.Title);
        }

        return ConsoleWindowHandle;
    }

    /// <summary>
    /// 禁用控制台窗口的关闭按钮
    /// </summary>
    public static void DisableCloseButton()
    {
        IntPtr closeMenu = GetSystemMenu(GetConsoleWindowHandle(), IntPtr.Zero);
        uint SC_CLOSE = 0xF060;
        RemoveMenu(closeMenu, SC_CLOSE, 0x0);
    }

    static void HiddenConsoleWindow()
    {
        uint SW_HIDE = 0;
        ShowWindow(GetConsoleWindowHandle(), SW_HIDE);
    }

    static void ShowConsoleWindow()
    {
        uint SW_SHOW = 5;
        ShowWindow(GetConsoleWindowHandle(), SW_SHOW);
        SetForegroundWindow(GetConsoleWindowHandle());
    }

    static bool _visable = true;

    /// <summary>
    /// 通过属性控制 console 的显示与隐藏
    /// </summary>
    public static bool ConsoleVisable
    {
        get { return _visable; }
        set
        {
            _visable = value;
            if (_visable)
                ShowConsoleWindow();
            else
                HiddenConsoleWindow();
        }
    }

    /// <summary>
    /// 关闭某一个窗口
    /// </summary>
    public static void CloseWindow(IntPtr hwnd)
    {
        UInt32 WM_CLOSE = 0x0010;
        SendMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
    }

    #endregion

    #region 托盘图标

    private static NotifyIcon _notifyIcon = new();

    /// <summary>
    /// 初始化
    /// 在程序的开头执行
    /// </summary>
    public static void Init()
    {
        NotifyIconInitialization();
        ShowNotifyIcon();
        DisableCloseButton();
        ConsoleVisable = false;
    }

    public static void NotifyIconInitialization()
    {
        _notifyIcon.Icon = new Icon("appicon.ico");
        _notifyIcon.Visible = false;
        _notifyIcon.Text = Console.Title;

        var menu = new ContextMenuStrip()
        {
            RenderMode = ToolStripRenderMode.System,
        };

        menu.Items.Add(new ToolStripMenuItem()
        {
            Name = "OpenUrl",
            Text = "打开浏览器"
        });

        menu.Items.Add(new ToolStripMenuItem()
        {
            Name = "Console",
            Text = "打开/关闭控制台"
        });

        menu.Items.Add(new ToolStripMenuItem()
        {
            Name = "Exit",
            Text = "退出"
        });

        // 设置右键菜单
        _notifyIcon.ContextMenuStrip = menu;

        menu.ItemClicked += (s, e) =>
        {
            Console.WriteLine("托盘菜单被点击");
            Console.WriteLine("被点击的菜单是:{0}", e.ClickedItem.Text);
            switch (e.ClickedItem.Name)
            {
                case "Exit":

                    // 结束程序
                    Application.Exit();
                    break;
                case "OpenUrl":
                    // 打开浏览器
                    OpenBrowser(Url);
                    break;
                case "Console":
                    // 控制台窗口的显示/隐藏
                    ConsoleVisable = !ConsoleVisable;
                    break;
            }
        };

        // 双击
        _notifyIcon.MouseDoubleClick += (s, e) =>
        {
            Console.WriteLine("托盘被双击.");
            ConsoleVisable = !ConsoleVisable;
        };
    }

    public static void ShowNotifyIcon()
    {
        _notifyIcon.Visible = true;
        // 弹出系统提示
        //_notifyIcon.ShowBalloonTip(3000, "", "我是托盘图标，右键单击显示菜单，左键双击 显示/隐藏 控制台窗口。", ToolTipIcon.None);
    }

    public static void HideNotifyIcon()
    {
        _notifyIcon.Visible = false;
    }

    #endregion

    public static string? Url { get; set; }

    /// <summary>
    /// 打开浏览器
    /// </summary>
    public static void OpenBrowser(string url)
    {
        Console.WriteLine("打开浏览器:" + url);
        var browser =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new ProcessStartInfo("cmd", $"/c start {url}") :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? new ProcessStartInfo("open", url) :
            new ProcessStartInfo("xdg-open", url); //linux, unix-like

        Process.Start(browser);
    }

    /// <summary>
    /// 启动 web host 并 打开浏览器
    /// </summary>
    /// <param name="app"></param>
    /// <param name="url"></param>
    public static void RunAndStartedOpenBrowser(this WebApplication app, string? url = null)
    {
        url ??= app.Services.GetService<IConfiguration>()?["urls"]?.Split(";")?[^1] ?? "http://localhost:5000";

        Url = url;

        // 从容器中拿到 生命周期对象
        var lifetime = app.Services.GetService<IHostApplicationLifetime>();
        // 注册 程序启动成功后,要执行的方法
        lifetime?.ApplicationStarted.Register(() => { OpenBrowser(Url); });

        app.Run(url);
    }
}