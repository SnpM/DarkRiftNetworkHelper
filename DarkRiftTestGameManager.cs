using UnityEngine;
using System.Collections;
using Lockstep.Example;
namespace Lockstep.DarkRift
{
    public class DarkRiftTestGameManager : ExampleGameManager
    {
        NetworkHelper _networkHelper = new DarkRiftNetworkHelper();
        public override NetworkHelper MainNetworkHelper
        {
            get
            {
                return _networkHelper;
            }
        }
    }
}