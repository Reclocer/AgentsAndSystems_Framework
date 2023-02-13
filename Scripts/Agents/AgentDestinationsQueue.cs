using System;
using System.Collections.Generic;
using UnityEngine;

namespace SUBS.AgentsAndSystems
{
    [Serializable]
    internal class AgentDestinationsQueue
    {        
        [SerializeField] internal bool DestinationsLooped = false;
        
        [SerializeField] private List<AgentsQueue> _agentDestinations = new List<AgentsQueue>(16);

        internal AgentsQueue GetDestination()
        {
            if (_agentDestinations.Count == 0)
                return null;

            AgentsQueue agentsQueue = _agentDestinations[0];
            _agentDestinations.Remove(agentsQueue);            

            if (DestinationsLooped)
            {
                _agentDestinations.Add(agentsQueue);
            }

            return agentsQueue;
        }

        internal void AddDestination(AgentsQueue destination)
        {
            _agentDestinations.Add(destination);
        }

        internal void AddDestinations(List<AgentsQueue> destinations)
        {
            _agentDestinations.AddRange(destinations);
        }

        internal void ClearDestinations()
        {
            _agentDestinations.Clear();
        }
    }
}
