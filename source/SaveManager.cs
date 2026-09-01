using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Godot;
using TupleAsJsonArray;
using FileAccess = Godot.FileAccess;

namespace Apothecary;

public class Profile: IComparable<Profile> {
	public required string Name { get; set; }
	public required string Filename { get; set; }
	public required DateTimeOffset Created { get; set; }
	public required DateTimeOffset LastLoaded { get; set; }
	
	public int CompareTo(Profile? other) {
		return other == null ? 1 : LastLoaded.CompareTo(other.LastLoaded);
	}
}

public class SaveMeta {
	public required List<Profile> Profiles { get; set; }
}

public class SaveManager {
	private readonly string META_FILENAME = ProjectSettings.GlobalizePath("user://save_meta.json");
	private readonly string SAVES_DIRECTORY = ProjectSettings.GlobalizePath("user://saves/");
	private SaveMeta? meta = null;
	private Profile? cur_profile = null;

	private readonly JsonSerializerOptions json_options = new() {
		Converters = {
			new TupleConverterFactory(),
		}
	};

	public Profile CreateProfile(string name) {
		var filename_base = new string(name.Where(c => !char.IsWhiteSpace(c)).ToArray());
		while (FileAccess.FileExists(GetSaveFilename(filename_base))) {
			filename_base += "2";
		}

		var new_profile = new Profile {
			Name = name,
			Filename = GetSaveFilename(filename_base),
			Created = DateTimeOffset.Now,
			LastLoaded = DateTimeOffset.Now
		};
		
		GetMeta().Profiles.Add(new_profile);
		SaveGame(new Game(), new_profile);
		SaveMeta();
		cur_profile = new_profile;
		return new_profile;
	}

	private string GetSaveFilename(string filename_base) {
		return SAVES_DIRECTORY +  filename_base + ".json";
	}

	public void DeleteProfile(Profile profile) {
		GetMeta().Profiles.Remove(profile);
		SaveMeta();
	}
	
	public void SaveGame(Game game, Profile? profile = null) {
		profile ??= cur_profile;
		if (profile == null) return;
		Directory.CreateDirectory(SAVES_DIRECTORY);
		var json = JsonSerializer.Serialize(game.State, json_options);
		File.WriteAllText(profile.Filename, json);
		profile.LastLoaded = DateTimeOffset.Now;
		SaveMeta();
	}

	public IReadOnlyList<Profile> GetProfiles() {
		return GetMeta().Profiles;
	}

	public SaveMeta GetMeta() {
		if (meta == null) {
			if (File.Exists(META_FILENAME)) {
				var json = File.ReadAllText(META_FILENAME);
				meta = JsonSerializer.Deserialize<SaveMeta>(json, json_options);
			} else {
				meta = new SaveMeta() { Profiles = [] };
			}
		}

		meta!.Profiles.Sort();
		return meta;
	}

	private void SaveMeta() {
		var json = JsonSerializer.Serialize(GetMeta(), json_options);
		File.WriteAllText(META_FILENAME, json);
	}

	public Game.GameState LoadGame(Profile profile) {
		var json = File.ReadAllText(profile.Filename);
		cur_profile = profile;
		return JsonSerializer.Deserialize<Game.GameState>(json, json_options)!;
	}
}
