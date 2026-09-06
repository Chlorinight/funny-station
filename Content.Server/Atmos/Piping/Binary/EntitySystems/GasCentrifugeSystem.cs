
// Server
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Monitor.Systems;
using Content.Server.Atmos.Piping.Binary.Components;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.Components;
// Shared
using Content.Shared.Atmos;
using Content.Shared.Atmos.Piping.Binary.Components;
using Content.Shared.Atmos.Piping.Components;
using Content.Shared.Atmos.Visuals;
using Content.Shared._Funkystation.Atmos.Visuals;
// Nodes
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
// Others
using Content.Server.Power.Components;
using Content.Shared.Audio;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Power;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server.Atmos.Piping.Binary.EntitySystems;

[UsedImplicitly]
public sealed class GasCentrifugeSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSoundSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;

    public override void Initialize()
        {
            base.Initialize();  

            SubscribeLocalEvent<GasCentrifugeComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<GasCentrifugeComponent, AtmosDeviceUpdateEvent>(OnUpdated);
            SubscribeLocalEvent<GasCentrifugeComponent, ExaminedEvent>(OnExamined);
            //SubscribeLocalEvent<GasCentrifugeComponent, ActivateInWorldEvent>(OnCentrifugeActivate);
            SubscribeLocalEvent<GasCentrifugeComponent, PowerChangedEvent>(OnPowerChanged);
        }

    private void OnInit(EntityUid uid, GasCentrifugeComponent comp, ComponentInit args)
        {
            UpdateAppearance(uid, comp);
        }

    private void OnPowerChanged(EntityUid uid, GasCentrifugeComponent comp, ref PowerChangedEvent args)
        {
            UpdateAppearance(uid, comp);
        }

    private void OnUpdated(EntityUid uid, GasCentrifugeComponent comp, ref AtmosDeviceUpdateEvent args)
        {
            if (!comp.Enabled ||
                (TryComp<ApcPowerReceiverComponent>(uid, out var power) && !power.Powered) ||
                !_nodeContainer.TryGetNodes(uid, comp.InletName, comp.OutletName, out PipeNode? inlet, out PipeNode? outlet))
            {
                _ambientSoundSystem.SetAmbience(uid, false);
                return;
            }

            if (inlet.Air.TotalMoles > 0 && inlet.Air.Temperature > comp.MinTemp)
            {
                var removed = inlet.Air.RemoveVolume(comp.RefineRate * _atmosphereSystem.PumpSpeedup() * args.dt);
                var nUF6 = removed.GetMoles(Gas.UF6);
                removed.AdjustMoles(Gas.CLFThree, nUF6*comp.Efficiency);
                removed.AdjustMoles(Gas.UF6, 0f-(nUF6-(nUF6*comp.Efficiency)));

                comp.lastMolesTransferred = removed.TotalMoles;
                _atmosphereSystem.Merge(outlet.Air, removed);
                _ambientSoundSystem.SetAmbience(uid, true);
            }
            

            UpdateAppearance(uid, comp);
            _ambientSoundSystem.SetAmbience(uid, true);
        }

    private void OnExamined(EntityUid uid, GasCentrifugeComponent comp, ref ExaminedEvent args)
    {
        if (!Comp<TransformComponent>(uid).Anchored || !args.IsInDetailsRange) // Not anchored? Out of range? No status.
            return;

        var str = Loc.GetString("gas-centrifuge-examined", ("flowRate", $"{comp.lastMolesTransferred:0.#}"));
        args.PushMarkup(str);
    }
    
    private void UpdateAppearance(EntityUid uid, GasCentrifugeComponent? comp = null, AppearanceComponent? appearance = null)
        {
            if (!Resolve(uid, ref comp, ref appearance, false))
                return;

            bool compOn = comp.Enabled && (TryComp<ApcPowerReceiverComponent>(uid, out var power) && power.Powered);
             _appearance.SetData(uid, PumpVisuals.Enabled, compOn);
        }
}