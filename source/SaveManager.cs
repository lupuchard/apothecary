using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;
using MessagePack;
using FileAccess = Godot.FileAccess;

namespace Apothecary;

[MessagePackObject(keyAsPropertyName: true)]
public class Profile: IComparable<Profile> {
	public required string Name;
	public required string Filename;
	public required DateTimeOffset Created;
	public required DateTimeOffset LastLoaded;
	
	public int CompareTo(Profile? other) {
		return other == null ? 1 : LastLoaded.CompareTo(other.LastLoaded);
	}
}

public class SaveManager {
	private readonly string PROFILE_FILENAME = ProjectSettings.GlobalizePath("user://profiles.json");
	private readonly string SAVES_DIRECTORY = ProjectSettings.GlobalizePath("user://saves/");
	private List<Profile> profiles = [];
	private Profile? cur_profile = null;

	public Profile CreateProfile(string name) {
		GetProfiles();

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
		
		profiles.Add(new_profile);
		SaveProfiles();
		cur_profile = new_profile;
		return new_profile;
	}

	private string GetSaveFilename(string filename_base) {
		return SAVES_DIRECTORY +  filename_base + ".json";
	}

	public void DeleteProfile(Profile profile) {
		profiles.Remove(profile);
		SaveProfiles();
	}
	
	public void SaveGame(Game game, Profile? profile = null) {
		profile ??= cur_profile;
		if (profile == null) return;
		Directory.CreateDirectory(SAVES_DIRECTORY);
		using var stream = File.Open(profile.Filename, FileMode.Create);
		MessagePackSerializer.Serialize(stream, game.state);
		profile.LastLoaded = DateTimeOffset.Now;
		SaveProfiles();
	}

	public List<Profile> GetProfiles() {
		if (profiles.Count == 0 && File.Exists(PROFILE_FILENAME)) {
			using var stream = File.Open(PROFILE_FILENAME, FileMode.Open);
			profiles = MessagePackSerializer.Deserialize<List<Profile>>(stream);
		}

		profiles.Sort();
		return profiles;
	}

	private void SaveProfiles() {
		using var stream = File.Open(PROFILE_FILENAME, FileMode.Create);
		MessagePackSerializer.Serialize(stream, profiles);
	}

	public Game.GameState LoadGame(Profile profile) {
		using var stream = File.Open(profile.Filename, FileMode.Open);
		cur_profile = profile;
		return MessagePackSerializer.Deserialize<Game.GameState>(stream);
	}
}
