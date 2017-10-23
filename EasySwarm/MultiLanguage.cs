using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EasySwarm
{
    static class MultiLanguage
    {
        //Current default language
        public static string DefaultLanguage = "zh-CN";

        /// <summary>
        /// Current default language
        /// </summary>
        /// <param name="lang">To set the default language</param>
        public static void SetDefaultLanguage(string lang)
        {
            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang);
            DefaultLanguage = lang;
            Properties.Settings.Default.DefaultLanguage = lang;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Load language
        /// </summary>
        /// <param name="form">Load language window</param>
        /// <param name="formType">The type of window</param>
        public static void LoadLanguage(Form form, Type formType)
        {
            if (form != null)
            {
                System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(formType);
                resources.ApplyResources(form, "$this");
                Loading(form, resources);
            }
        }

        /// <summary>
        /// Load language
        /// </summary>
        /// <param name="control">Controls</param>
        /// <param name="resources">Language resources</param>
        private static void Loading(Control control, System.ComponentModel.ComponentResourceManager resources)
        {
            if (control is MenuStrip)
            {
                resources.ApplyResources(control, control.Name);
                MenuStrip ms = (MenuStrip)control;
                if (ms.Items.Count > 0)
                {
                    foreach (ToolStripMenuItem c in ms.Items)
                    {
                        //Traverse the menu
                        Loading(c, resources);
                    }
                }
            }

            foreach (Control c in control.Controls)
            {
                resources.ApplyResources(c, c.Name);
                Loading(c, resources);
            }
        }

        /// <summary>
        /// Traverse the menu
        /// </summary>
        /// <param name="item">Menu item</param>
        /// <param name="resources">Language resources</param>
        private static void Loading(ToolStripMenuItem item, System.ComponentModel.ComponentResourceManager resources)
        {
            if (item is ToolStripMenuItem)
            {
                resources.ApplyResources(item, item.Name);
                ToolStripMenuItem tsmi = (ToolStripMenuItem)item;
                if (tsmi.DropDownItems.Count > 0)
                {
                    foreach (ToolStripMenuItem c in tsmi.DropDownItems)
                    {
                        Loading(c, resources);
                    }
                }
            }
        }
    }
}
