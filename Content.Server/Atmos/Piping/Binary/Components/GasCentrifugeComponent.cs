
using Content.Shared.Atmos;

namespace Content.Server.Atmos.Piping.Binary.Components
{
    [RegisterComponent]
    public sealed partial class GasCentrifugeComponent : Component
    {
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("enabled")]
        public bool Enabled { get; set; } = true;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("inlet")]
        public string InletName { get; set; } = "inlet";

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("outlet")]
        public string OutletName { get; set; } = "outlet";

        [DataField, ViewVariables(VVAccess.ReadWrite)]
        public float MinTemp = 0 + Atmospherics.T0C;

        [ViewVariables(VVAccess.ReadOnly)]
        [DataField("flowRate")]
        public float FlowRate { get; set; } = 0;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("RefineRate")]
        public float RefineRate { get; set; } = Atmospherics.GasCentrifugeRate;

        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("WasteAmount")]
        public float WasteAmount { get; set; } = Atmospherics.GasCentrifugeWaste/100;
    }
}
