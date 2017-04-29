using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Xml;
using log4net;
using log4net.Config;

namespace LostPolygon.MethodInlineInjector.Cli {
    internal class Program {
        private static readonly ILog Log = LogManager.GetLogger("ConsoleInjector");

        public static void Main(params string[] args) {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            Console.OutputEncoding = Encoding.Unicode;
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            SetupLog4Net();

            ConsoleInjector.Run(args);
        }

        private static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e) {
            Log.Fatal(
                "Fatal error:" + Environment.NewLine +
                ((Exception) e.ExceptionObject) + Environment.NewLine +
                ((Exception) e.ExceptionObject).InnerException
            );
            Environment.Exit(1);
        }

        private static void SetupLog4Net() {
            XmlDocument objDocument = new XmlDocument();
            objDocument.LoadXml(Resources.log4netConfiguration);
            XmlElement objElement = objDocument.DocumentElement;

            XmlConfigurator.Configure(objElement);
        }
    }
}
