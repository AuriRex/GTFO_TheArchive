
using DropServer;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TheArchive.Models.Progression;
using TheArchive.Utilities;
using UnhollowerRuntimeLib;

namespace TheArchive.Models
{
    public class LocalRundownProgression
    {
		public static float ARTIFACT_HEAT_MIN { get; set; } = 0.2f;
		public static float ARTIFACT_HEAT_UNCOMPLETED_MIN { get; set; } = 0.5f;

		public Dictionary<string, Expedition> Expeditions = new Dictionary<string, Expedition>();

		public void AddSessionResults(ExpeditionSession session)
		{
			if (session == null) return;

			if (Expeditions == null) Expeditions = new Dictionary<string, Expedition>();

			Expedition expeditionEntry = GetOrAdd(Expeditions, session.ExpeditionId);

			foreach (var stateKvp in session.CurrentData.LayerStates)
            {
				Layers layer = stateKvp.Key;
				LayerState state = stateKvp.Value;


				Expedition.Layer expeditionLayer = expeditionEntry.Layers.GetOrAddLayer(layer);
				expeditionLayer.IncreaseStateAndCompletion(state);
				expeditionEntry.Layers.SetLayer(layer, expeditionLayer);
			}

			if (session.PrisonerEfficiencyCompleted)
			{
				expeditionEntry.AllLayerCompletionCount++;
			}

			// Much love to RandomKenny <3
			// https://www.youtube.com/watch?v=H00bStiiiFk
			if (session.ArtifactsCollected > 0)
            {
				var minimumHeatValue = expeditionEntry.HasBeenCompletedBefore() ? ARTIFACT_HEAT_MIN : ARTIFACT_HEAT_UNCOMPLETED_MIN;

				var newHeat = Math.Max(minimumHeatValue, expeditionEntry.ArtifactHeat - (session.ArtifactsCollected * 1.5f / 100));

				expeditionEntry.ArtifactHeat = newHeat;

				foreach (var otherExpedition in Expeditions.Values)
                {
					if (otherExpedition == expeditionEntry) continue;
					if (otherExpedition == null) continue;
					if (otherExpedition.ArtifactHeat >= 1f) continue;

					var otherHeatNew = otherExpedition.ArtifactHeat + (session.ArtifactsCollected * 0.5f / 100);
					
					otherExpedition.ArtifactHeat = Math.Min(1f, otherHeatNew);
				}
            }
		}

		public RundownProgression ToBaseGameProgression()
		{
			var rundownProgression = new RundownProgression(ClassInjector.DerivedConstructorPointer<RundownProgression>());

			rundownProgression.Expeditions = new Il2CppSystem.Collections.Generic.Dictionary<string, RundownProgression.Expedition>();

			if (Expeditions == null) Expeditions = new Dictionary<string, Expedition>();

			foreach (var expKvp in Expeditions)
            {
				var expeditionKey = expKvp.Key;
				var expedition = expKvp.Value;
				var bgExpedition = new RundownProgression.Expedition(ClassInjector.DerivedConstructorPointer<RundownProgression.Expedition>());

				bgExpedition.AllLayerCompletionCount = expedition.AllLayerCompletionCount;
				if(ArchiveMod.CurrentBuildInfo.Rundown.IsIncludedIn(Utils.RundownFlags.RundownFive.ToLatest()))
                {
					SetArtifactHeat(bgExpedition, expedition);
                }

				bgExpedition.Layers = expedition.Layers.ToBaseGameLayers();

				rundownProgression.Expeditions.Add(expeditionKey, bgExpedition);
			}

			return rundownProgression;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private void SetArtifactHeat(RundownProgression.Expedition bgExp, Expedition cExp)
        {
			bgExp.ArtifactHeat = cExp.ArtifactHeat;
        }

		public Expedition GetOrAdd(Dictionary<string, Expedition> dict, string keyName)
        {
			if(dict.TryGetValue(keyName, out Expedition value))
            {
				return value;
            }
			var exp = new Expedition();
			dict.Add(keyName, exp);
			return exp;
		}

		public class Expedition
		{
			public int AllLayerCompletionCount = 0;

			public LayerSet Layers = new LayerSet();

			public float ArtifactHeat = 1f;

			public bool HasBeenCompletedBefore() => (Layers?.Main?.CompletionCount ?? 0) > 0;

			public static Expedition FromBaseGame(RundownProgression.Expedition baseGameExpedition)
            {
				return new Expedition
				{
					AllLayerCompletionCount = baseGameExpedition.AllLayerCompletionCount,
					Layers = LayerSet.FromBaseGame(baseGameExpedition.Layers)
				};
            }

			public RundownProgression.Expedition ToBaseGame()
            {
				return new RundownProgression.Expedition(ClassInjector.DerivedConstructorPointer<RundownProgression.Expedition>())
				{
					AllLayerCompletionCount = this.AllLayerCompletionCount,
					Layers = this.Layers?.ToBaseGameLayers() ?? new LayerSet<RundownProgression.Expedition.Layer>()
				};
			}

			public class Layer
			{
				public LayerState State = LayerState.Undiscovered;

				public int CompletionCount = 0;

				public void IncreaseStateAndCompletion(LayerState newState)
                {
					if ((int)State < (int)newState)
                    {
						State = newState;
                    }
					if (newState == LayerState.Completed)
                    {
						CompletionCount++;
					}
				}

				public static Layer FromBaseGame(RundownProgression.Expedition.Layer baseGameType)
                {
					return new Layer()
					{
						State = baseGameType.State.ToCustom(),
						CompletionCount = baseGameType.CompletionCount
					};
                }

				public RundownProgression.Expedition.Layer ToBaseGame()
                {
					return new RundownProgression.Expedition.Layer
					{
						CompletionCount = this.CompletionCount,
					    State = this.State.ToBasegame()
					};
				}
            }
		}

		public class LayerSet
		{
			public Expedition.Layer Main { get; set; }
			public Expedition.Layer Secondary { get; set; }
			public Expedition.Layer Third { get; set; }

			public static LayerSet FromBaseGame(DropServer.LayerSet<RundownProgression.Expedition.Layer> baseGameLayers)
			{
				return new LayerSet
				{
					Main = Expedition.Layer.FromBaseGame(baseGameLayers.Main),
					Secondary = Expedition.Layer.FromBaseGame(baseGameLayers.Secondary),
					Third = Expedition.Layer.FromBaseGame(baseGameLayers.Third)
				};
			}

            public Expedition.Layer GetOrAddLayer(Layers layer)
            {
				var expeditionLayer = layer switch
				{
					Layers.Main => Main,
					Layers.Secondary => Secondary,
					Layers.Third => Third,
					_ => throw new Exception($"Unknown layer enum {layer}"),
				};
				return expeditionLayer ?? new Expedition.Layer();
			}

			public void SetLayer(Layers layer, Expedition.Layer data)
            {
				switch (layer)
				{
					case Layers.Main:
						Main = data;
						break;
					case Layers.Secondary:
						Secondary = data;
						break;
					case Layers.Third:
						Third = data;
						break;
					default:
						throw new Exception($"Unknown layer enum {layer}");
				}
			}

			public DropServer.LayerSet<RundownProgression.Expedition.Layer> ToBaseGameLayers()
            {
				return new DropServer.LayerSet<RundownProgression.Expedition.Layer>
				{
					Main = this.Main?.ToBaseGame() ?? new RundownProgression.Expedition.Layer(),
					Secondary = this.Secondary?.ToBaseGame() ?? new RundownProgression.Expedition.Layer(),
					Third = this.Third?.ToBaseGame() ?? new RundownProgression.Expedition.Layer()
				};
			}
        }

	}
}
