using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Fiddler;
using System.Web.Script.Serialization;
using System.Collections;
using System.Reflection;
using System.ComponentModel;

[assembly: Fiddler.RequiredVersion("2.2.8.6")]

namespace FiddlerExtension
{
    public class HostsFiddlerExtension : Fiddler.IAutoTamper, IHandleExecAction, IFiddlerExtension
    {
        // Who
        MenuItem oMenuWho = new MenuItem("Who");
        MenuItem oMenuWhoSub1 = new MenuItem("webapp");
        MenuItem oMenuWhoSub2 = new MenuItem("liushoukai");
        MenuItem oMenuWhoSub3 = new MenuItem("Disabled");
        public string who = "webapp";

        // Hosts
        MenuItem oMenuHosts = new MenuItem("Hosts");
        MenuItem oMenuHostsSub1 = new MenuItem("Development");  // Development Environment
        MenuItem oMenuHostsSub2 = new MenuItem("Pre-Release");  // Pre-Release Environment
        MenuItem oMenuHostsSub3 = new MenuItem("Production");   // Production  Environment
        MenuItem oMenuHostsSub4 = new MenuItem("Disabled");
        public string env = "Development";

        public HostsGridView oHostsGridView;

        public HostsFiddlerExtension() {
                    
            // config file path
            string hostsConfigPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "hostsConfig.json");

            // If config file not exists, create a demo config file
            FiddlerApplication.Log.LogString(hostsConfigPath);
            if (!File.Exists(hostsConfigPath)) {
                FileStream fs = new FileStream(hostsConfigPath, FileMode.Create, FileAccess.Write, FileShare.Read);
                List<HostsConfig> configs = new List<HostsConfig>();

                HostsConfig config = new HostsConfig("Development", false, new Dictionary<string, string>() { { "tieba.duowan.com", "192.168.56.101" } });
                configs.Add(config);

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string json = serializer.Serialize(configs);
                fs.Write(Encoding.UTF8.GetBytes(json), 0, Encoding.UTF8.GetBytes(json).Length);
                fs.Flush(true);
                fs.Close();
            }

            // Init Env Menu
            oMenuHostsSub1.RadioCheck = true;
            oMenuHostsSub2.RadioCheck = true;
            oMenuHostsSub3.RadioCheck = true;
            oMenuHostsSub4.RadioCheck = true;
            oMenuHostsSub1.Checked = true;
            oMenuHosts.MenuItems.AddRange(new MenuItem[] { oMenuHostsSub1, oMenuHostsSub2, oMenuHostsSub3, new MenuItem("-"), oMenuHostsSub4 });

            // Init Who Menu
            oMenuWhoSub1.RadioCheck = true;
            oMenuWhoSub2.RadioCheck = true;
            oMenuWhoSub3.RadioCheck = true;
            oMenuWhoSub1.Checked = true;
            oMenuWho.MenuItems.AddRange(new MenuItem[] { oMenuWhoSub1, oMenuWhoSub2, new MenuItem("-"), oMenuWhoSub3 });

            this.oHostsGridView = new HostsGridView();
        }

        public void AutoTamperRequestBefore(Session oSession) {

            // Switch Hosts
            BindingList<HostsConfig> list = this.oHostsGridView.configs;
            foreach (HostsConfig hostConfig in list) {
                if (hostConfig.Check) {
                    foreach (var item in hostConfig.Items) {
                        string domain = item.Key;
                        string ip = item.Value;
                        if (oSession.HostnameIs(domain)) {
                            oSession["x-overridehost"] = ip;
                        }
                    }
                }
            }

            // Add Custom Headers
            if (!String.IsNullOrEmpty(this.who)) {
                oSession.oRequest["who"] = who;
            }
        }

        public void AutoTamperRequestAfter(Session oSession) {}
        public void AutoTamperResponseBefore(Session oSession) {}
        public void AutoTamperResponseAfter(Session oSession) {}
        public void OnBeforeReturningError(Session oSession) {}
        public void OnBeforeUnload() {}

        public bool OnExecAction(string sCommand) {
            string[] args = Fiddler.Utilities.Parameterize(sCommand);

            string command = args[0];

            if (command.ToLower() == "removehost")
            {

                if (args == null || args.Length != 2)
                {
                    FiddlerApplication.UI.SetStatusText("Specify host to remove");
                    return false;
                }

                string host = args[1];

                FiddlerApplication.UI.actSelectSessionsWithRequestHeaderValue("Host", host);
                FiddlerApplication.UI.actRemoveSelectedSessions();
            }

            return true;
        }

        public static string FillMethodColumnRequestMethod(Session oSession) {
            return oSession.RequestMethod;
        }

        public static string FillMethodColumnHostIP(Session oSession) {
            return oSession.m_hostIP;
        }

        public static string FillMethodColumnWho(Session oSession) {
            HTTPHeaderItem[] headers = oSession.oRequest.headers.ToArray();
            foreach (HTTPHeaderItem header in headers) {
                string[] items = header.ToString().Split(':');
                if (items.Length >= 2 && "who".Equals(items[0].Trim())) {
                    return items[1];
                }
            }
            return "";
        }

        public string FillMethodColumnEnv(Session oSession) {
            switch (env) {
                case "Development":
                    return "Development";
                case "Pre-Release":
                    return "Pre-Release";
                case "Production":
                    return "Production";
                default:
                    return "";
            }      
        }

        public void OnLoad() {
            // Event Binding for Hosts Menu
            foreach (MenuItem son in oMenuHosts.MenuItems)
            {
                son.Click += (sender, e) => {
                    MenuItem mi = sender as MenuItem;
                    this.env = mi.Text;

                    oMenuHostsSub1.Checked = false;
                    oMenuHostsSub2.Checked = false;
                    oMenuHostsSub3.Checked = false;
                    oMenuHostsSub4.Checked = false;
                    mi.Checked = true;

                    FiddlerApplication.Log.LogString(env);
                };
            }

            // Event Binding for Who Menu
            foreach (MenuItem son in oMenuWho.MenuItems)
            {
                son.Click += (sender, e) => {
                    MenuItem mi = sender as MenuItem;
                    this.who = mi.Text;

                    oMenuWhoSub1.Checked = false;
                    oMenuWhoSub2.Checked = false;
                    oMenuWhoSub3.Checked = false;
                    mi.Checked = true;

                    FiddlerApplication.Log.LogString(env);
                };
            }

            // Add Menus
            FiddlerApplication.UI.mnuRules.MenuItems.Add(oMenuHosts);
            FiddlerApplication.UI.mnuRules.MenuItems.Add(oMenuWho);

            // Add Tab
            TabPage hostsTabPage = new TabPage("SwitchHosts");
            hostsTabPage.Controls.Add(this.oHostsGridView);
            FiddlerApplication.UI.tabsViews.TabPages.Add(hostsTabPage);

            // Add Session Columns
            FiddlerApplication.UI.lvSessions.AddBoundColumn("Method", 1, 50, new Fiddler.getColumnStringDelegate(FillMethodColumnRequestMethod));
            FiddlerApplication.UI.lvSessions.AddBoundColumn("HostIP", 1, 100, new Fiddler.getColumnStringDelegate(FillMethodColumnHostIP));
            FiddlerApplication.UI.lvSessions.AddBoundColumn("Who", 1, 80, new Fiddler.getColumnStringDelegate(FillMethodColumnWho));
            FiddlerApplication.UI.lvSessions.AddBoundColumn("Environment", 1, 80, new Fiddler.getColumnStringDelegate(FillMethodColumnEnv));
        }

    }
}
