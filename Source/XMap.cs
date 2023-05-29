using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace XMLib;

/// <summary>
/// Represents an XML document, mapped for element access with XPath notation.
/// </summary>
public class XMap : XDocument
{

    const char XPATH_DIVIDER = '/';
    const char XATTRIBUTE_IDENTIFIER = '@';
    
    public enum KeyTypes
    {
        XAttribute,
        XElementName,
        XText
    }

    public KeyTypes KeyType { get; set; } = KeyTypes.XAttribute;


    public XMap(XDocument doc) : base(doc) { }
    public XMap(XElement element) : base(element) { }
    public XMap(XName name) : base(name) { }
    public XMap(XName name, object? content) :base(name, content) { }
    public XMap(XName name, object[]? content) : base(name, content!) { }
    public XMap(XStreamingElement other) : base(other) { }

    public static new XMap Load(Stream stream) => new XMap(XDocument.Load(stream));
    public static new XMap Load(TextReader textReader) => new XMap(XDocument.Load(textReader));
    public static new XMap Load(string uri) => new XMap(XDocument.Load(uri));
    public static new XMap Load(XmlReader xmlReader) => new XMap(XDocument.Load(xmlReader));


    public List<string> GetPathList() => dict.Keys.ToList();
    public XElement? this[string key]
    {
        get 
        {
            XElement? element; 
            return dict.TryGetValue(key, out element) ? element! : null;
        }
        set 
        {
            if (dict.ContainsKey(key)) dict[key] = value!;
        }
    }
    public bool RemoveElement(string key) 
    {
        XElement element = this[key]!;
        lock(element.Parent!) { lock(element){ element.Remove();  }} //remove from xml tree
        return true;
    }
    public bool AddChildElement(string key, XElement newElement) 
    {
        XElement? element = this[key]; 
        dict.Add(key, newElement);

        return true;
    }
    public bool AddElementAfter(string key, XElement newElement) 
    {
        return true;
    }

    public bool AddElementBefore(string key, XElement newElement)
    {
        return true;
    }
    public bool ReplaceElement(string key, XElement newElement)
    {
        XElement? element = this[key];
        if (element is null || element.Parent is null) return false; //null check
        lock(element.Parent) { lock(element){ element.ReplaceWith(newElement);  }} 
        return true; 
    }


    public Action<string> Log { get; set; } = (message) => Console.WriteLine(message);

    private string keyName = "name";
    // private string defaultValue = "object";

    private Dictionary<string, XElement> dict { get; set; } = new Dictionary<string, XElement>();
    
    private string MapChildElement(string path, XElement element, int elementIndex)
    {
        //Console.WriteLine($"XPath {path} keyName {keyName}");
        //Console.WriteLine(element);

        string defaultValue = XmlNodeType.Element.ToString();

        bool IsOpenPath = (path != string.Empty && path[path.Length-1] != XPATH_DIVIDER);
        
        StringBuilder defaultPathBuilder =  !IsOpenPath ? new StringBuilder(path).Append(defaultValue).Append(elementIndex) : new StringBuilder(path).Append(XPATH_DIVIDER).Append(defaultValue).Append(elementIndex);
        StringBuilder pathBuilder = !IsOpenPath ? new StringBuilder(path) : new StringBuilder(path).Append(XPATH_DIVIDER);
        switch (KeyType)
        {
            case KeyTypes.XAttribute:
                pathBuilder.Append((element.Attribute(keyName) ?? new XAttribute("name", $"{defaultValue}{elementIndex}")).Value);
                break;
            case KeyTypes.XElementName:
                pathBuilder.Append(element.Name ?? defaultValue);
                break;  
            case KeyTypes.XText:
                pathBuilder.Append(element.Value ?? defaultValue);
                break;
        }
        string elementPath = pathBuilder.ToString();
        string defaultPath = defaultPathBuilder.ToString();

        if (elementPath != defaultPath) dict.Add(elementPath, element);
        dict.Add(defaultPathBuilder.ToString(), element);
        
        //Console.WriteLine($"Path: {elementPath} Default Path: {defaultPath}");
        return elementPath;
        
    }
    
    public bool Map(int skipdepth)
    {

        try
        {
            if (Root!.HasElements)
            {
                IEnumerable<XElement> ChildElements = new XElement[] { Root };
                for (int d = 0 ; d < skipdepth; d++)
                {
                    ChildElements = ChildElements.First().Elements();
                }
                MapSubTree("", ChildElements);
            }
            return true;
        }
        catch (XmlException e)
        {
            throw new XmlException($"{e.Message} in file {Root!.Name}");
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
    public XCompare Compare(XMap otherMap) 
    {
        XNodeEqualityComparer comparer = new XNodeEqualityComparer();
        XCompare compare = new XCompare();
        List<string> Paths = GetPathList();

        List<string> otherPaths = otherMap.GetPathList();
        
        foreach(string path in Paths)
        {
            if (otherMap[path] is null)
            {
                compare.DifferencesByPath.Add(path, XDifference.XRemove); 
                continue;
            } 

            // if (!comparer.Equals(this[path], otherMap[path]))
            // {
            //     compare.DifferencesByPath.Add(path, XDifference.XChange); 
            //     continue; 
            // }
            
        }

        foreach(string path in otherPaths)
        {
            // if (!path.Contains(XPATH_DIVIDER) && this[path] is null) Console.WriteLine(path);
            if (this[path] is null) 
            {
                
                compare.DifferencesByPath.Add(path, XDifference.XAppend);
            }
        }
        
        return compare;
    }
    
}
