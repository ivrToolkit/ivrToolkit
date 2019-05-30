using System;
using System.IO;
using NLog;

namespace ivrToolkit.Core.Util
{
    public class TenantSingleton
    {
        private static TenantSingleton _instance;
        private readonly string _tenantDirectory;
        private readonly string _tenant = "";

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private TenantSingleton() {
            Logger.Debug("Starting TenantSingleton");
            String[] args = Environment.GetCommandLineArgs();
            Logger.Debug($"There are {args.Length} args");

            foreach (string s in args)
            {
                if (s.StartsWith("-t"))
                {
                    _tenant = s.Substring(2);
                    Logger.Debug($"Tenant name is {_tenant}");
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
            Logger.Debug($"Tenant directory = {_tenantDirectory}");
        }

        public static TenantSingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TenantSingleton();
                }
                return _instance;
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
