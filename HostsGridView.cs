using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Fiddler;
using System.IO;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using System.Reflection;

namespace FiddlerExtension
{
    public partial class HostsGridView : UserControl
    {
        public BindingList<HostsConfig> configs;
        public bool changed = false;

        public HostsGridView()
        {
            InitializeComponent();

            // 配置文件绝对路径
            string dllLocation = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string scriptDir = System.IO.Path.GetDirectoryName(dllLocation);
            string hostsConfigPath = Path.Combine(scriptDir, "hostsConfig.json");

            // 反序列化生成配置文件
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            FileStream fs = new FileStream(hostsConfigPath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
            StreamReader sr = new StreamReader(fs);
            string json = sr.ReadToEnd();
            sr.Close();
            fs.Close();
            FiddlerApplication.Log.LogString(json);
            this.configs = serializer.Deserialize<BindingList<HostsConfig>>(json);
            this.dataGridView1.DataSource = configs;
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.Columns["Items"].Visible = false;
            this.dataGridView1.Columns["Check"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            this.dataGridView1.Columns["Name"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Add new line
            HostsConfig hc = new HostsConfig();
            hc.Check = false;
            hc.Name = "";
            Dictionary<string, string> items = new Dictionary<string, string>();
            hc.Items = items;
            this.configs.Add(hc);
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dataGridView1_DataSourceChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {

        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            // 在选中行变化时，将选中行的hosts信息输出到textBox
            int rowIdx = this.dataGridView1.CurrentRow.Index;
            DataGridViewRow row = this.dataGridView1.Rows[rowIdx];
            Dictionary<string, string> items = (Dictionary<string, string>)row.Cells["Items"].Value;
            string info = "";
            foreach (var item in items)
            {
                info += item.Value + " " + item.Key + Environment.NewLine;
            }
            this.textBox1.Text = info;
            FiddlerApplication.Log.LogString("select changed");
          
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            DataGridViewRow row = this.dataGridView1.CurrentRow;
            string name = (string)row.Cells["Name"].Value;
            string input = this.textBox1.Text;

            FiddlerApplication.Log.LogString("text changed");

            //根据当前的选中的行，获取对应的数据对象，用文本域中的内容重构
            Regex regex = new Regex(@"^([0-2]?\d?\d(?:\.[0-2]?\d?\d){3})\s+([0-9a-zA-Z\.\-_]+)");
            foreach (HostsConfig item in this.configs)
            {
                if (item.Name.Equals(name) && !String.IsNullOrWhiteSpace(input)) {
                    string[] lines = this.textBox1.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                    Dictionary<string, string> data = new Dictionary<string, string>();
                    foreach (string line in lines)
                    {
                        Match match = regex.Match(line);
                        if (!match.Success) continue;
                        string ip = match.Groups[1].ToString();
                        string domain = match.Groups[2].ToString();
                        if (!data.ContainsKey(domain)) {
                            data.Add(domain, ip);
                        }
                    }
                    item.Items = data;
                }
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(this.configs);
            FiddlerApplication.Log.LogString(json);

            // 将数据写入配置文件
            string hostsConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "hostsConfig.json");
            using (FileStream fs = new FileStream(hostsConfigPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                fs.Write(Encoding.UTF8.GetBytes(json), 0, Encoding.UTF8.GetBytes(json).Length);
                fs.Flush();
                fs.Close();
            }
            FiddlerApplication.Log.LogString("json content: " + json);

        }

        private void hostsConfigBindingSource_CurrentChanged(object sender, EventArgs e)
        {

        }

        // exit edit mode after edit a checkbox
        private void dataGridView1_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == 0 && e.RowIndex > -1) {
                this.dataGridView1.EndEdit();
            }
        }

        // Del line
        private void btnDelClick(object sender, EventArgs e)
        {
            DataGridViewRow curRow = this.dataGridView1.CurrentRow;
            //MessageBox.Show(curRow.Index + "");
            this.configs.RemoveAt(curRow.Index);
        }

        // Add line
        private void btnAddClick(object sender, EventArgs e)
        {
            HostsConfig hc = new HostsConfig();
            hc.Check = false;
            hc.Name = "";
            Dictionary<string, string> items = new Dictionary<string, string>();
            hc.Items = items;
            this.configs.Add(hc);
        }
    }
}
