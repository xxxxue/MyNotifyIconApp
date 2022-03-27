public static class Program
{
    //[STAThread]
    static void Main(string[] args)
    {

#if DEBUG
        WebProgram.RunWebHost(args);
#else
        Console.Title = "我的程序名";

        // 初始化 托盘图标 和 隐藏控制台
        NotifyIconUtils.Init();

        // 新开一个线程运行 web服务器
        Task.Run(() => WebProgram.RunWebHost(args));

        // 消息循环, 处理托盘图标的点击操作, 并保持程序的持续运行, 
        // 使用 Application.Exit() 退出
        Application.Run();
#endif
    }
}