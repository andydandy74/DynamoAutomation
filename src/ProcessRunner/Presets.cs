using System.Collections.Generic;
using System.Xml;

namespace Dynamo.Automation
{
    /// <summary>
    /// Automates Preset creation.
    /// </summary>
    public static class Presets
    {
        /// <summary>
        /// Creates Presets of all possible combinations of any given Integer Slider values within a Dynamo workspace and adds them to the original workspace file.
        /// </summary>
        /// <param name="dynFilePath">The path to the .dyn file.</param>
        /// <param name="intSliderNames">The names of the Integer Sliders. If more than one Slider has the same name, the node will iterate through all of them.</param>
        /// <returns>The dynFilePath with the added Presets.</returns>
        public static string ByWorkspacePathAndSliderNames(string dynFilePath, List<string> intSliderNames)
        {
            return ByWorkspacePathSliderNamesAndNewPath(dynFilePath, intSliderNames, null);
        }

        /// <summary>
        /// Creates Presets of all possible combinations of any given Integer Slider values within a Dynamo workspace and adds them to a new preset file.
        /// </summary>
        /// <param name="dynFilePath">The path to the .dyn file.</param>
        /// <param name="intSliderNames">The names of the Integer Sliders. If more than one Slider has this name, the will be all iterated.</param>
        /// <param name="presetFilePath">File path for the new preset file.</param>
        /// <returns>The path of the new preset file.</returns>
        public static string ByWorkspacePathSliderNamesAndNewPath(string dynFilePath, List<string> intSliderNames, string presetFilePath = null)
        {
            
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(dynFilePath);

            var xmlWorkspace = xmlDocument.DocumentElement;

            var sliders = new List<XmlNode>();

            var xmlElements = xmlWorkspace["Elements"].ChildNodes;
            if (xmlElements != null)
            {
                foreach (XmlNode element in xmlElements)
                {
                    if (element.Name == "DSCoreNodesUI.Input.IntegerSlider")
                    {
                        for (int i = 0; i < intSliderNames.Count; i++)
                        {
                            if (element.Attributes["nickname"].Value.Equals(intSliderNames[i]))
                            {
                                sliders.Add(element);
                            }
                        }
                    }
                }
            }

            if (xmlWorkspace.SelectSingleNode("Presets") == null)
            {
                XmlElement presets = xmlDocument.CreateElement("Presets");
                xmlWorkspace.AppendChild(presets);
            }

            if (sliders.Count > 0)
            {
                var presetNodes = new XmlNode[sliders.Count];
                IterateList(xmlDocument, sliders, presetNodes);
            }


            if (presetFilePath == null)
            {
                presetFilePath = dynFilePath;
                xmlDocument.Save(presetFilePath);
            }
            else
            {
                var presetDoc = new XmlDocument();
                presetDoc.LoadXml(@"<Workspace></Workspace>");
                var presets = presetDoc.ImportNode(xmlWorkspace.SelectSingleNode("Presets"), true);

                var presetWorkspace = presetDoc.DocumentElement;
                presetWorkspace.SetAttribute("Version", xmlWorkspace.Attributes["Version"].Value);
                presetWorkspace.AppendChild(presets);

                presetDoc.Save(presetFilePath);
            }

            return presetFilePath;

        }

        private static void IterateList( XmlDocument doc, List<XmlNode> nodes, XmlNode[] collector, int index = 0)
        {

            var min = int.Parse(nodes[index].SelectSingleNode("Range").Attributes["min"].Value);
            var max = int.Parse(nodes[index].SelectSingleNode("Range").Attributes["max"].Value);
            var step = int.Parse(nodes[index].SelectSingleNode("Range").Attributes["step"].Value);

            //var presetNode = nodes[index].Clone();

            while ( min <= max )
            {
                var presetNode = nodes[index].Clone();

                presetNode.SelectSingleNode("System.Int32").InnerText = min.ToString();
                collector[index] = presetNode;

                if (index != nodes.Count - 1)
                {
                    IterateList(doc, nodes, collector, index + 1);
                }
                else
                {
                    var name = "-";
                    var presetModel = doc.CreateElement("Dynamo.Models.PresetModel");
                    foreach (var xmlNode in collector)
                    {
                        presetModel.AppendChild(xmlNode.Clone());
                        name += xmlNode.SelectSingleNode("System.Int32").InnerText + "-";
                    }
                    presetModel.SetAttribute("Name", name);
                    //if (doc.DocumentElement.SelectSingleNode("Presets").ChildNodes.Attributes.Exist("Name", name) == null) //[redinkinc] doesn't work yet...
                        doc.DocumentElement.SelectSingleNode("Presets").AppendChild(presetModel);
                }

                min = min + step;
            }
            

        }

        /// <summary>
        /// Remove all Presets from a specified file
        /// </summary>
        /// <param name="dynFilePath">The path to the .dyn file.</param>
        /// <returns>The dynFilePath.</returns>
        public static string Clear(string dynFilePath)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(dynFilePath);

            var xmlWorkspace = xmlDocument.DocumentElement;
            var presets = xmlWorkspace.SelectSingleNode("Presets");

            if (presets != null) presets.RemoveAll();

            xmlDocument.Save(dynFilePath);

            return dynFilePath;
        }

    }
}
