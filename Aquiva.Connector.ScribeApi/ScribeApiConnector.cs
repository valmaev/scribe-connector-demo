using Scribe.Core.ConnectorApi;

namespace Aquiva.Connector.ScribeApi
{
    [ScribeConnector(
        name: "TIBCO Scribe Platform API (Demo)",
        description: "Metaconnector which connects platform to itself.",
        connectorTypeId: "e0e8461e-92ae-4b5f-980b-a2dd104b7d24",
        connectorType: typeof(ScribeApiConnector),
        connectorVersion: "0.0.0.0",
        supportedSolutionRoles: new[] {"Scribe.IS2.Source", "Scribe.IS2.Target"},
        supportsCloud: true,
        connectionUITypeName: "ScribeOnline.GenericConnectionUI",
        connectionUIVersion: "1.0.0.0",
        settingsUITypeName: "",
        settingsUIVersion: "1.0.0.0",
        xapFileName: "")]
    public sealed class ScribeApiConnector
    {
    }
}
