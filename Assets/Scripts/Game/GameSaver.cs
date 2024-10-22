using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Game/Game Saver")]
	public class GameSaver : Singleton<GameSaver>
	{
		public enum Mode { Binary, JSON, PlayerPrefs }

		public Mode mode = Mode.Binary;
		public string fileName = "save";
		public string binaryFileExtension = "data";

		/// <summary>
		/// The amount of available slots to save data to.
		/// </summary>
		protected static readonly int TotalSlots = 5;

		/// <summary>
		/// Persists a given Game Data on a disk slot.
		/// </summary>
		/// <param name="data">The Game Data you want to persist.</param>
		/// <param name="index">The index of the slot.</param>
		public virtual void Save(GameData data, int index)
		{
			switch (mode)
			{
				default:
				case Mode.Binary:
					SaveBinary(data, index);
					break;
				case Mode.JSON:
					SaveJSON(data, index);
					break;
				case Mode.PlayerPrefs:
					SavePlayerPrefs(data, index);
					break;
			}
		}

		/// <summary>
		/// Returns the Game Data or null by reading a given slot.
		/// </summary>
		/// <param name="index">The index of the slot you want to read.</param>
		public virtual GameData Load(int index)
		{
			switch (mode)
			{
				default:
				case Mode.Binary:
					return LoadBinary(index);
				case Mode.JSON:
					return LoadJSON(index);
				case Mode.PlayerPrefs:
					return LoadPlayerPrefs(index);
			}
		}

		/// <summary>
		/// Erases the data from a slot.
		/// </summary>
		/// <param name="index">The index of the slot you want to erase.</param>
		public virtual void Delete(int index)
		{
			switch (mode)
			{
				default:
				case Mode.Binary:
				case Mode.JSON:
					DeleteFile(index);
					break;
				case Mode.PlayerPrefs:
					DeletePlayerPrefs(index);
					break;
			}
		}

		/// <summary>
		/// Returns an array of Game Data from all the slots.
		/// </summary>
		/// <returns></returns>
		public virtual GameData[] LoadList()
		{
			var list = new GameData[TotalSlots];

			for (int i = 0; i < TotalSlots; i++)
			{
				var data = Load(i);

				if (data != null)
				{
					list[i] = data;
				}
			}

			return list;
		}

		protected virtual void SaveBinary(GameData data, int index)
		{
			var path = GetFilePath(index);
			var formatter = new BinaryFormatter();
			var stream = new FileStream(path, FileMode.Create);
			formatter.Serialize(stream, data);
			stream.Close();
		}

		protected virtual GameData LoadBinary(int index)
		{
			var path = GetFilePath(index);

			if (File.Exists(path))
			{
				var formatter = new BinaryFormatter();
				var stream = new FileStream(path, FileMode.Open);
				var data = formatter.Deserialize(stream);
				stream.Close();
				return data as GameData;
			}

			return null;
		}

		protected virtual void SaveJSON(GameData data, int index)
		{
			var json = data.ToJson();
			var path = GetFilePath(index);
			File.WriteAllText(path, json);
		}

		protected virtual GameData LoadJSON(int index)
		{
			var path = GetFilePath(index);

			if (File.Exists(path))
			{
				var json = File.ReadAllText(path);
				return GameData.FromJson(json);
			}

			return null;
		}

		protected virtual void DeleteFile(int index)
		{
			var path = GetFilePath(index);

			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		protected virtual void SavePlayerPrefs(GameData data, int index)
		{
			var json = data.ToJson();
			var key = index.ToString();
			PlayerPrefs.SetString(key, json);
		}

		protected virtual GameData LoadPlayerPrefs(int index)
		{
			var key = index.ToString();

			if (PlayerPrefs.HasKey(key))
			{
				var json = PlayerPrefs.GetString(key);
				return GameData.FromJson(json);
			}

			return null;
		}

		protected virtual void DeletePlayerPrefs(int index)
		{
			var key = index.ToString();

			if (PlayerPrefs.HasKey(key))
			{
				PlayerPrefs.DeleteKey(key);
			}
		}

		protected virtual string GetFilePath(int index)
		{
			var extension = mode == Mode.JSON ? "json" : binaryFileExtension;
			return Application.persistentDataPath + $"/{fileName}_{index}.{extension}";
		}
	}
}
