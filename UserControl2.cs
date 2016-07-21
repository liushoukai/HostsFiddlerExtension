using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Fiddler;

namespace TestFiddlerExtension
{
    public partial class UserControl2 : UserControl
    {
        public UserControl2()
        {
            InitializeComponent();
            string filePath1 = @"F:\\dev.config";
            this.textBox1.Text = loadConfigFile(filePath1);
            string filePath2 = @"F:\\pre.config";
            this.textBox2.Text = loadConfigFile(filePath2);
            string filePath3 = @"F:\\pro.config";
            this.textBox3.Text = loadConfigFile(filePath3);
        }

        private string loadConfigFile(string filePath)
        {

            FiddlerApplication.Log.LogString("Load hosts config file: " + filePath);

            // 判断配置文件是否存在，不存在则创建
            if (!File.Exists(filePath))
            {
                FileStream fs = File.Create(filePath);
                fs.Close();
            }

            // 获取配置文件内容
            StreamReader sr = new StreamReader(filePath, Encoding.UTF8);
            string content = "";
            while (!sr.EndOfStream)
            {
                content += (sr.ReadLine() + "\r\n");
            }
            sr.Close();
            return content;
        }

        private void tbTextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            string filePath = "";
            FiddlerApplication.Log.LogString(tb.Name);
            switch (tb.Name)
            {
                case "textBox1":
                    filePath = @"F:\\dev.config"; break;
                case "textBox2":
                    filePath = @"F:\\pre.config"; break;
                case "textBox3":
                    filePath = @"F:\\pro.config"; break;
            }
            StreamWriter sw = new StreamWriter(filePath, false, Encoding.UTF8);
            sw.Write(tb.Text);
            sw.Close();
            FiddlerApplication.Log.LogString(tb.Text);
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
