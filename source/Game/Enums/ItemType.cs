using System;
using Serde;

namespace Apothecary;

[Flags]
[GenerateSerde]
public enum ItemType {
	Raw = 1,
	Ground = 2,
	Roasted = 4,
	Infusion = 8
}
