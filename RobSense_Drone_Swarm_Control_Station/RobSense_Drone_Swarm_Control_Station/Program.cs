using System;
using System.Windows.Forms;

namespace RobSense_Drone_Swarm_Control_Station
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new EasySwarm());
        }
    }
}
