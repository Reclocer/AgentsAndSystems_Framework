using System;
using System.Collections.Generic;
using UnityEngine;
using SUBS.Core;
using SUBS.Core.Messenger;
using SUBS.Core.Components;
using SUBS.Core.Testing;
using UnityEngine.AI;
//using NaughtyAttributes;
using Sirenix.OdinInspector;

namespace SUBS.AgentsAndSystems
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NavMeshAgent))]
    internal partial class Agent : MonoBehaviour, ICarrier, ITestable
    {
        [HideInPrefabAssets]
        [SerializeField] protected bool _testing;
        public bool Testing => _testing;

        [SerializeField] private CarrierType _carrierType = CarrierType.Wheeled;
        public CarrierType CarrierType => _carrierType;

        [NaughtyAttributes.EnumFlags]
        [SerializeField] protected AgentSpecialization _agentSpecialization = AgentSpecialization.Coal;
        internal AgentSpecialization AgentSpecialization => _agentSpecialization;

        [SerializeField] protected int _maxMovablesCount = 3;
        protected Stack<MovableObject> _movableObjects = new Stack<MovableObject>(8);

        [Space][ChildGameObjectsOnly]
        [SerializeField] protected List<ForMovablePlace> _forMovablePlaces;

        //[SerializeField] private Animator _animator;
        protected NavMeshAgent _agent;
        protected AgentState _state = AgentState.Unknown;

        protected InteractionState _curInteractionState = InteractionState.Special;
        public InteractionState CurInteractionState => _curInteractionState;

        protected IExporter _exporter;
        protected IImporter _importer;
        protected MovableObject _lastObject;
                
        internal Action<Agent> OnLeaveTheQueueCallBack = (a) => { };

        #region MonoBehaviour
        protected virtual void OnEnable()
        {
            Messenger.AddListener<float>(EventName_SUBSCore.SimulationSpeedChanged.ToString(), OnSimulationSpeedChanged);
        }

        protected virtual void OnDisable()
        {
            Messenger.RemoveListener<float>(EventName_SUBSCore.SimulationSpeedChanged.ToString(), OnSimulationSpeedChanged);
        }

        protected virtual void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            Mover_Awake();
        }

        protected virtual void Start()
        {
            //_animator.SetTrigger("Run");            
        }

        protected virtual void Update()
        {
            Mover_Update();            
        }
        #endregion MonoBehaviour
                
        #region Operations at Movable
        public virtual MovableObject GetMovableObject(float delay = 0)
        {
            if (_movableObjects.Count <= 0)
                return null;

            MovableObject obj = _movableObjects.Pop();
            _importer.RemoveExporter(this);

            if (delay <= 0)
            {
                OnLeaveTheQueueCallBack(this);
            }
            else
            {
                StartCoroutine(this.DoAfterSeconds(() => { OnLeaveTheQueueCallBack(this); }, delay));
            }

            return obj;
        }

        public virtual MovableObject GetMovableObject( MovableID movableID, float delay = 0)
        {
            if (_movableObjects.Count <= 0)
                return null;

            foreach (MovableObject obj in _movableObjects)
            {
                if (obj.MovableID == movableID)
                {
                    _movableObjects.Pop();
                    _importer.RemoveExporter(this);

                    if (delay <= 0)
                    {
                        OnLeaveTheQueueCallBack(this);
                    }
                    else
                    {
                        StartCoroutine(this.DoAfterSeconds(() => { OnLeaveTheQueueCallBack(this); }, delay));
                    }

                    return obj;
                }
            }

            return null;

            //for stack list movables
            //if (_movableObjects.Count <= 0)
            //    return null;

            //List<ForCreatedPlace> places = _placesInContainer.ToList();
            //places.Reverse();
            //bool needDownReplace = false;
            //bool isSelected = false;
            //MovableObject selectedObj = _movableObjects[0];
            //ForCreatedPlace selectedPlace;
            //int selectedIndex = _placesInContainer.Count - 1;

            //foreach (ForCreatedPlace place1 in places)
            //{
            //    if (!place1.EmptyPlace)
            //    {
            //        MovableObject obj = place1.GetObject(movableID);

            //        if (obj != null)
            //        {
            //            _movableObjects.Remove(obj);

            //            if (_movableObjects.Count == 0 && _unitController != null)
            //                _unitController.DisableOnCarrierAnimation();

            //            selectedObj = obj;
            //            selectedPlace = place1;
            //            isSelected = true;
            //            selectedIndex = _placesInContainer.IndexOf(place1);
            //            break;
            //        }
            //        else
            //        {
            //            needDownReplace = true;
            //        }
            //    }
            //}

            //if (needDownReplace)
            //{
            //    for (int i = selectedIndex; i < _placesInContainer.Count - 1; i++)
            //    {
            //        MovableObject obj = _placesInContainer[i + 1].GetObject();

            //        if (obj != null)
            //        {
            //            _placesInContainer[i].SetObject(obj);
            //        }
            //        else
            //        {
            //            Debug.Log("Cant replace");
            //        }
            //    }
            //}

            //if (isSelected)
            //    return selectedObj;

            //return null;
        }

        public virtual bool SetMovableObject(MovableObject movable, float delay = 0)
        {
            if (_movableObjects.Count >= _maxMovablesCount)
                return false;

            _movableObjects.Push(movable);
            ForMovablePlace place = GetEmptyPlace();

            if (place == null)
            {
                Debug.Log("No empty places");
                return false;
            }

            movable.Place.GetObject();
            place.SetObject(movable);
            _exporter.RemoveImporter(this);
            _exporter = null;

            if (delay <= 0)
            {
                OnLeaveTheQueueCallBack(this);
            }
            else
            {
                StartCoroutine(this.DoAfterSeconds(() => { OnLeaveTheQueueCallBack(this); }, delay));
            }

            return true;
        }

        public virtual bool CheckCapacity()
        {
            if (_movableObjects.Count >= _maxMovablesCount)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        internal virtual void AddMaxMovablesCount(int count)
        {
            _maxMovablesCount += count;
        }

        protected virtual ForMovablePlace GetEmptyPlace()
        {
            foreach (ForMovablePlace place in _forMovablePlaces)
            {
                if (place.EmptyPlace)
                {
                    if (place != null)
                    {
                        return place;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        protected virtual ForMovablePlace GetEmptyPlace(MovableID forMovableType)
        {
            foreach (ForMovablePlace place in _forMovablePlaces)
            {
                if (place.EmptyPlace 
                && (place.ForMovableType == MovableID.Unknown || place.ForMovableType == forMovableType))
                {
                    if (place != null)
                    {
                        return place;
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return null;
        }
        #endregion Operations at Movable

        internal virtual void SetExporter(IExporter exporter)
        {
            _exporter = exporter;
        }

        internal virtual void SetImporter(IImporter importer)
        {
            _importer = importer;
        }        
    }
}
