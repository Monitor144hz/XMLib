using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace XMLib;

public class XParticleCollector
{
    public Action<string?>? WriteLog = Console.WriteLine;


    public Dictionary<string, Func<XObject, XParticle, bool>> ActionDict { get; set; } = new Dictionary<string, Func<XObject, XParticle, bool>>();
    

    public XParticleGroup CollectFromFile(string file, string captureStartComment) => CollectFromFile(file, new HashSet<string>() { captureStartComment });
    public XParticleGroup CollectFromFile(string file, HashSet<string> captureStartComments)
    {
        bool captureAll = captureStartComments.Count == 0;
        XmlReaderSettings settings = new XmlReaderSettings() { IgnoreWhitespace = true, CheckCharacters = false, IgnoreProcessingInstructions = true };

        XParticleGroup particleGroup = new XParticleGroup();

        XPathTracker pathTracker = new XPathTracker();

        string readerValue;


        bool capture = false;

        try
        {
            using (XmlReader reader = XmlReader.Create(file, settings))
            {


                while (reader.Read())
                {
                    readerValue = reader.Value;
                    pathTracker.ResolvePath(reader);
                    string currentPath = pathTracker.GetCurrentPath(reader);
                    WriteLog?.Invoke(currentPath);
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (capture)
                            {
                                using (XmlReader subreader = reader.ReadSubtree())
                                {
                                        particleGroup.AddToCache(new XParticle(XElement.Load(subreader), currentPath));
                                }
                            }
                            break;
                        case XmlNodeType.Text:
                                particleGroup.AddToCache(new XParticle(new XText(readerValue), currentPath));
                            break;
                        case XmlNodeType.Comment:
                            WriteLog?.Invoke($"Reader value {readerValue}");

                            if (capture)
                            {
                                Func<XObject, XParticle, bool> func;

                                if (ActionDict.TryGetValue(readerValue, out func!))
                                {
                                    WriteLog?.Invoke("Valid capture block. Values captured:");
                                    particleGroup.SetWorkForCache(func);
                                    particleGroup.ApplyCache();
                                    capture = false;
                                }
                            }
                            else if (captureStartComments.Contains(readerValue) || captureAll)
                            {
                                WriteLog?.Invoke("Capture started.");
                                capture = true;
                            }
                            break;
                    }

                }
            }
            return particleGroup;
        }
        catch (XmlException e)
        {
            throw new XmlException($"XMLib: {e.Message} in file {file}");
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
