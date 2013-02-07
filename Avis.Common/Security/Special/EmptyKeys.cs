using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Security.Special
{
    class EmptyKeys : Keys
  {   
    public override void add (Keys keys)
    {
      throw new NotSupportedException ();
    }

    public override void remove (Keys keys)
    {
        throw new NotSupportedException();
    }

    public override void add (SingleKeyScheme scheme, Key key)
    {
        throw new NotSupportedException();
    }

    public override void remove (SingleKeyScheme scheme, Key key)
    {
        throw new NotSupportedException();
    }

    public override void add (DualKeyScheme scheme, DualKeyScheme.Subset subset, Key key)
    {
        throw new NotSupportedException();
    }

    public override void remove(DualKeyScheme scheme, DualKeyScheme.Subset subset, Key key)
    {
        throw new NotSupportedException();
    }
  }

}
