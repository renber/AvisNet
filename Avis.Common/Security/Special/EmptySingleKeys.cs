using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Avis.Security.Special
{

     class EmptySingleKeys : SingleKeySet
  {
    
    public override bool Add (Key key)
    {
      throw new NotSupportedException ();
    }

    
    public override void Add (IKeySet keys)      
    {
      throw new NotSupportedException ();
    }

    public override bool Remove (Key key)      
    {
      return false;
    }
    
    public override void Remove (IKeySet keys)      
    {
      // zip
    }
  }
}
