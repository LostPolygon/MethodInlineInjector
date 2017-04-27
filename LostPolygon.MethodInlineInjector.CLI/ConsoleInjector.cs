using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using CommandLine;
using CommandLine.Text;
using log4net;
using log4net.Core;
using LostPolygon.Common.SimpleXmlSerialization;

namespace LostPolygon.MethodInlineInjector.Cli {
    internal class ConsoleInjector {
        private static readonly ILog Log = LogManager.GetLogger("ConsoleInjector");

        private readonly string[] _args;
        private CommandLineOptions _commandLineOptions;
        private ParserResult<CommandLineOptions> _commandLineParserResult;

        public static void Run(string[] args) {
            ConsoleInjector consoleInjector = new ConsoleInjector(args);
            consoleInjector.Run();
        }

        private ConsoleInjector(string[] args) {
            _args = args;
        }

        private void Run() {
            Parser commandLineParser =
                new Parser(settings => {
                    settings.IgnoreUnknownArguments = false;
                    settings.CaseInsensitiveEnumValues = true;
                    settings.CaseSensitive = false;
                    settings.MaximumDisplayWidth = 100;
                    settings.HelpWriter = null;
                });

            _commandLineParserResult = commandLineParser.ParseArguments<CommandLineOptions>(_args);
            _commandLineParserResult
                .WithParsed(options => {
                    _commandLineOptions = options;
                    if (!ValidateCommandLineOptions(options)) {
                        Environment.ExitCode = 1;
                        return;
                    }

                    Level threshold;
                    switch (_commandLineOptions.LogLevel) {
                        case CommandLineOptions.LogThresholdLevel.All:
                            threshold = Level.All;
                            break;
                        case CommandLineOptions.LogThresholdLevel.Debug:
                            threshold = Level.Debug;
                            break;
                        case CommandLineOptions.LogThresholdLevel.Info:
                            threshold = Level.Info;
                            break;
                        case CommandLineOptions.LogThresholdLevel.Warning:
                            threshold = Level.Warn;
                            break;
                        case CommandLineOptions.LogThresholdLevel.Error:
                            threshold = Level.Error;
                            break;
                        case CommandLineOptions.LogThresholdLevel.Fatal:
                            threshold = Level.Fatal;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    LogManager.GetAllRepositories().ToList().ForEach(repository => repository.Threshold = threshold);

                    ExecuteInjection();
                })
                .WithNotParsed(errors => {
                    Console.WriteLine(GetHelpText());
                    Environment.ExitCode = 1;
                });
        }

        private void ExecuteInjection() {
            try {
                string serializedInjectorConfiguration;
                if (_commandLineOptions.ReadConfigurationFromStandardInput) {
                    serializedInjectorConfiguration = Console.In.ReadToEnd();
                } else {
                    if (!File.Exists(_commandLineOptions.ConfigurationFilePath)) {
                        throw new MethodInlineInjectorException(
                            "Injection configuration file doesn't exists",
                            new FileNotFoundException(_commandLineOptions.ConfigurationFilePath)
                        );
                    }

                    serializedInjectorConfiguration = File.ReadAllText(_commandLineOptions.ConfigurationFilePath);
                }

                if (String.IsNullOrWhiteSpace(serializedInjectorConfiguration))
                    throw new MethodInlineInjectorException("Injector configuration is empty");

                Log.Info("Parsing configuration file");
                ValidateConfiguration(serializedInjectorConfiguration);
                InjectionConfiguration injectionConfiguration =
                    SimpleXmlSerializationUtility.XmlDeserializeFromString<InjectionConfiguration>(serializedInjectorConfiguration);

                Log.Info("Resolving configuration file");
                ResolvedInjectionConfiguration resolvedInjectionConfiguration =
                    ResolvedInjectionConfigurationLoader.LoadFromInjectionConfiguration(injectionConfiguration);

                Log.Info("Starting injection");
                MethodInlineInjector assemblyMethodInjector = new MethodInlineInjector(resolvedInjectionConfiguration);

                int injectedMethodCount = 0;
                assemblyMethodInjector.BeforeMethodInjected += tuple => injectedMethodCount++;
                assemblyMethodInjector.Inject();

                Log.InfoFormat("Injected {0} methods", injectedMethodCount);

                Log.Info("Writing modified assemblies");
                foreach (ResolvedInjecteeAssembly injecteeAssembly in resolvedInjectionConfiguration.InjecteeAssemblies) {
                    injecteeAssembly.AssemblyDefinition.Write(injecteeAssembly.AssemblyDefinition.MainModule.FullyQualifiedName);
                }
            } catch (MethodInlineInjectorException e) {
                string message = "Fatal error: " + e.Message;
                if (e.InnerException != null) {
                    message += Environment.NewLine;
                    message += "Error details: ";
                    message += e.InnerException.Message;
                }
                Log.Fatal(message);
                Environment.ExitCode = 1;
            }
        }

        private bool ValidateCommandLineOptions(CommandLineOptions options) {
            if (!options.ReadConfigurationFromStandardInput && string.IsNullOrWhiteSpace(options.ConfigurationFilePath)) {
                ShowHelpTextWithCustomErrorMessage("  at least one of --file or --stdin options must be specified.");
                return false;
            }

            if (options.ReadConfigurationFromStandardInput && !string.IsNullOrWhiteSpace(options.ConfigurationFilePath)) {
                ShowHelpTextWithCustomErrorMessage("  only one of --file or --stdin options is allowed at the same time.");
                return false;
            }

            return true;
        }

        private void ShowHelpTextWithCustomErrorMessage(params string[] errors) {
            HelpText helpText = GetHelpText();
            Console.WriteLine(helpText.Heading);
            Console.WriteLine(helpText.Copyright);
            helpText.Heading = "";
            helpText.Copyright = "";
            Console.WriteLine();
            Console.WriteLine(helpText.SentenceBuilder.ErrorsHeadingText());
            foreach (string error in errors) {
                Console.WriteLine(error);
            }
            Console.WriteLine(helpText);
        }

        private HelpText GetHelpText() {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string assemblyConfiguration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration;
            string assemblyVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            DateTime assemblyBuildDateTime = assembly.GetBuildDateTime().GetValueOrDefault();

            HelpText helpText =
                _commandLineParserResult.Tag == ParserResultType.NotParsed ?
                HelpText.AutoBuild(_commandLineParserResult) :
                HelpText.AutoBuild(_commandLineParserResult, text => text, example => example);
            helpText.AddEnumValuesToHelpText = true;
            helpText.AddDashesToOption = true;
            helpText.Heading =
                $"MethodInlineInjector v{assemblyVersion} ({assemblyConfiguration}, built at {assemblyBuildDateTime:dd.MM.yyyy HH:MM})";
            helpText.Copyright = $"© Lost Polygon, {assemblyBuildDateTime.Year}";
            return helpText;
        }

        private static void ValidateConfiguration(string injectorConfigurationXml) {
            XmlDocument document = new XmlDocument();

            XmlSchema schema = XmlSchema.Read(new StringReader(Resources.injectorConfigurationSchema), null);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += (sender, args) => {
                switch (args.Severity) {
                    case XmlSeverityType.Error:
                        throw new MethodInlineInjectorException("Error parsing configuration", args.Exception);
                    case XmlSeverityType.Warning:
                        Log.Warn(args.Message);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };

            settings.Schemas.Add(schema);

            // Create the XmlReader object.
            XmlReader reader = XmlReader.Create(new StringReader(injectorConfigurationXml), settings);

            document.Load(reader);
        }
    }
}