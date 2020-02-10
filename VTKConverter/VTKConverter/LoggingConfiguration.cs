using log4net.Config;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Filter;

namespace VTKConverter
{
    public class LoggingConfiguration
    {
        public LoggingConfiguration()
        {
            var infoFileLogger = InitializeInfoLogger();
            var errorFileLogger = InitializeErrorLogger();

            BasicConfigurator.Configure(infoFileLogger, errorFileLogger);
        }

        private static ConsoleAppender InitializeInfoLogger()
        {
            var infoPatternLayout = new PatternLayout
            {
                ConversionPattern = "%date %logger %level - %message%newline",
            };
            infoPatternLayout.ActivateOptions();

            var filter = new LevelRangeFilter
            {
                LevelMin = Level.Debug,
                LevelMax = Level.Info,
            };
            filter.ActivateOptions();
            var infofileAppender = new ConsoleAppender
            {
                Layout = infoPatternLayout,
            };
            infofileAppender.AddFilter(filter);
            infofileAppender.ActivateOptions();

            return infofileAppender;
        }

        private static ConsoleAppender InitializeErrorLogger()
        {
            var errorPatternLayout = new PatternLayout
            {
                ConversionPattern = "%date %logger %level - %message%newline%exception%stacktracedetail",
                IgnoresException = false,
            };
            errorPatternLayout.ActivateOptions();
            var filter = new LevelRangeFilter
            {
                LevelMin = Level.Warn,
                LevelMax = Level.Fatal
            };
            filter.ActivateOptions();
            var errorFileAppender = new ConsoleAppender
            {
                Layout = errorPatternLayout
            };
            errorFileAppender.AddFilter(filter);
            errorFileAppender.ActivateOptions();
            return errorFileAppender;
        }
    }
}
