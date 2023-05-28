

using System.Xml;

namespace XMLib; 

public class XPathTracker
{
    List<string> trackedPath = new List<string>();
    List<int> unnamedElementCounts = new List<int>();

    int maxDepth = -1;
    int lastDepth = 2147483647;

    private Func<XmlReader, string> GetPathName;
    private Func<XmlReader, bool> HasIdentifier; 
    private string GetPathNameFromAttribute(XmlReader reader) =>  reader.GetAttribute(0);
    private bool HasIdentiferFromAttribute(XmlReader reader) => reader.HasAttributes;

    public XPathTracker()
    {
        GetPathName = GetPathNameFromAttribute;
        HasIdentifier = HasIdentiferFromAttribute;
    }
    private bool IsContentNode(XmlNodeType nodeType) =>(XmlNodeType.EndElement != nodeType && XmlNodeType.Comment != nodeType);

    private void ChangePath(int depth, string value) => trackedPath[depth] = value;

    private void ExtendPath(XmlReader reader)
    {
        maxDepth = reader.Depth;

        if (HasIdentifier(reader))
        {
            trackedPath.Add(GetPathName(reader));
            unnamedElementCounts.Add(0);
            return;
        }
        trackedPath.Add(reader.NodeType.ToString() + "0");
        unnamedElementCounts.Add(1);
    }

    public string GetCurrentPath(XmlReader reader) => String.Join("/", trackedPath.SkipLast(maxDepth - reader.Depth));
    public void ResolvePath(XmlReader reader)
    {
        int depth = reader.Depth; 
        XmlNodeType nodeType = reader.NodeType;
        
        if (depth < lastDepth && depth + 1 < trackedPath.Count)  unnamedElementCounts[depth + 1] = 0;
        lastDepth = depth;
        if (depth > maxDepth) ExtendPath(reader);
        else if (depth < trackedPath.Count)
        {
            if (HasIdentifier(reader)) 
            {
                ChangePath(depth, GetPathName(reader));
                return;
            }
            if (IsContentNode(reader.NodeType))
            {
                trackedPath[depth] = nodeType.ToString() + unnamedElementCounts[depth];
                unnamedElementCounts[depth]++;
            }
            
        }
    }
}
