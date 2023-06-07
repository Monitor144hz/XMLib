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
    
    private string GetDefaultPath(string path,string defaultValue, int elementIndex) 
    {   
        bool IsOpenPath = (path != string.Empty && path[path.Length - 1] != XPATH_DIVIDER);
        StringBuilder defaultBuilder = new StringBuilder(path); 
        if (IsOpenPath) defaultBuilder.Append(XPATH_DIVIDER); 
        defaultBuilder.Append(defaultValue).Append(elementIndex); 
        return defaultBuilder.ToString(); 
    }

    private string GetUniquePath(string path, string key)
    {
        bool IsOpenPath = (path != string.Empty && path[path.Length - 1] != XPATH_DIVIDER);
        StringBuilder uniqueBuilder = new StringBuilder(path); 

        if (IsOpenPath) uniqueBuilder.Append(XPATH_DIVIDER);
        uniqueBuilder.Append(key); 
        return uniqueBuilder.ToString(); 
    }
    private string MapChildElement(string path, XElement element, int elementIndex)
    {
        string defaultValue = Element.NodeType.ToString();
        string key = GenerateKey(element);
         
        return (key == defaultValue) ? GetDefaultPath(path, defaultValue, elementIndex) : GetUniquePath(path, key);  
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
            if (childElement.HasElements)  MapSubTree(elementName, childElement.Elements());
            defaultCount++;
        }
    }
}