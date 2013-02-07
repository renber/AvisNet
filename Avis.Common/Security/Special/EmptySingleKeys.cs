using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Security.Special
{

     class EmptySingleKeys : SingleKeySet
  {
    
    public override bool add (Key key)
    {
      throw new NotSupportedException ();
    }

    
    public override void add (IKeySet keys)      
    {
      throw new NotSupportedException ();
    }

    public override bool remove (Key key)      
    {
      return false;
    }
    
    public override void remove (IKeySet keys)      
    {
      // zip
    }
  }
}
