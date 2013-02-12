using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Security
{
    /// <summary>
    /// A single set of keys.
    /// </summary>
    public class SingleKeySet : HashSet<Key>, IKeySet
    {
        public virtual bool IsEmpty
        {
            get { return Count == 0; }
        }        

        public SingleKeySet()
            : base()
        {

        }

        public SingleKeySet(IEnumerable<Key> keys)
            : base(keys)
        {

        }

        public virtual bool Add(Key key)
        {
            return this.Add(key);
        }

        public virtual void Add(IKeySet theKeys)
        {
            ((SingleKeySet)theKeys).ToList().ForEach(x => this.Add(x));
        }

        public virtual void Remove(IKeySet theKeys)
        {
            ((SingleKeySet)theKeys).ToList().ForEach(x => this.Remove(x));
        }

        public virtual bool Remove(Key key)
        {
            return Remove(key);
        }

        public virtual IKeySet Subtract(IKeySet keys)
        {
            return new SingleKeySet(this.Except((SingleKeySet)keys));            
        }

        public override bool Equals(object obj)
        {
            return obj is SingleKeySet && Equals((SingleKeySet)obj);
        }

        public bool Equals(SingleKeySet sks)
        {
            return HashSet<Key>.CreateSetComparer().Equals(this, sks);
        }

        public override int GetHashCode()
        {
            return HashSet<Key>.CreateSetComparer().GetHashCode(this);
        }
    }
}
