using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snobal.Utilities
{
    internal class FrequentUpdater
    {
        List<IUpdatable> objectsToUpdate;

        internal FrequentUpdater()
        {
            objectsToUpdate = new List<IUpdatable>();
        }

        internal void Add(IUpdatable objectToUpdate)
        {
            objectsToUpdate.Add(objectToUpdate);
        }

        internal void Remove(IUpdatable objectToUpdate)
        {
            objectsToUpdate.Remove(objectToUpdate);
        }

        internal void UpdateAll()
        {
            foreach (IUpdatable uo in objectsToUpdate)
            {
                uo.Update();
            }
        }
    }
}
