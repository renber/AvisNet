using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Security.Special
{
    class EmptyKeys : Keys
  {   
    public override void Add (Keys keys)
    {
      throw new NotSupportedException ();
    }

    public override void remove (Keys keys)
    {
        throw new NotSupportedException();
    }

    public override void Add (SingleKeyScheme scheme, Key key)
    {
        throw new NotSupportedException();
    }

    public override void Remove (SingleKeyScheme scheme, Key key)
    {
        throw new NotSupportedException();
    }

    public override void Add (DualKeyScheme scheme, DualKeyScheme.Subset subset, Key key)
    {
        throw new NotSupportedException();
    }

    public override void Remove(DualKeyScheme scheme, DualKeyScheme.Subset subset, Key key)
    {
        throw new NotSupportedException();
    }
  }

}
