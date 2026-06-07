using System;

namespace Apothecary;

[Flags]
public enum ItemFindCondition {
	None = 0,

	Morning = 0x001,
	Afternoon = 0x002,
	Night = 0x004,

	AfterRaining = 0x010,

	InMoonlight = 0x100,
}
