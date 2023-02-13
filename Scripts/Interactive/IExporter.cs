namespace SUBS.AgentsAndSystems
{
    internal interface IExporter
    {
        void Export();
        void RemoveImporter(IContainerForMovable carrier);
    }
}
