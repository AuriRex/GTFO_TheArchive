
using DropServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Models.Progression;
using TheArchive.Utilities;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;

namespace TheArchive.Models
{
    public class LocalRundownProgression
    {

		[JsonIgnore]
		public static JsonSerializerSettings Settings = new JsonSerializerSettings()
		{
			Formatting = Formatting.Indented,
			DefaultValueHandling = DefaultValueHandling.Populate,
			MissingMemberHandling = MissingMemberHandling.Ignore
		};

		public static LocalRundownProgression FromJSON(string json)
		{
			return JsonConvert.DeserializeObject<LocalRundownProgression>(json, Settings);
		}


		private static RundownProgressionResult rundownProgressionResult = new RundownProgressionResult();
		public static RundownProgression JSONToRundownProgression(string json)
		{
			rundownProgressionResult.EscapedRundownProgression = json;
			return rundownProgressionResult.GetRundownProgression();
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
		public void SetArtifactHeat(RundownProgression.Expedition bgExp, Expedition cExp)
        {
			bgExp.ArtifactHeat = cExp.ArtifactHeat;
        }


		public Dictionary<string, Expedition> Expeditions = new Dictionary<string, Expedition>();

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

		public void UpdateExpeditionCompletion(string expeditionName, ExpeditionLayerMask layerMask, bool allLayersCompleted)
		{
			if(Expeditions == null)
            {
				ArchiveLogger.Warning($"[{nameof(CustomRundownProgression)}] Expeditions dictionary was null for some reason ...");
				Expeditions = new Dictionary<string, Expedition>();
            }
			Expedition orCreate = GetOrAdd(Expeditions, expeditionName);
			foreach (ExpeditionLayers layer in RundownProgression.LayersFromMask(layerMask))
			{
				Expedition.Layer layer2 = orCreate.Layers.GetLayer(layer);
				layer2.State = LayerProgressionState.Completed;
				layer2.CompletionCount++;
				orCreate.Layers.SetLayer(layer, layer2);
			}
			if (allLayersCompleted)
			{
				orCreate.AllLayerCompletionCount++;
			}
		}

        [Serializable]
		public class Expedition
		{
			public int AllLayerCompletionCount;

			public LayerSet Layers;

			public float ArtifactHeat = 1f;

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
					Layers = this.Layers.ToBaseGameLayers()
				};
			}

			[Serializable]
			public class Layer
			{
				public LayerProgressionState State;

				public int CompletionCount;

				public static Layer FromBaseGame(RundownProgression.Expedition.Layer baseGameType)
                {
					return new Layer()
					{
						State = baseGameType.State,
						CompletionCount = baseGameType.CompletionCount
					};
                }

				public RundownProgression.Expedition.Layer ToBaseGame()
                {
					return new RundownProgression.Expedition.Layer
					{
						CompletionCount = this.CompletionCount,
					    State = this.State
					};
				}
            }
		}

		[Serializable]
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

            public Expedition.Layer GetLayer(ExpeditionLayers layer)
            {
				return layer switch
				{
					ExpeditionLayers.Main => Main,
					ExpeditionLayers.Secondary => Secondary,
					ExpeditionLayers.Third => Third,
					_ => throw new Exception($"Unknown layer enum {layer}"),
				};
			}

			public void SetLayer(ExpeditionLayers layer, Expedition.Layer data)
            {
				switch (layer)
				{
					case ExpeditionLayers.Main:
						Main = data;
						break;
					case ExpeditionLayers.Secondary:
						Secondary = data;
						break;
					case ExpeditionLayers.Third:
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
					Main = this.Main.ToBaseGame(),
					Secondary = this.Secondary.ToBaseGame(),
					Third = this.Third.ToBaseGame()
				};
			}
        }

	}
}
