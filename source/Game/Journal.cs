using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using MessagePack;

namespace Apothecary;

[MessagePackObject(AllowPrivate=true)]
public partial class Journal {
	public const int ConfirmationRequirement = 3;
	[Key("entries")] private readonly Dictionary<string, JournalEntry> entries = [];
	[Key("solved")] private readonly HashSet<JournalEntry> solved = [];
	[Key("total_confirmed")] public int TotalConfirmed { get; private set; }
	[IgnoreMember] public Action<IEnumerable<ItemModel>>? OnConfirmation;
	
	public bool IsDiscovered(ItemModel item) {
		return entries.ContainsKey(item.Id);
	}

	public bool IsConfirmed(ItemModel item) {
		return entries.GetValueOrDefault(item.Id)?.Confirmed == true;
	}

	public void Discover(ItemModel item) {
		entries.TryAdd(item.Id, new JournalEntry(item));
	}

	public JournalEntry? Get(ItemModel item) {
		return entries.GetValueOrDefault(item.Id);
	}

	public void UpdateGuess(JournalEntry entry) {
		var existingEntry = entries.GetValueOrDefault(entry.Item.Id);
		if (existingEntry == null || existingEntry.Confirmed) return;
		entries[entry.Item.Id] = CheckGuess(entry);
	}

	public void UpdateNotes(JournalEntry entry, string? notes) {
		entries[entry.Item.Id] = entry with { Notes = notes };
	}

	private JournalEntry CheckGuess(JournalEntry entry) {
		if (entry.LocationGuess == entry.Item.WhereFound
			&& entry.ConditionGuess == entry.Item.WhenFound
			&& entry.RarityGuess == entry.Item.Rarity
		) {
			solved.Add(entry);
			if (solved.Count >= ConfirmationRequirement) {
				foreach (var solved_entry in solved) {
					if (!solved_entry.Confirmed) {
						entries[solved_entry.Item.Id] = solved_entry with { Confirmed = true };
						TotalConfirmed += 1;
					}
				}
				OnConfirmation?.Invoke(solved.Select(s => s.Item));
				solved.Clear();
				return entry with { Confirmed = true };
			}
		} else {
			solved.Remove(entry);
		}

		return entry;
	}
	
	public ImmutableList<(Aspect?, int)> GetShownAspects(IList<ItemModel> items, IList<(Aspect, int)> aspects) {
		if (aspects.Count == 0) return [];

		if (items.All(IsConfirmed)) {
			return [.. aspects.Cast<(Aspect?, int)>()];
		} else if (items.All(item => item.Aspects.FirstOrDefault().Item1 == aspects[0].Item1)) {
			return [.. aspects.Take(1).Cast<(Aspect?, int)>().Concat(aspects.Skip(1).Select(x => ((Aspect?)null, x.Item2)))];
		} else {
			return [.. aspects.Select(x => ((Aspect?)null, x.Item2))];
		}
	}
}
