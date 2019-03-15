using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace _7DaysToDie.Model
{    
    [XmlRoot("prefabs", Namespace = "", IsNullable = false)]
    public class Prefabs
    {
        private string originalFileName;
        //[XmlArray("prefabs")]
        [XmlElement("decoration", typeof(Decoration))]
        public List<Decoration> Decorations { get; set; }

        public static Prefabs FromFile(string prefabsXmlFilePath)
        {            
            var serializer = new XmlSerializer(typeof(Prefabs));
            using (var file = File.OpenText(prefabsXmlFilePath))
            {
                var prefabs = (Prefabs)serializer.Deserialize(file);
                prefabs.originalFileName = prefabsXmlFilePath;
                return prefabs;
            }            
        }

        public void Save()
        {
            var serializer = new XmlSerializer(typeof(Prefabs));
            using (var stream = File.Open(originalFileName, 
                FileMode.Create, FileAccess.Write, FileShare.None))
            {
                serializer.Serialize(stream, this);
                stream.Flush(true);
                stream.Close();
            }

            
        }
    }
}
