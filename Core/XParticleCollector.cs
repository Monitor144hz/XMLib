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

namespace XMLib
{
    public class XParticleCollector
    {
        public Action<string?>? WriteLog;


        public Dictionary<string, Func<XObject, XParticle, bool>> ActionDict { get; set; } = new Dictionary<string, Func<XObject, XParticle, bool>>();
        
        public HashSet<XParticle> Collect(string file, HashSet<string> captureStartComments)
        {
            bool captureAll = captureStartComments.Count == 0;
            XmlReaderSettings settings = new XmlReaderSettings() { IgnoreWhitespace = true, CheckCharacters = false, IgnoreProcessingInstructions = true };
            HashSet<XParticle> particles = new HashSet<XParticle>();
            HashSet<XParticle> ParticleSet = new HashSet<XParticle>();

            List<string> pathStack = new List<string>();
            List<int> genericCounts = new List<int>();

            HashSet<string> elementPaths = new HashSet<string>();
            HashSet<string> textPaths = new HashSet<string>();
            HashSet<string> attributePaths = new HashSet<string>();

            int maxDepth = -1;
            int lastDepth = 2147483647;
            int Depth;
            string readerValue;
            XmlNodeType NodeType;

            bool capture = false;

            try
            {
                using (XmlReader reader = XmlReader.Create(file, settings))
                {


                    while (reader.Read())
                    {
                        Depth = reader.Depth;
                        readerValue = reader.Value;
                        NodeType = reader.NodeType;

                        if (Depth < lastDepth && Depth + 1 < pathStack.Count)
                        {
                            genericCounts[Depth + 1] = 0;

                        }
                        lastDepth = Depth;
                        if (Depth > maxDepth)
                        {
                            maxDepth = Depth;

                            if (reader.HasAttributes)
                            {
                                pathStack.Add(reader.GetAttribute(0));
                                genericCounts.Add(0);
                            }
                            else
                            {
                                pathStack.Add("object" + "0");
                                genericCounts.Add(1);
                            }
                        }
                        else if (Depth < pathStack.Count)
                        {
                            if (reader.HasAttributes)
                            {
                                pathStack[Depth] = reader.GetAttribute(0);
                            }
                            else if (XmlNodeType.EndElement != NodeType && XmlNodeType.Comment != NodeType)
                            {
                                pathStack[Depth] = "object" + genericCounts[Depth];
                                genericCounts[Depth]++;

                            }
                        }
                        string s = String.Join("/", pathStack.SkipLast(maxDepth - Depth));
                        
                        switch (NodeType)
                        {
                            case XmlNodeType.Element:
                                if (capture)
                                {
                                    //WriteLog("Capture element found");
                                    using (XmlReader subreader = reader.ReadSubtree())
                                    {
                                        if (!elementPaths.Contains(s))
                                        {
                                            particles.Add(new XParticle(XElement.Load(subreader), s));
                                            elementPaths.Add(s);
                                        }
                                    }
                                }
                                break;
                            case XmlNodeType.Text:
                                if (capture && !textPaths.Contains(s))
                                {
                                    //particles.Add(new XParticle(new XText(readerValue), s));
                                    //textPaths.Add(s);
                                }
                                break;
                            case XmlNodeType.Comment:
                                WriteLog?.Invoke($"Reader value {readerValue}");
                                
                                if (capture)
                                {
                                    Func<XObject, XParticle, bool> func;
                                    
                                    if (ActionDict.TryGetValue(readerValue, out func!))
                                    {
                                        WriteLog?.Invoke("Valid capture block. Values captured:");
                                        foreach (XParticle particle in particles)
                                        {
                                            WriteLog?.Invoke(particle.Content.ToString());
                                            particle.SetWork(func);
                                            ParticleSet.Add(particle);
                                        }
                                        particles.Clear();
                                        elementPaths.Clear();
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
                return ParticleSet;
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
}
