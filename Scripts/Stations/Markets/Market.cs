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

    internal partial class Market : MonoBehaviour, ITestable
    {
        [HideInPrefabAssets]
        [SerializeField] protected bool _testing;
        public bool Testing => _testing;

        [SerializeField] protected MovableID _movableID = MovableID.Movable1;
        public MovableID MovableID => _movableID;

        [AssetsOnly]
        [SerializeField] protected MovableObject _movablePrefab;

        [Min(0.001f)]
        [SerializeField] protected float _creatingForExportDelay = 1;
        protected float _curExportDelay = 0.5f;
        protected CancellationTokenSource _exportToken = new CancellationTokenSource();

        [Min(0.001f)]
        [SerializeField] protected float _importDelay = 1;
        protected float _curImportDelay = 0.5f;
        protected CancellationTokenSource _importToken = new CancellationTokenSource();

        [ChildGameObjectsOnly]
        [SerializeField] protected List<Exporter> _exporters;

        [Space]
        [ChildGameObjectsOnly]
        [SerializeField] private List<Importer> _importers;

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
            _curExportDelay = _creatingForExportDelay;
            _curImportDelay = _importDelay;
            NeedExport();
            NeedImport();
        }

        protected virtual void OnSimulationSpeedChanged(float speed)
        {
            if (speed > 0)
            {
                if (_curExportDelay == 0)
                {
                    _curExportDelay = _creatingForExportDelay / speed;
                    NeedExport();
                }
                else
                {
                    _curExportDelay = _creatingForExportDelay / speed;
                }

                if (_curImportDelay == 0)
                {
                    _curImportDelay = _importDelay / speed;
                    NeedImport();
                }
                else
                {
                    _curImportDelay = _importDelay / speed;
                }
            }
            else
            {
                _exportToken.Cancel();
                _importToken.Cancel();
            }
        }

        protected async virtual UniTaskVoid NeedExport()
        {
            if (_exporters.Count == 0)
            {
                if (_testing)
                    Debug.Log("Cant export! Exporters.Count = 0");

                return;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_curExportDelay), false, default, _exportToken.Token);
            NeedExport();

            if (CheckCanCreate())
                Create();
        }

        protected virtual void Create()
        {
            ForMovablePlace place = null;
            Exporter selectedExporter = null;

            foreach (Exporter exporter in _exporters)
            {
                place = exporter.GetEmptyPlaceForExport();

                if (place != null)
                {
                    selectedExporter = exporter;
                    break;
                }
            }

            if (place == null)
            {
                Debug.LogError("No empty places");
                return;
            }

            if (TempUIManager.Instance.ScoreCount < _movablePrefab.Cost)
            {
                Debug.LogError($"No money for {_movableID}");
                return;
            }

            TempUIManager.Instance.TryTakeMoney(_movablePrefab.Cost);
            MovableObject movable = Instantiate(_movablePrefab, place.transform.position, Quaternion.identity);
            movable.Init();
            place.SetObject(movable, 0);
            selectedExporter.TrySetMovable(movable);
        }

        protected virtual bool CheckCanCreate()
        {
            ForMovablePlace place = null;

            foreach (Exporter exporter in _exporters)
            {
                place = exporter.GetEmptyPlaceForExport();

                if (place != null)
                    break;
            }

            if (place == null)
                return false;

            if (TempUIManager.Instance.ScoreCount >= _movablePrefab.Cost)
                return true;

            return false;
        }

        protected async virtual UniTaskVoid NeedImport()
        {
            if (_importers.Count == 0)
            {
                if (_testing)
                    Debug.Log("Cant import! Importers.Count = 0");

                return;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_curImportDelay), false, default, _importToken.Token);
            NeedImport();

            MovableObject importedMovable = null;

            foreach (Importer importer in _importers)
            {
                importedMovable = importer.PeekMovable();

                if (importedMovable != null)
                    break;
            }

            if (importedMovable == null)
            {
                return;
            }

            TempUIManager.Instance.AddMoney(importedMovable.Cost);

            importedMovable.Place.GetObject();
            importedMovable.Destroy();
        }
    }
}
