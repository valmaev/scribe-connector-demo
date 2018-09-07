using System;
using System.Collections.Generic;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.ConnectionUI;
using Scribe.Core.ConnectorApi.Query;

namespace Aquiva.Connector.ScribeApi
{
    [ScribeConnector(
        name: "TIBCO Scribe Platform API (Demo)",
        description: "Metaconnector which connects platform to itself.",
        connectorTypeId: ConnectorTypeIdString,
        connectorType: typeof(ScribeApiConnector),
        connectorVersion: "0.0.0.0",
        supportedSolutionRoles: new[] {"Scribe.IS2.Source", "Scribe.IS2.Target"},
        supportsCloud: true,
        connectionUITypeName: "ScribeOnline.GenericConnectionUI",
        connectionUIVersion: "1.0.0.0",
        settingsUITypeName: "",
        settingsUIVersion: "1.0.0.0",
        xapFileName: "")]
    public sealed class ScribeApiConnector : IConnector
    {
        private const string ConnectorTypeIdString = "e0e8461e-92ae-4b5f-980b-a2dd104b7d24";
        private const string CryptoKey = "3103dcf5-6d7c-4b56-8297-f9e449b57576";

        public Guid ConnectorTypeId { get; } = Guid.Parse(ConnectorTypeIdString);
        public bool IsConnected { get; }

        public string PreConnect(IDictionary<string, string> properties)
        {
            var form = new FormDefinition
            {
                CompanyName = "Aquiva Labs",
                CryptoKey = CryptoKey,
                HelpUri = new Uri("https://aquivalabs.com"),
                Entries = new List<EntryDefinition>
                {
                    new EntryDefinition
                    {
                        Label = "Environment",
                        PropertyName = "Environment",
                        InputType = InputType.Text,
                        IsRequired = false,
                        Order = 0,
                        Options =
                        {
                            ["https://qaendpoint.scribeqa.net"] = "QA",
                            ["https://demoendpoint.scribeqa.net"] = "DEMO",
                            ["https://sbendpoint.scribesoft.com"] = "SANDBOX",
                            ["https://api.scribesoft.com"] = "PRODUCTION",
                        }
                    },
                    new EntryDefinition
                    {
                        Label = "Username",
                        PropertyName = "Username",
                        InputType = InputType.Text,
                        IsRequired = true,
                        Order = 1
                    },
                    new EntryDefinition
                    {
                        Label = "Password",
                        PropertyName = "Password",
                        InputType = InputType.Password,
                        IsRequired = true,
                        Order = 2
                    }
                }
            };

            return form.Serialize();
        }

        public void Connect(IDictionary<string, string> properties) { }
        public void Disconnect() { }

        public IMetadataProvider GetMetadataProvider() => null;

        public IEnumerable<DataEntity> ExecuteQuery(Query query) => new DataEntity[0];
        public OperationResult ExecuteOperation(OperationInput input) => null;
        public MethodResult ExecuteMethod(MethodInput input) => null;
    }
}
