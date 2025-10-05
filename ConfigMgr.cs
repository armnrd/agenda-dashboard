using System.IO;
using YamlDotNet.RepresentationModel;

namespace AgendaDashboard;

public class ConfigMgr
{
    public YamlNode Config { get; private set; }

    public ConfigMgr()
    {
        var yaml = File.ReadAllText("settings.yaml");
        var stream = new YamlStream();
        stream.Load(new StringReader(yaml));

        // Get root node
        Config = (YamlMappingNode)stream.Documents[0].RootNode;
    }
}