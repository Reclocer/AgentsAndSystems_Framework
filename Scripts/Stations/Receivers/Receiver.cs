using System;
using System.Collections.Generic;
using UnityEngine;
using SUBS.Core;
using SUBS.Core.Messenger;
using SUBS.Core.Components;
using SUBS.Core.Testing;
using Cysharp.Threading.Tasks;
using System.Threading;
using Zenject;
using Sirenix.OdinInspector;

namespace SUBS.AgentsAndSystems
{
#pragma warning disable 4014

    internal class Receiver : MonoBehaviour, ITestable
    {
#if UNITY_EDITOR
        [FoldoutGroup("ForTesting")]
        [HideInPrefabAssets]
        [SerializeField] protected bool _testing;
        public bool Testing => _testing;

        [FoldoutGroup("ForTesting")]
        [HideInPrefabAssets]
        [SerializeField] protected int _onStartCreateCount = 0;
#endif
          
        [Space]
        [SerializeField] protected MovableID _movableID = MovableID.Unknown;

        [SerializeField] protected float _interactDelay = 0.5f;
        protected float _curInteractDelay = 0.5f;

        //[SerializeField] protected int _receiveCapacity = 8;
        [SerializeField] protected bool _createHalfCapacityOnStart;

        [AssetsOnly]
        [SerializeField] protected MovableObject _movablePrefab;
        //[SerializeField] protected TextMeshProUGUI _movablesCountText;
        protected Stack<MovableObject> _receivedObjects;

        [SceneObjectsOnly]
        [SerializeField] protected Exporter _exporter;

        [SceneObjectsOnly]
        [SerializeField] protected Importer _importer;

        [Space]
        [SerializeField] protected List<ForMovablePlace> _receiverPlaces;

        protected InteractionState _curInteractionState = InteractionState.Special;
        public InteractionState CurInteractionState => _curInteractionState;

        protected CancellationTokenSource _interactionToken = new CancellationTokenSource();

        #region MonoBehaviour
        protected virtual void OnEnable()
        {
            Messenger.AddListener<float>(EventName_SUBSCore.SimulationSpeedChanged.ToString(), OnSimulationSpeedChanged);
        }

        protected virtual void OnDisable()
        {
            Messenger.RemoveListener<float>(EventName_SUBSCore.SimulationSpeedChanged.ToString(), OnSimulationSpeedChanged);
        }

        protected virtual void Start()
        {
            _receivedObjects = new Stack<MovableObject>(/*_receiveCapacity*/_receiverPlaces.Count);
            _curInteractDelay = _interactDelay;
            NeedInteract();

            if (_createHalfCapacityOnStart)
                CreateHalfCapacity();

#if UNITY_EDITOR
            if (_testing)
            {
                ForTest();
                //InvokeRepeating(nameof(ForTest), 2, 2);
            }
#endif
        }
        #endregion MonoBehaviour

#if UNITY_EDITOR
        private void ForTest()
        {
            int i = 0;

            while (i < _receiverPlaces.Count/2)
            {
                //ForCreatedPlace place = GetEmptyCreatedPlace();

                //if (place == null)
                //{
                //    Debug.Log("No empty places");
                //    return;
                //}

                i++;
                ForMovablePlace place = GetEmptyPlace();

                if (place != null)
                {
                    MovableObject obj = Instantiate(_movablePrefab, place.transform.position, Quaternion.identity, place.transform);
                    _receivedObjects.Push(obj);
                    //_movablesCountText.text = $"{_receivedObjects.Count}/{_receiveCapacity}";
                    place.SetObject(obj);
                }
            }
        }
#endif

        protected virtual void OnSimulationSpeedChanged(float speed)
        {
            if (speed > 0)
            {
                if (_curInteractDelay == 0)
                {
                    _curInteractDelay = _interactDelay / speed;
                    NeedInteract();
                }
                else
                {
                    _curInteractDelay = _interactDelay / speed;
                }
            }
            else
            {
                _interactionToken.Cancel();
            }

            //_interactDelay *= speed;
        }

        protected virtual void CreateHalfCapacity()
        {
            int i = 0;

            while (i < _receiverPlaces.Count / 2)
            {
                i++;
                ForMovablePlace place = GetEmptyPlace();

                if (place != null)
                {
                    MovableObject movable = Instantiate(_movablePrefab, place.transform.position, Quaternion.identity, place.transform);
                    _receivedObjects.Push(movable);
                    //_movablesCountText.text = $"{_receivedObjects.Count}/{_receiveCapacity}";
                    place.SetObject(movable);
                }
            }
        }

        protected async virtual UniTaskVoid NeedInteract()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_curInteractDelay), false, default, _interactionToken.Token);
            NeedInteract();

            Interact();
        }

        protected virtual void Interact()
        {
            MovableObject movable = _importer.PeekMovable();
            ForMovablePlace place = _exporter.GetEmptyPlaceForExport();

            if (movable != null)
            {

                if (place != null)
                {
                    _importer.GetMovable();
                    movable.Place.GetObject();
                    place.SetObject(movable, 0);
                    _exporter.TrySetMovable(movable);
                }
                else if (_receivedObjects.Count < /*_receiveCapacity*/_receiverPlaces.Count)
                {
                    _importer.GetMovable();
                    movable.Place.GetObject();
                    SetMovable(movable);
                }
            }
            else if (_receivedObjects.Count > 0)
            {
                if (place != null)
                {
                    MovableObject movable1 = GetMovable();
                    place.SetObject(movable1);
                    _exporter.TrySetMovable(movable1);
                }
            }
        }

        /// <summary>
        /// Return true if Receiver is not full
        /// </summary>
        /// <returns></returns>
        internal virtual bool CheckReceiverCapacity()
        {
            if (_receivedObjects.Count >= /*_receiveCapacity*/_receiverPlaces.Count)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        internal virtual bool CheckGetting(int count = 1)
        {
            if (_receivedObjects.Count >= count)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        internal virtual MovableObject GetMovable()
        {
            if (_receivedObjects.Count <= 0)
                return null;

            MovableObject movable = _receivedObjects.Pop();
            //_movablesCountText.text = $"{_receivedObjects.Count}/{_receiveCapacity}";
            ForMovablePlace place = movable.Place;

            if (place != null)
            {
                place.GetObject();
            }
            else
            {
                Debug.Log("Place not found");
            }

            return movable;
        }

        internal virtual void SetMovable(MovableObject movable)
        {
            if (_receivedObjects.Count >= /*_receiveCapacity*/_receiverPlaces.Count)
            {
                Debug.Log("Receiver is full");
                return;
            }

            _receivedObjects.Push(movable);
            //_movablesCountText.text = $"{_receivedObjects.Count}/{_receiveCapacity}";   
            ForMovablePlace place = GetEmptyPlace();

            if (place == null)
            {
                Debug.LogError("Place = null");
                return;
            }

            place.SetObject(movable);
        }    

        protected virtual ForMovablePlace GetEmptyPlace()
        {
            foreach (ForMovablePlace place in _receiverPlaces)
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
    }
}
