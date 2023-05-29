using System.Xml;
using System.Xml.Linq;

namespace XMLib;



public enum XDifference
{
    XRemove, 
    XAppend,
    XInsert, 
    XChange
}
public class XCompare 
{


    private XMap originalMap; 
    private XMap newMap; 

    public Dictionary<string, XDifference> DifferencesByPath { get; set; } = new Dictionary<string, XDifference>(); 
    
    // public XCompare(XDocument compareFrom, XDocument compareTo, int mapDepth)
    // {
    //     originalMap = new XMap(compareFrom);
    //     newMap = new XMap(compareTo); 


    //     originalMap.Map(mapDepth);
    //     newMap.Map(mapDepth); 
    // }
    // public XCompare(XMap compareFrom, XMap compareTo)
    // {
    //     originalMap = compareFrom; 
    //     newMap = compareTo;
    // }

    


    

    


    

    
}