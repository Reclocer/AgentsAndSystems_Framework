using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SUBS.AgentsAndSystems
{
    internal class TempComplex2 : MonoBehaviour
    {
        [SerializeField] private AgentsQueue _station1Export11Queue;
        [SerializeField] private AgentsQueue _station1Export12Queue;

        [Space]
        [SerializeField] private AgentsQueue _station2Export2Queue;

        [Space]
        [SerializeField] private AgentsQueue _station4Import11Queue;
        [SerializeField] private AgentsQueue _station4Import12Queue;
        [SerializeField] private AgentsQueue _station4Import3Queue;
        [SerializeField] private AgentsQueue _station4Export4Queue;

        [Space]
        [SerializeField] private AgentsQueue _receiver2Import2Queue;
        [SerializeField] private AgentsQueue _receiver3Export3Queue;

        [Space]
        [SerializeField] private AgentsQueue _market1Import1Queue;
        [SerializeField] private AgentsQueue _market1Export1Queue;
        [SerializeField] private AgentsQueue _market1Import2Queue;
        [SerializeField] private AgentsQueue _market1Export3Queue;

        [Space]
        [SerializeField] private AgentsQueue _market2Import1Queue;
        [SerializeField] private AgentsQueue _market2Export1Queue;
        [SerializeField] private AgentsQueue _market2Import4Queue;

        [Space]
        [SerializeField] private List<Agent> _agents;

        private void Start()
        {
            for (int i = 0; i < 2; i++)
            {
                _agents[i].AddDestinationQueue(_station1Export11Queue);
                _agents[i].AddDestinationQueue(_station4Import11Queue);
                _agents[i].GoToNextQueue();
            }

            for (int i = 2; i < 5; i++)
            {
                _agents[i].AddDestinationQueue(_station1Export12Queue);
                _agents[i].AddDestinationQueue(_station4Import12Queue);
                _agents[i].GoToNextQueue();
            }

            for (int i = 5; i < 7; i++)
            {
                _agents[i].AddDestinationQueue(_receiver3Export3Queue);
                _agents[i].AddDestinationQueue(_station4Import3Queue);
                _agents[i].GoToNextQueue();
            }

            for (int i = 7; i < 10; i++)
            {
                _agents[i].AddDestinationQueue(_station4Export4Queue);
                _agents[i].AddDestinationQueue(_market2Import4Queue);
                _agents[i].GoToNextQueue();
            }

            for (int i = 10; i < 13; i++)
            {
                _agents[i].AddDestinationQueue(_station2Export2Queue);
                _agents[i].AddDestinationQueue(_receiver2Import2Queue);
                _agents[i].GoToNextQueue();
            }

            _agents[13].AddDestinationQueue(_station1Export12Queue);
            _agents[13].AddDestinationQueue(_market2Import1Queue);
            _agents[13].GoToNextQueue();

            _agents[14].AddDestinationQueue(_station2Export2Queue);
            _agents[14].AddDestinationQueue(_market1Import2Queue);
            _agents[14].GoToNextQueue();

            _agents[15].AddDestinationQueue(_market1Export3Queue);
            _agents[15].AddDestinationQueue(_station4Import3Queue);
            _agents[15].GoToNextQueue();

            _agents[16].AddDestinationQueue(_station4Export4Queue);
            _agents[16].AddDestinationQueue(_market2Import4Queue);
            _agents[16].GoToNextQueue();
        }
    }
}
