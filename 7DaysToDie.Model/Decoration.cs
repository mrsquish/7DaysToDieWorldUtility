using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace _7DaysToDie.Model
{
    public class Decoration
    {
        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }
        
        public Vector3Int Position { get; set; }

        [XmlAttribute("position")]
        public string positionString {
            get => $"{Position.X},{Position.Y},{Position.Z}";
            set
            {
                var points = value.Split(',').Select(s => Int32.TryParse(s, out var fout) ? fout : 0).ToArray();
                Position = new Vector3Int(points[0], points[1], points[2]);
            }
        }

        [XmlAttribute("rotation")]
        public int Rotation { get; set; }

    }
}
