using System;
using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
using Microsoft.Practices.EnterpriseLibrary.Common.Configuration.ContainerModel.Unity;
using Microsoft.Practices.EnterpriseLibrary.Common.Utility;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using Microsoft.Practices.EnterpriseLibrary.Logging.Filters;
using Microsoft.Practices.Unity;
using Xunit;

namespace Aquiva.Connector.ScribeApi.Metadata
{
    public class LoggingFixture : IDisposable
    {
        public LoggingFixture()
        {
            var container = new UnityContainer();
            
            // Put default EnterpriseLibrary registrations into container
            EnterpriseLibraryContainer.ConfigureContainer(
                new UnityContainerConfigurator(container),
                ConfigurationSourceFactory.Create());
            
            // Override LogWriter registration with stub
            // Stub registered with Singleton lifetime scope
            container.RegisterInstance(typeof(LogWriter), new StubbedLogWriter());
            EnterpriseLibraryContainer.Current = new UnityServiceLocator(container);

            StubbedLogWriter = (StubbedLogWriter) container.Resolve<LogWriter>();
        }
        
        public StubbedLogWriter StubbedLogWriter { get; }
        
        public void Dispose()
        {
            StubbedLogWriter.Dispose();
            EnterpriseLibraryContainer.Current.Dispose();
        }
    }

    [CollectionDefinition(nameof(LoggingCollection))]
    public class LoggingCollection : ICollectionFixture<LoggingFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }

    public class StubbedLogWriter : LogWriterImpl
    {
        public StubbedLogWriter() 
            : base(new ILogFilter[0], new LogSource[0], new LogSource(""), "")
        {
            LogEntries = new List<LogEntry>();
        }
        
        public IList<LogEntry> LogEntries { get; }
        
        public override void Write(LogEntry log)
        {
            LogEntries.Add(log);
        }

        protected override void Dispose(bool disposing)
        {
            LogEntries.Clear();
            base.Dispose(disposing);
        }
    }
}
