using System;

namespace ThumbnailGenerator;

[Flags]
public enum ThumbnailOptions
{
	None = 0,
	BiggerSizeOk = 1,
	InMemoryOnly = 2,
	IconOnly = 4,
	ThumbnailOnly = 8,
	InCacheOnly = 0x10
}
