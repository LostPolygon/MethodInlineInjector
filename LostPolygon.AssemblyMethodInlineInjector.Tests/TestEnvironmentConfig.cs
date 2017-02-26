using System;
using System.Reflection;
using IniParser;
using IniParser.Model;
using IniParser.Parser;
using NUnit.Framework;

namespace LostPolygon.AssemblyMethodInlineInjector.Tests {
    internal class TestEnvironmentConfig {
        private static TestEnvironmentConfig _instance;

        public static TestEnvironmentConfig Instance => _instance;

        public string TargetDir { get; private set; }
        public string ProjectDir { get; private set; }
        public string SolutionDir { get;  private set; }
        public string ConfigurationName { get; private set; }

        private TestEnvironmentConfig(string testEnvironmentConfigPath) {
            FileIniDataParser iniParser = new FileIniDataParser();
            IniData iniData = iniParser.ReadFile(testEnvironmentConfigPath);

            PropertyInfo[] propertyInfos = 
                typeof(TestEnvironmentConfig)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo propertyInfo in propertyInfos) {
                string value = iniData["Config"][propertyInfo.Name];
                Assert.NotNull(value);
                propertyInfo.SetValue(this, value);
            }
        }

        public static void SetTestEnvironmentConfigPath(string testEnvironmentConfigPath) {
            _instance = new TestEnvironmentConfig(testEnvironmentConfigPath);
        }
    }
}