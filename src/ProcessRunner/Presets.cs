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
        /// Creates an automatic Preset set of all combinations from Sliders of a Dynamo project with a specific name and adds them to the original file.
        /// </summary>
        /// <param name="dynFilePath">The .dyn file with the presets to be added.</param>
        /// <param name="intSliderNames">The names of the Integer Sliders. If more than one Slider has this name, the will be all iterated.</param>
        /// <returns>The dynFilePath with the added Presets</returns>
        public static string Create(string dynFilePath, List<string> intSliderNames)
        {
            return Create(dynFilePath, intSliderNames, null);
        }

        /// <summary>
        /// Creates an automatic Preset set of all combinations from Sliders of a Dynamo project with a specific name and adds them to a preset file.
        /// </summary>
        /// <param name="dynFilePath">The .dyn file with the presets to be added.</param>
        /// <param name="intSliderNames">The names of the Integer Sliders. If more than one Slider has this name, the will be all iterated.</param>
        /// <param name="presetFilePath">Filepath for new preset file.</param>
        /// <returns>The path of the file containing the presets</returns>
        public static string Create(string dynFilePath, List<string> intSliderNames, string presetFilePath = null)
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

                    doc.DocumentElement.SelectSingleNode("Presets").AppendChild(presetModel);
                }

                min = min + step;
            }
            

        }

    }
}
