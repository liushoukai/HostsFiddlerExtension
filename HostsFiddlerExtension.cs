using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Fiddler;

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

        public HostsTabView oHostsTabView;

        public HostsFiddlerExtension() {
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
            oHostsTabView = new HostsTabView();
        }

        public void AutoTamperRequestBefore(Session oSession) {

            // Override Hosts
            if (!String.IsNullOrEmpty(this.env))
            {
                string filePath = @"F:\\dev.config";

                switch (this.env)
                {
                    case "Development":
                        filePath = @"F:\\dev.config"; break;
                    case "Pre-Release":
                        filePath = @"F:\\pre.config"; break;
                    case "Production":
                        filePath = @"F:\\pro.config"; break;
                }

                StreamReader sr = new StreamReader(filePath);
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string ip = line.Trim().Split(' ')[0];
                    string domain = line.Trim().Split(' ')[1];
                    if (oSession.HostnameIs(domain))
                    {
                        oSession["x-overridehost"] = ip;
                    }
                    FiddlerApplication.Log.LogString(ip + "~~" + domain);
                }
                sr.Close();
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

            TabPage envTabPage = new TabPage("Hosts");
            envTabPage.Controls.Add(this.oHostsTabView);
            FiddlerApplication.UI.tabsViews.TabPages.Add(envTabPage);

            // Add Session Columns
            FiddlerApplication.UI.lvSessions.AddBoundColumn("Method", 3, 50, new Fiddler.getColumnStringDelegate(FillMethodColumnRequestMethod));
            FiddlerApplication.UI.lvSessions.AddBoundColumn("HostIP", 2, 100, new Fiddler.getColumnStringDelegate(FillMethodColumnHostIP));
            FiddlerApplication.UI.lvSessions.AddBoundColumn("Who", 1, 80, new Fiddler.getColumnStringDelegate(FillMethodColumnWho));
            FiddlerApplication.UI.lvSessions.AddBoundColumn("Environment", 1, 80, new Fiddler.getColumnStringDelegate(FillMethodColumnEnv));
        }

    }
}
