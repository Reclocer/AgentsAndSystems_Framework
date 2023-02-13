using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace SUBS.AgentsAndSystems
{
    internal class TempComplex1 : MonoBehaviour
    {
        [SerializeField] private AgentsQueue _station1Export1Queue;
        [SerializeField] private AgentsQueue _station2Import1Queue;
        [SerializeField] private AgentsQueue _station2Import2Queue;
        [SerializeField] private AgentsQueue _station2Export3Queue;
        [SerializeField] private AgentsQueue _receiver1Import3Queue;
        [SerializeField] private AgentsQueue _receiver2Export2Queue;

        [Header("Market")]
        [SerializeField] private AgentsQueue _marketImport1Queue;
        [SerializeField] private AgentsQueue _marketExport2Queue;
        [SerializeField] private AgentsQueue _marketImport3Queue;

        [Space]
        [SerializeField] private List<Agent> _agents;        

        private void Start()
        {
            var orderedNearestesAgentsToQueue = _agents.OrderBy((agent) => Vector3.Distance(agent.transform.position, _station1Export1Queue.transform.position));
            List<Agent> sortedNearestesAgentsToQueue = orderedNearestesAgentsToQueue.ToList();

            #region Binding to Movable1
            List<Agent> forMovable1Queue = orderedNearestesAgentsToQueue
                //.Where((agent) => agent.AgentSpecialization == AgentSpecialization.Coal)
                .Take(4)
                .ToList();

            for (int i = 0; i < forMovable1Queue.Count; i++)
            {
                if (i == 2)
                {
                    forMovable1Queue[i].AddDestinationQueue(_station1Export1Queue);
                    forMovable1Queue[i].AddDestinationQueue(_marketImport1Queue);
                    forMovable1Queue[i].GoToNextQueue();
                    continue;
                }

                forMovable1Queue[i].AddDestinationQueue(_station1Export1Queue);
                forMovable1Queue[i].AddDestinationQueue(_station2Import1Queue);
                forMovable1Queue[i].GoToNextQueue();
            }
            #endregion Binding to Movable1

            #region Binding to Movable2
            List<Agent> forMovable2Queue = sortedNearestesAgentsToQueue
                .Except(forMovable1Queue)
                //.Where((agent) => agent.AgentSpecialization == AgentSpecialization.Copper)
                .Take(4)
                .ToList();

            for (int i = 0; i < forMovable2Queue.Count; i++)
            {
                if (i == 2)
                {
                    forMovable2Queue[i].AddDestinationQueue(_marketExport2Queue);
                    forMovable2Queue[i].AddDestinationQueue(_station2Import2Queue);
                    forMovable2Queue[i].GoToNextQueue();
                    continue;
                }

                forMovable2Queue[i].AddDestinationQueue(_receiver2Export2Queue);
                forMovable2Queue[i].AddDestinationQueue(_station2Import2Queue);
                forMovable2Queue[i].GoToNextQueue();
            }
            #endregion Binding to Movable2

            #region Binding to Movable3
            List<Agent> forMovable3Queue = sortedNearestesAgentsToQueue
                .Except(forMovable1Queue)
                .Except(forMovable2Queue)
                //.Where((agent) => agent.AgentSpecialization == AgentSpecialization.Copper)
                .Take(4)
                .ToList();

            for (int i = 0; i < forMovable3Queue.Count; i++)
            {
                if (i == 2)
                {
                    forMovable3Queue[i].AddDestinationQueue(_station2Export3Queue);
                    forMovable3Queue[i].AddDestinationQueue(_marketImport3Queue);
                    forMovable3Queue[i].GoToNextQueue();
                    continue;
                }

                forMovable3Queue[i].AddDestinationQueue(_station2Export3Queue);
                forMovable3Queue[i].AddDestinationQueue(_receiver1Import3Queue);
                forMovable3Queue[i].GoToNextQueue();
            }
            #endregion Binding to Movable3
        }
    }
}
