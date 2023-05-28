using System.Xml;
using System.Xml.Linq;


namespace XMLib;



public class XParticleGroup
{
    private List<XParticle> particleCache  = new List<XParticle>(); 

    public List<XParticle> Particles {get; set;}= new List<XParticle>();
    public void AddToCache(XParticle particle) => particleCache.Add(particle);

    public void SetWorkForCache(Func<XObject,XParticle, bool> XAction) => particleCache.ForEach(p => p.SetWork(XAction));

    public void ApplyCache() 
    {
        Particles.AddRange(particleCache);
        particleCache.Clear();
    } 


    

}
