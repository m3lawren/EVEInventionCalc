using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using log4net;
using log4net.Config;
using log4net.Util;

namespace EVEInventionCalc.Logging
{
    public static class Logger
    {

        private static readonly ILog logValue = log4net.LogManager.GetLogger(typeof(Logger));

        static Logger()
        {
            XmlConfigurator.Configure();
            GlobalContext.Properties["MachineName"] = Environment.MachineName;
            GlobalContext.Properties["ProcessName"] = Process.GetCurrentProcess().ProcessName;
            GlobalContext.Properties["ProcessID"] = Process.GetCurrentProcess().Id.ToString();
            GlobalContext.Properties["ProcessArguments"] = string.Join(" ", Environment.GetCommandLineArgs().Skip(1).ToArray());
            GlobalContext.Properties["CurrentDirectory"] = Environment.CurrentDirectory;
        }

        public static ThreadContextStacks Level
        {
            get { return ThreadContext.Stacks; }
        }

        public static ILog Log
        {
            get { return logValue; }
        }
    }
}
