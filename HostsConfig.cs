using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FiddlerExtension
{
    public class HostsConfig
    {
        private string name;
        private bool check;
        private Dictionary<string, string> items = new Dictionary<string, string>();

        public HostsConfig() { }

        public HostsConfig(string name, bool check, Dictionary<String, String> items) {
            this.name = name;
            this.check = check;
            this.items = items;
        }

        public void addItem(string hosts, string ip) {
            this.items.Add(hosts, ip);
        }

        public bool removeItem(string hosts) {
            return this.items.Remove(hosts);
        }

        public bool Check
        {
            get
            {
                return check;
            }

            set
            {
                check = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public Dictionary<string, string> Items
        {
            get
            {
                return items;
            }

            set
            {
                items = value;
            }
        }

    }
}
