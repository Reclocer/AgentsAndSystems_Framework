using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Zenject;
using SUBS.Core.Messenger;
using SUBS.Core.Components;
using SUBS.Core.Testing;
using Sirenix.OdinInspector;

namespace SUBS.AgentsAndSystems
{
#pragma warning disable 4014

    [DisallowMultipleComponent]
    internal abstract partial class MovableCreatorBase : MonoBehaviour, ITestable
    {
        [HideInPrefabAssets]
        [SerializeField] protected bool _testing;
        public bool Testing => _testing;

        [AssetsOnly]
        [SerializeField] protected MovableObject _movablePrefab;
        [SerializeField] protected int _maxCreatedCount = 2;

        [Min(0.001f)]
        [SerializeField] protected float _creatingDelay = 1;
        protected float _curCreatingDelay = 0.5f;

        [ChildGameObjectsOnly]
        [SerializeField] protected List<Exporter> _exporters;
        protected int _nextInExportersOrder;
        protected Exporter _nextExporter;
        
        protected CancellationTokenSource _creatingToken = new CancellationTokenSource();

        protected TempComplex1 _complex1;

        [Inject]
        protected virtual void DoInjections(TempComplex1 complex1)
        {
            _complex1 = complex1;
        }

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
            _curCreatingDelay = _creatingDelay;
            NeedCreate();           
        }

        protected virtual void OnSimulationSpeedChanged(float speed)
        {
            if (speed > 0)
            {
                if (_curCreatingDelay == 0)
                {
                    _curCreatingDelay = _creatingDelay / speed;
                    NeedCreate();
                }
                else
                {
                    _curCreatingDelay = _creatingDelay / speed;
                }                
            }
            else
            {
                _creatingToken.Cancel();
            }
        }

        protected async virtual UniTaskVoid NeedCreate()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_curCreatingDelay), false, default, _creatingToken.Token);
            NeedCreate();

            if (CheckCanCreate())
                Create();
        }

        protected virtual void Create()
        {
            if (_nextExporter == null)
            {
                Debug.LogError("NextExporter = null");
                return;
            }            

            ForMovablePlace place = _nextExporter.GetEmptyPlaceForExport();           

            if (place == null)
            {
                Debug.LogError("No empty places");
                return;
            }

            MovableObject movable = Instantiate(_movablePrefab, place.transform.position, Quaternion.identity);
            movable.Init();
            place.SetObject(movable, 0);
            _nextExporter.TrySetMovable(movable);
        }

        protected virtual bool CheckCanCreate()
        {
            Exporter exporter = FindNextEmptyExporter();

            if (exporter != null)            
                return true;    
            
            return false;
        }

        protected virtual Exporter FindNextEmptyExporter()
        {
            ForMovablePlace place = null;            

            int i = 0;

            while (i < _exporters.Count)
            {
                i++;
                int j = _nextInExportersOrder;

                place = _exporters[j].GetEmptyPlaceForExport();

                if (place != null)
                {
                    _nextExporter = _exporters[j];
                    _nextInExportersOrder++;

                    if (_nextInExportersOrder >= _exporters.Count)
                        _nextInExportersOrder = 0;

                    return _exporters[j];
                }
                else
                {
                    _nextInExportersOrder++;

                    if (_nextInExportersOrder >= _exporters.Count)
                        _nextInExportersOrder = 0;
                }
            }

            return null;
        }
    }
}
