using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace LostPolygon.MethodInlineInjector.ConsoleApp {
    internal sealed class CommandLineOptions {
        [Option(
            'f',
            "file",
            HelpText = "Read injector configuration from a file.",
            Default = null,
            SetName = "Source"
            )]
        public string ConfigurationFilePath { get; private set; }

        [Option(
            's',
            "stdin",
            HelpText = "Read injector configuration from standard input.",
            Default = false,
            SetName = "Source"
        )]
        public bool ReadConfigurationFromStandardInput { get; private set; }

        [Option(
            'l',
            "loglevel",
            HelpText =
            "Logging verbosity. (Available values: " +
            nameof(LogThresholdLevel.Debug) + ", " +
            nameof(LogThresholdLevel.Info) + ", " +
            nameof(LogThresholdLevel.Warning) + ", " +
            nameof(LogThresholdLevel.Error) + ", " +
            nameof(LogThresholdLevel.Fatal) +
            ")",
            Default = LogThresholdLevel.Info
        )]
        public LogThresholdLevel LogLevel { get; private set; }

        public enum LogThresholdLevel {
            All,
            Debug,
            Info,
            Warning,
            Error,
            Fatal
        }

        [Usage(ApplicationAlias = "LostPolygon.MethodInlineInjector.Console")]
        public static IEnumerable<Example> Examples {
            get {
                yield return new Example(
                    "From standard input",
                    new CommandLineOptions { ReadConfigurationFromStandardInput = true });

                yield return new Example(
                    "From file",
                    new CommandLineOptions { ConfigurationFilePath = "myInjectorConfig.xml"});
            }
        }
    }
}
