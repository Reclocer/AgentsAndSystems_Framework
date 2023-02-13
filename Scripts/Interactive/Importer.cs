using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using SUBS.Core.Messenger;
using SUBS.Core.Components;
using SUBS.Core.Testing;
using Sirenix.OdinInspector;

namespace SUBS.AgentsAndSystems
{
#pragma warning disable 4014

    internal class Importer : MonoBehaviour, IImporter, ITestable
    {
        [HideInPrefabAssets]
        [SerializeField] protected bool _testing;
        public bool Testing => _testing;

        [Space]
        [Min(0.001f)]
        [SerializeField] protected float _importTime = 2;
        protected float _curImportTime = 0.5f;

        [Space]
        [Min(0.001f)]
        [SerializeField] protected float _onExportAgentDelay = 0.5f;
        protected float _curOnExportAgentDelay;

        [ChildGameObjectsOnly]
        [SerializeField] protected ForMovablePlace _importPlace;
        protected MovableObject _importedMovable;
        protected IContainerForMovable _exporter;
        protected CancellationTokenSource _importToken = new CancellationTokenSource();

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
            NeedImport();
        }

        protected virtual void OnSimulationSpeedChanged(float speed)
        {
            if (speed > 0)
            {
                if (_curImportTime == 0)
                {
                    _curImportTime = _importTime / speed;
                    NeedImport();
                }
                else
                {
                    _curImportTime = _importTime / speed;
                }   
                
                _curOnExportAgentDelay = _onExportAgentDelay / speed;
            }
            else
            {
                _importToken.Cancel();
            }
        }

        protected async virtual UniTaskVoid NeedImport()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_curImportTime), false, default, _importToken.Token);
            NeedImport();
            TryImport();
        }

        public virtual bool TryImport()
        {
            if (_exporter == null)
                return false;

            ForMovablePlace place = GetEmptyPlaceForImport();

            if (place != null)
            {
                MovableObject movable = _exporter.GetMovableObject(_curOnExportAgentDelay);
                movable.Place.GetObject();
                place.SetObject(movable);
                _importedMovable = movable;
                return true;
            }
            else
            {
                if (_testing)
                    Debug.Log("No empty place");
            }

            return false;
        }

        internal virtual MovableObject PeekMovable()
        {
            return _importedMovable;
        }

        internal virtual MovableObject GetMovable()
        {
            MovableObject movable = _importedMovable;
            _importedMovable = null;
            return movable;
        }

        internal virtual void SetExporter(IContainerForMovable exporter)
        {
            if (_exporter == null)
            {
                _exporter = exporter;
                //TryImport();
            }
            else
            {
                Debug.LogError("Creator has an Exporter");
            }
        }

        public virtual void RemoveExporter(IContainerForMovable exporter)
        {
            _exporter = null;
        }

        protected virtual ForMovablePlace GetEmptyPlaceForImport()
        {
            if (_importPlace.EmptyPlace)
            {
                return _importPlace;
            }
            else
            {
                return null;
            }
        }
    }
}