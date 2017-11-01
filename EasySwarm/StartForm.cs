using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CCWin;
using System.Xml;

namespace EasySwarm
{
    public partial class StartForm : CCSkinMain
    {
        public static bool ENorCH;
        public StartForm()
        {
            InitializeComponent();
        }

        private void StartForm_Load(object sender, EventArgs e)
        {
        }
        /////////////////////
        //Read the default language 
        public static string ReadDefaultLanguage()
        {
            XmlReader reader = new XmlTextReader("resources/LanguageDefine.xml");
            XmlDocument doc = new XmlDocument();
            doc.Load(reader);
            XmlNode root = doc.DocumentElement;
            //Select the DefaultLangugae node
            XmlNode node = root.SelectSingleNode("DefaultLanguage");

            string result = "EN";
            if (node != null)
                result = node.InnerText;

            reader.Close();
            return result;
        }
        //Modify the default language 
        public static void WriteDefaultLanguage(string lang)
        {
            DataSet ds = new DataSet();
            ds.ReadXml("resources/LanguageDefine.xml");
            DataTable dt = ds.Tables["Language"];

            dt.Rows[0]["DefaultLanguage"] = lang;
            ds.AcceptChanges();
            ds.WriteXml("resources/LanguageDefine.xml");
        }

        private void btn_ch_Click(object sender, EventArgs e)
        {
            ENorCH = false;
            MultiLanguage.SetDefaultLanguage("zh-CN");
            Easyswarm easyswarm = new Easyswarm();
            easyswarm.Show();
            this.Hide();
        }

        private void btn_en_Click(object sender, EventArgs e)
        {
            ENorCH = true;
            MultiLanguage.SetDefaultLanguage("en-US");
            Easyswarm easyswarm = new Easyswarm();
            easyswarm.Show();
            this.Hide();
        }
    }
}
