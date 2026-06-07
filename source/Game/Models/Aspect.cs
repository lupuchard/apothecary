using System;
namespace Apothecary;

public class Aspect {
	public string Id { get; }

	private readonly Func<Aspect> mutates_into;
	public Aspect MutatesInto => mutates_into();

	public Aspect(string id, Func<Aspect> mutates_into) {
		Id = id;
		this.mutates_into = mutates_into;
	}
}
