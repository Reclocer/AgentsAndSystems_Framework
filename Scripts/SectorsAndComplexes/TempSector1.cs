using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SUBS.AgentsAndSystems
{
    internal class TempSector1 : MonoBehaviour
    {
        [SerializeField] private AgentsQueue _complex1Import2Queue;
        [SerializeField] private AgentsQueue _complex1Export3Queue;

        [Space]
        [SerializeField] private AgentsQueue _complex2Import3Queue;
        [SerializeField] private AgentsQueue _complex2Export2Queue;

        [Space]
        [SerializeField] private List<Agent> _agents;

        private void Start()
        {
            for (int i = 0; i < 4; i++)
            {
                _agents[i].AddDestinationQueue(_complex1Export3Queue);
                _agents[i].AddDestinationQueue(_complex2Import3Queue);
                _agents[i].GoToNextQueue();
            }

            for (int i = 4; i < 8; i++)
            {
                _agents[i].AddDestinationQueue(_complex2Export2Queue);
                _agents[i].AddDestinationQueue(_complex1Import2Queue);
                _agents[i].GoToNextQueue();
            }
        }
    }
}
