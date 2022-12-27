using System.Runtime.InteropServices;

public static class KeyBdEvent
{
    [DllImport("user32.dll", EntryPoint = "keybd_event")]
    public static extern void keybd_event(
            byte bVk,            //虚拟键值 对应按键的ascll码十进制值  
            byte bScan,          //0
            int dwFlags,         //0 为按下，1按住，2为释放 
            int dwExtraInfo      //0
        );

    public static void SaveKey()
    {
        keybd_event(17, 0, 0, 0);//按下 ctrl
        //keybd_event(17, 0, 1, 0);//按住ctrl

        keybd_event(83, 0, 0, 0);//按下s

        keybd_event(17, 0, 2, 0);//抬起ctrl
        keybd_event(83, 0, 2, 0);//抬起s
    }
}
