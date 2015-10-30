using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ivrToolkit.Core.Util
{
    public class TenantSingleton
    {
        private static TenantSingleton instance;
        private string _tenantDirectory;
        private string _tenant = "";

        private TenantSingleton() {

            String[] args = Environment.GetCommandLineArgs();

            foreach (string s in args)
            {
                if (s.StartsWith("-t"))
                {
                    _tenant = s.Substring(2);
                }
            }

            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            location = Path.GetDirectoryName(location);
            if (location == null) throw new Exception("Executable path not found");

            if ((_tenant == null) || (_tenant.Trim().Equals("")))
            {
                _tenantDirectory = location;
            }
            else
            {
                _tenantDirectory = Path.Combine(location, _tenant);
            }
        }

        public static TenantSingleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TenantSingleton();
                }
                return instance;
            }
        }

        public string TenantDirectory
        {
            get
            {
                return _tenantDirectory;
            }
        }

        public string Tenant {
            get
            {
                return _tenant;
            }
        }
    }
}
