using DropServer;

namespace TheArchive.Models.Progression
{
    public static class LayerExtensions
    {

        public static Layers ToCustom(this ExpeditionLayers layer) => (Layers)(int)layer;
        public static ExpeditionLayers ToBasegame(this Layers layer) => (ExpeditionLayers)(int)layer;

        public static LayerState ToCustom(this LayerProgressionState state) => (LayerState)(int)state;
        public static LayerProgressionState ToBasegame(this LayerState layer) => (LayerProgressionState)(int)layer;

    }
}
