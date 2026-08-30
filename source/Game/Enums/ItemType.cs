using System;

namespace Apothecary;

[Flags]
public enum ItemType {
	Raw = 1,
	Ground = 2,
	Roasted = 4,
	Infusion = 8
}
