// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Atmos;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server._Funkystation.Atmos.Reactions;

[UsedImplicitly]
public sealed partial class FollyProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        if (mixture.Temperature > 20f && mixture.GetMoles(Gas.HyperNoblium) >= 5f)
            return ReactionResult.NoReaction;

        var initPlasma = mixture.GetMoles(Gas.Plasma);
        var initWater = mixture.GetMoles(Gas.WaterVapor);
        var initFolly = mixture.GetMoles(Gas.Folly);

        var temperature = mixture.Temperature;
        var heatEfficiency = Math.Min(temperature * 0.005f, Math.Min(initFolly * 0.2f, Math.Min(initWater * 0.9f, initPlasma * 0.9f)));

        if (heatEfficiency <= 0 || initWater - heatEfficiency * 0.9f < 0 || initPlasma - heatEfficiency * 0.9f < 0 || initFolly - heatEfficiency * 0.2f < 0 )
            return ReactionResult.NoReaction;

        mixture.AdjustMoles(Gas.Plasma, -heatEfficiency * 0.9f);
        mixture.AdjustMoles(Gas.WaterVapor, -heatEfficiency * 0.9f);
        mixture.AdjustMoles(Gas.Folly, heatEfficiency * 1.8f);

        var energyReleased = heatEfficiency * Atmospherics.FollyProductionEnergy;

        var heatCap = atmosphereSystem.GetHeatCapacity(mixture, true);
        if (heatCap > Atmospherics.MinimumHeatCapacity)
            mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);

        return ReactionResult.Reacting;
    }
}