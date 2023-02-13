using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace SUBS.AgentsAndSystems
{
    internal class MovableCreator4 : MovableCreatorBase
    {
        [ChildGameObjectsOnly]
        [SerializeField] private Importer _importer11;

        [ChildGameObjectsOnly]
        [SerializeField] private Importer _importer12;

        [ChildGameObjectsOnly]
        [SerializeField] private Importer _importer3;

        protected override void Create()
        {
            MovableObject importedMovable11 = _importer11.PeekMovable();
            MovableObject importedMovable12 = _importer12.PeekMovable();
            MovableObject importedMovable3 = _importer3.PeekMovable();

            ForMovablePlace forExportPlace = null;
            Exporter selectedExporter = null;

            foreach (Exporter exporter in _exporters)
            {
                forExportPlace = exporter.GetEmptyPlaceForExport();

                if (forExportPlace != null)
                {
                    selectedExporter = exporter;
                    break;
                }
            }

            if (importedMovable11 == null
             || importedMovable12 == null
             || importedMovable3  == null)
            {
                Debug.LogError("No imported movable");
                return;
            }

            if (forExportPlace == null)
            {
                Debug.LogError("No empty places for export");
                return;
            }

            importedMovable11.Place.GetObject();
            importedMovable11.Destroy();

            importedMovable12.Place.GetObject();
            importedMovable12.Destroy();

            importedMovable3.Place.GetObject();
            importedMovable3.Destroy();

            MovableObject movable = Instantiate(_movablePrefab, forExportPlace.transform.position, Quaternion.identity);
            movable.Init();
            forExportPlace.SetObject(movable, 0);
            selectedExporter.TrySetMovable(movable);
        }

        protected override bool CheckCanCreate()
        {
            if (_importer11.PeekMovable() == null)
                return false;

            if (_importer12.PeekMovable() == null)
                return false;

            if (_importer3.PeekMovable() == null)
                return false;

            ForMovablePlace place;

            foreach (Exporter exporter in _exporters)
            {
                place = exporter.GetEmptyPlaceForExport();

                if (place != null)
                    return true;
            }

            return false;
        }
    }
}
