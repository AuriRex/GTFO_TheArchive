
using DropServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArchive.Utilities;
using UnhollowerBaseLib.Attributes;

namespace TheArchive.Models
{
	[Serializable]
    public class CustomRundownProgression
    {

		[JsonIgnore]
		public static JsonSerializerSettings Settings = new JsonSerializerSettings()
		{
			Formatting = Formatting.Indented,
			DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
			MissingMemberHandling = MissingMemberHandling.Ignore
		};

		[Obsolete("Just don't")]
		public static CustomRundownProgression FromBaseGameProgression(RundownProgression baseGameRundownProgression)
        {
			var customRundownProgression = new CustomRundownProgression();

			foreach(Il2CppSystem.Collections.Generic.KeyValuePair<string, RundownProgression.Expedition> kvp in baseGameRundownProgression.Expeditions)
            {
				customRundownProgression.Expeditions.Add(kvp.Key, Expedition.FromBaseGame(kvp.value));
            }

			return customRundownProgression;
		}

		public static CustomRundownProgression FromJSON(string json)
        {
			return JsonConvert.DeserializeObject<CustomRundownProgression>(json, Settings);
        }

		private static RundownProgressionResult rundownProgressionResult = new RundownProgressionResult();
		public static RundownProgression JSONToRundownProgression(string json)
		{
			rundownProgressionResult.EscapedRundownProgression = json;
			return rundownProgressionResult.GetRundownProgression();
		}


		public RundownProgression ToBaseGameProgression()
		{

			string json = JsonConvert.SerializeObject(this, Settings);

			return JSONToRundownProgression(json);
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
				var customExpedition = new Expedition();

				customExpedition.AllLayerCompletionCount = baseGameExpedition.AllLayerCompletionCount;

				customExpedition.Layers = LayerSet.FromBaseGame(baseGameExpedition.Layers);

				return customExpedition;
            }

			public RundownProgression.Expedition ToBaseGameExpedition()
            {
				var baseGameExpedition = new RundownProgression.Expedition()
				{
					AllLayerCompletionCount = this.AllLayerCompletionCount,
					Layers = this.Layers.ToBaseGameLayers()
				};

				//baseGameExpedition.AllLayerCompletionCount = this.AllLayerCompletionCount;
				//baseGameExpedition.Layers = this.Layers.ToBaseGameLayers();
				//baseGameExpedition.Layers = null;

				return baseGameExpedition;
			}

			[Serializable]
			public struct Layer// : IToBaseGameConvertible<RundownProgression.Expedition.Layer, Layer>
			{
				public LayerProgressionState State;

				public int CompletionCount;

				public static Layer FromBaseGameType(RundownProgression.Expedition.Layer baseGameType)
                {
					var layer = new Layer();
					layer.State = baseGameType.State;
					layer.CompletionCount = baseGameType.CompletionCount;
					ArchiveLogger.Msg(ConsoleColor.Yellow, $"Created New Layer: State: C:{layer.State} - BG:{baseGameType.State}");
					ArchiveLogger.Msg(ConsoleColor.Yellow, $"^ ^ ^            : CompletionCount: C:{layer.CompletionCount} - BG:{baseGameType.CompletionCount}");
					return layer;
                }

				public RundownProgression.Expedition.Layer ToBaseGameType()
                {
					var bgt = new RundownProgression.Expedition.Layer();

					bgt.CompletionCount = this.CompletionCount;
					bgt.State = this.State;

					return bgt;
				}
            }
		}

		[Serializable]
		public struct LayerSet //where TData : struct, IToBaseGameConvertible<RundownProgression.Expedition.Layer, TData>
		{
			public Expedition.Layer Main { get; set; }
			public Expedition.Layer Secondary { get; set; }
			public Expedition.Layer Third { get; set; }

			public static LayerSet FromBaseGame(DropServer.LayerSet<RundownProgression.Expedition.Layer> baseGameLayers)
			{
				var layers = new LayerSet();


				layers.Main = Expedition.Layer.FromBaseGameType(baseGameLayers.Main);
				layers.Secondary = Expedition.Layer.FromBaseGameType(baseGameLayers.Secondary);
				layers.Third = Expedition.Layer.FromBaseGameType(baseGameLayers.Third);

				return layers;
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
				var baseGameLayers = new DropServer.LayerSet<RundownProgression.Expedition.Layer>();

				baseGameLayers.Main = this.Main.ToBaseGameType();
				baseGameLayers.Secondary = this.Secondary.ToBaseGameType();
				baseGameLayers.Third = this.Third.ToBaseGameType();

				return baseGameLayers;
			}
        }

	}

    /*public interface IToBaseGameConvertible<BGT, CT>
    {
		public abstract BGT ToBaseGameType();
		public abstract CT FromBaseGameType(BGT baseGameType);
	}*/
}
