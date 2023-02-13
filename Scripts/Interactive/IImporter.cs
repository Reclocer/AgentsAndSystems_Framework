namespace SUBS.AgentsAndSystems
{
    internal interface IImporter
    {
        bool TryImport();
        void RemoveExporter(IContainerForMovable carrier);
    }
}
