namespace SUBS.AgentsAndSystems
{
    internal interface IExporterAndImporter : IExporter, IImporter 
    {
        InteractionState CurInteractionState { get; }
    }
}
