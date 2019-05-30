using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DialogicWrapperSync;

namespace ConsoleSipSync
{
    class SIPThread
    {

         /*
         * Commands
         */
        public const string CLI_HELP_MSG = "HMP SIP Please enter the command: \n" +
        "'{0}' for make call, \n" +
        "'{1}' for drop call, \n" +
        "'{2}' for stop all, \n" +
        "'{3}' for registration, "+
        "'{4}' for un-registration, \n" +
        "'{5}' for print this help message, " +
        "'{6}' for print system status, \n" +
        "'{7}' for quit this test.\n\n";

        public static string PRINT_CLI_HELP_MSG = String.Format(CLI_HELP_MSG, CLI_MAKECALL, CLI_DROPCALL, CLI_STOP, CLI_REGISTER, CLI_UNREGISTER, CLI_HELP, CLI_STAT, CLI_QUIT);

        public const string CLI_HELP = "help";
        public const string CLI_QUIT = "quit";
        public const string CLI_OPENCHANNEL = "op";
        public const string CLI_CLOSECHANNEL = "cl";
        public const string CLI_MAKECALL = "mc";
        public const string CLI_WAITCALL = "wc";
        public const string CLI_DROPCALL = "dc";

        public const string CLI_STOP = "stop";

        public const string CLI_REGISTER = "reg";
        public const string CLI_UNREGISTER = "unreg";

        public const string CLI_STAT = "stat";
        /*
         * Command Setup
         * 
         */
        public const string CLI_REQ_INDEX = "index(1-{0},{1}): ";
        public const int CLI_REQ_INDEX_DEFAULT = 1;
        public const string CLI_REQ_ANI = "ani({0}): ";
        //public const string CLI_REQ_ANI_DEFAULT = "5555555555@10.143.102.42";
        public const string CLI_REQ_ANI_DEFAULT = "Developer1@10.143.102.42";
        public const string CLI_REQ_DNIS = "dnis({0}): ";
        public const string CLI_REQ_DNIS_DEFAULT = "7782320255@10.143.102.42";
        //Marko's phone number#define CLI_REQ_DNIS_DEFAULT		=	"6047641484@10.143.102.42";
        public const string CLI_REQ_WAVE_FILE = "wave file({0}): ";
        public const string CLI_REQ_WAVE_FILE_DEFAULT = "play.wav";
        public const string CLI_REQ_FAX_FILE = "fax file({0}): ";
        public const string CLI_REQ_FAX_FILE_DEFAULT = "fax.tif";
        public const string CLI_REQ_CONFIRM = "confirm?({0}): ";
        public const string CLI_REQ_CONFIRM_DEFAULT = "Y";
        public const string CLI_REQ_PROXY_IP = "proxy({0}): ";
        public const string CLI_REQ_PROXY_IP_DEFAULT = "10.143.102.42";
        public const string CLI_REQ_LOCAL_IP = "local({0}): ";
        public const string CLI_REQ_LOCAL_IP_DEFAULT = "10.143.102.220";
        public const string CLI_REQ_ALIAS = "alias({0}): ";
        //public const string CLI_REQ_ALIAS_DEFAULT = "5555555555";
        public const string CLI_REQ_ALIAS_DEFAULT = "Developer1";
        public const string CLI_REQ_PASSWORD = "password({0}): ";
        public const string CLI_REQ_PASSWORD_DEFAULT = "password";
        public const string CLI_REQ_REALM = "realm({0}): ";
        public const string CLI_REQ_REALM_DEFAULT = "";

        /*
         * Setup Variables
         * 
         */
        private const int MAX_CHANNELS = 2;
        private const string USER_DISPLAY = "Michael Cox";
        private const string USER_AGENT = "SRB_SIP_CLIENT";
        private const int HMP_SIP_PORT = 5060;
        private static DialogicSIPSync sip = new DialogicSIPSync();

        public void CommandLineInput()
        {

            sip = new DialogicSIPSync();
            //sip.WStartLibraries();



            CLIPrintHelp();

            bool exitLoop = false;
            while (!exitLoop)
            {

                string line = Console.ReadLine();

                switch (line)
                {
                    case CLI_HELP:
                        CLIPrintHelp();
                        break;
                    case CLI_OPENCHANNEL:
                        CLIOpenChannel();
                        break;
                    case CLI_CLOSECHANNEL:
                        CLICloseChannel();
                        break;
                    case CLI_QUIT:
                        sip.WClose();
                        sip.WStartLibraries(1720,5060);
                        exitLoop = true;
                        break;
                    case CLI_MAKECALL:
                        CLIMakeCall();
                        break;
                    case CLI_WAITCALL:
                        CLIWaitCall();
                        break;
                    case CLI_DROPCALL:
                        CLIDropCall();
                        break;
                    case CLI_STOP:
                        CLIStop();
                        break;
                    case CLI_REGISTER:
                        CLIRegister();
                        break;
                    case CLI_UNREGISTER:
                        CLIUnregister();
                        break;
                    case CLI_STAT:
                        CLIStatus();
                        break;
                    default:
                        Console.WriteLine("Invalid Command");
                        break;
                }
            }
        }

        static void CLIPrintHelp()
        {
            Console.WriteLine(PRINT_CLI_HELP_MSG);
        }

        public void CLIOpenChannel()
        {
            int ChannelIndex = 0;
            // Get the channel to use for the make call.
            Console.Write(CLI_REQ_INDEX, MAX_CHANNELS, CLI_REQ_INDEX_DEFAULT);
            string readChannel = Console.ReadLine();
            if (!readChannel.Trim().Equals(""))
            {
                ChannelIndex = Convert.ToInt32(readChannel);
            }

            sip.WOpen(ChannelIndex);
        }

        static void CLICloseChannel()
        {
            int ChannelIndex = 0;
            // Get the channel to use for the make call.
            Console.Write(CLI_REQ_INDEX, MAX_CHANNELS, CLI_REQ_INDEX_DEFAULT);
            string readChannel = Console.ReadLine();
            if (!readChannel.Trim().Equals(""))
            {
                ChannelIndex = Convert.ToInt32(readChannel);
            }

            sip.WClose();
        }

        static void CLIMakeCall()
        {
            //Setup all the defaults
            int ChannelIndex = CLI_REQ_INDEX_DEFAULT;
            string ani = CLI_REQ_ANI_DEFAULT;
            string dnis = CLI_REQ_DNIS_DEFAULT;

            // Get the channel to use for the make call.
            Console.Write(CLI_REQ_INDEX, MAX_CHANNELS, CLI_REQ_INDEX_DEFAULT);
            string line = Console.ReadLine();
            if (!line.Trim().Equals(""))
            {
                ChannelIndex = Convert.ToInt32(line);
            }

            //Get the ani to use for the call
            Console.Write(CLI_REQ_ANI, CLI_REQ_ANI_DEFAULT);
            line = Console.ReadLine();
            if (!line.Trim().Equals(""))
            {
                ani = line;
            }

            //Get the dnis to use for the call
            Console.Write(CLI_REQ_DNIS, CLI_REQ_DNIS_DEFAULT);
            line = Console.ReadLine();
            if (!line.Trim().Equals(""))
            {
                dnis = line;
            }

            //Debug to console.  Its dirty but I will take it out of the final codebase.
            //Console.WriteLine(ChannelIndex);
            //Console.WriteLine(ani);
            //Console.WriteLine(dnis);

            //make the call
            int return_value = sip.WMakeCall(ani, dnis);
            Console.WriteLine("Call Progress Analysis Result: {0} ", return_value);
        }

        static void CLIWaitCall()
        {

            int ChannelIndex = CLI_REQ_INDEX_DEFAULT;

            Console.Write(CLI_REQ_INDEX, MAX_CHANNELS, CLI_REQ_INDEX_DEFAULT);
            string line = Console.ReadLine(); // Get string from user
            if (!line.Trim().Equals(""))
            {
                ChannelIndex = Convert.ToInt32(line);
            }
            sip.WWaitCallAsync();

            // -2 is expired
            while (sip.WWaitForCallEventSync(50) == -2) // wait 5 seconds
            {
            }
        }

        static void CLIDropCall()
        {

            int ChannelIndex = CLI_REQ_INDEX_DEFAULT;

            Console.Write(CLI_REQ_INDEX, MAX_CHANNELS, CLI_REQ_INDEX_DEFAULT);
            string line = Console.ReadLine(); // Get string from user
            if (!line.Trim().Equals(""))
            {
                ChannelIndex = Convert.ToInt32(line);
            }
            sip.WDropCall();
        }

        static void CLIStop()
        {
            int ChannelIndex = CLI_REQ_INDEX_DEFAULT;

            Console.Write(CLI_REQ_INDEX, MAX_CHANNELS, CLI_REQ_INDEX_DEFAULT);
            string line = Console.ReadLine();
            if (!line.Trim().Equals(""))
            {
                ChannelIndex = Convert.ToInt32(line);
            }
            sip.WStop();
        }

        static void CLIRegister()
        {
            string ProxyIp = CLI_REQ_PROXY_IP_DEFAULT;
            string LocalIp = CLI_REQ_PROXY_IP_DEFAULT;
            string Alias = CLI_REQ_ALIAS_DEFAULT;
            string Password = CLI_REQ_PASSWORD_DEFAULT;
            string Realm = CLI_REQ_REALM_DEFAULT;

            //Get the proxy ip for registration
            Console.Write(CLI_REQ_PROXY_IP, CLI_REQ_PROXY_IP_DEFAULT);
            string line = Console.ReadLine();
            if (!line.Trim().Equals(""))
            {
                ProxyIp = line;
            }

            //Get the local ip for registration
            Console.Write(CLI_REQ_LOCAL_IP, CLI_REQ_LOCAL_IP_DEFAULT);
            line = Console.ReadLine();
            if (!line.Trim().Equals(""))
            {
                LocalIp = line;
            }

            //Get the alias for registration
            Console.Write(CLI_REQ_ALIAS, CLI_REQ_ALIAS_DEFAULT);
            line = Console.ReadLine();
            if (!line.Trim().Equals(""))
            {
                Alias = line;
            }

            //Get the password for registration
            Console.Write(CLI_REQ_PASSWORD, CLI_REQ_PASSWORD_DEFAULT);
            line = Console.ReadLine();
            if (!line.Trim().Equals(""))
            {
                Password = line;
            }

            //Get the realm for registration
            Console.Write(CLI_REQ_REALM, CLI_REQ_REALM_DEFAULT);
            line = Console.ReadLine();
            if (!line.Trim().Equals(""))
            {
                Realm = line;
            }

            sip.WRegister(ProxyIp, LocalIp, Alias, Password, Realm);
        }

        static void CLIUnregister()
        {
            sip.WUnregister();
        }

        static void CLIStatus()
        {
            sip.WStatus();
            Console.WriteLine("##################");
            String name = sip.WGetDeviceName();
            Console.WriteLine(name);
            String name2 = sip.WGetDeviceName();
            Console.WriteLine(name2);

        }
    }
}
