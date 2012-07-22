using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Hanasu.Core
{
    public static class ObservableCollectionExt
    {
        public static int FastIndexOf<T>(this ObservableCollection<T> coll, T obj)
        {
            int index = -1;

            for (int i = 0; i < coll.Count; i++)
                if (coll[i].GetHashCode() == obj.GetHashCode())
                {
                    index = i;
                    break;
                }

            return index;
        }
    }
}
