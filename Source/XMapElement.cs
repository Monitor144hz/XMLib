using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace XMLib; 

public class XMapElement
{

    private XElement Element;

    
    public XMapElement(XElement element) 
    {
        Element = element; 
        GenerateKey = GetKeyFromNameAttribute; 
    }

    private Dictionary<string, XElement> dict {get; set;}= new Dictionary<string, XElement>();

    public Func<XElement, string> GenerateKey;

    public char XPATH_DIVIDER { get; private set; } = '/'; 

    public string GetKeyFromNameAttribute(XElement element) => element.Attribute("name")!.Value; 

    public XElement this[string key]
    {
        // get
        // {
        //     XElement? element;
        //     return dict.TryGetValue(key, out element) ? element! : null;
        // }
        // set
        // {
        //     if (dict.ContainsKey(key)) dict[key] = value!;
        // }
        get => dict[key] ; 
        set => dict[key] = value; 
    }
    private string MapChildElement(string path, XElement element, int elementIndex)
    {
        //Console.WriteLine($"XPath {path} keyName {keyName}");
        //Console.WriteLine(element);

        string defaultValue = XmlNodeType.Element.ToString();

        bool IsOpenPath = (path != string.Empty && path[path.Length - 1] != XPATH_DIVIDER);

        StringBuilder defaultPathBuilder = !IsOpenPath ? new StringBuilder(path).Append(defaultValue).Append(elementIndex) : new StringBuilder(path).Append(XPATH_DIVIDER).Append(defaultValue).Append(elementIndex);
        StringBuilder pathBuilder = !IsOpenPath ? new StringBuilder(path) : new StringBuilder(path).Append(XPATH_DIVIDER);
        pathBuilder.Append(GenerateKey(element)); 
        string elementPath = pathBuilder.ToString();
        string defaultPath = defaultPathBuilder.ToString();

        if (elementPath != defaultPath) dict.Add(elementPath, element);
        dict.Add(defaultPathBuilder.ToString(), element);

        //Console.WriteLine($"Path: {elementPath} Default Path: {defaultPath}");
        return elementPath;

    }

    public bool Map()
    {
        dict.Clear();
        try
        {
            if (!Element.HasElements) return false;
            IEnumerable<XElement> ChildElements = Element.Elements();

            MapSubTree("", ChildElements);
        
            return true;
        }
        catch (XmlException e)
        {
            throw new XmlException($"{e.Message} in element {Element.ToString()}");
        }
    }


    public string GetKeyPath(XElement element)
    {
        throw new NotImplementedException();
    }
    private void MapSubTree(string path, IEnumerable<XElement> ChildElements)
    {
        string elementName;
        int defaultCount = 0;

        foreach (XElement childElement in ChildElements)
        {
            elementName = MapChildElement(path, childElement, defaultCount);
            if (childElement.HasElements)
            {
                MapSubTree(elementName, childElement.Elements());
            }
            defaultCount++;
        }
    }
}