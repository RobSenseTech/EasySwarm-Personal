using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace RobSense_Drone_Swarm_Control_Station
{
    class IEversion
    {
        /// <summary>
              /// IE WebBrowser内核设置
              /// </summary>
        public void BrowserEmulationSet()
        {
            //当前程序名称
            var exeName = Process.GetCurrentProcess().ProcessName + ".exe";
            //系统注册表信息
            var mreg = Registry.LocalMachine;
            //IE注册表信息
            var ie = mreg.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION", RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (ie != null)
            {
                try
                {
                    var val = ieVersionEmulation(ieVersion());
                    if (val != 0)
                    {
                        ie.SetValue(exeName, val);
                    }
                    mreg.Close();
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                }
            }
        }

        /// <summary>
              /// IE版本号
              /// </summary>
              /// <returns></returns>
        static int ieVersion()
        {
            //IE版本号
            RegistryKey mreg = Registry.LocalMachine;
            mreg = mreg.CreateSubKey("SOFTWARE\\Microsoft\\Internet Explorer");

            //更新版本
            var svcVersion = mreg.GetValue("svcVersion");
            if (svcVersion != null)
            {
                mreg.Close();
                var v = svcVersion.ToString().Split('.')[0];
                return int.Parse(v);
            }
            else
            {
                //默认版本
                var ieVersion = mreg.GetValue("Version");
                mreg.Close();
                if (ieVersion != null)
                {
                    var v = ieVersion.ToString().Split('.')[0];
                    return int.Parse(v);
                }
            }
            return 0;
        }

        /// <summary>
              /// 根据IE版本号 返回Emulation值
              /// </summary>
              /// <param name="ieVersion"></param>
              /// <returns></returns>
        static int ieVersionEmulation(int ieVersion)
        {
            //IE7 7000 (0x1B58)
            if (ieVersion < 8)
            {
                return 0;
            }
            if (ieVersion == 8)
            {
                return 0x1F40;//8000 (0x1F40)、8888 (0x22B8)
            }
            if (ieVersion == 9)
            {
                return 0x2328;//9000 (0x2328)、9999 (0x270F)
            }
            else if (ieVersion == 10)
            {
                return 0x02710;//10000 (0x02710)、10001 (0x2711)
            }
            else if (ieVersion == 11)
            {
                return 0x2AF8;//11000 (0x2AF8)、11001 (0x2AF9
            }
            return 0;
        }
    }
}
