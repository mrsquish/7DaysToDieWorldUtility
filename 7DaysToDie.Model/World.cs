using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DaysToDie.Model
{
    public class World : IDisposable
    {
        private Prefabs _prefabs;
        public string BasePath { get; set; }

        public Prefabs Prefabs
        {
            get
            {
                if (_prefabs == null)
                    _prefabs = Prefabs.FromFile(Path.Combine(BasePath, "prefabs.xml"));
                return _prefabs;
            }
        }

        public void Dispose()
        {

        }
    }
}
