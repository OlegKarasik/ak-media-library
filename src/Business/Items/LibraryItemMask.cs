namespace MediaLibrary.Business.Items;

[Flags]
public enum LibraryItemMask
{
  None      = 0b00000000,
  Episodes  = 0b00000001,
  Movies    = 0b00000010,
  Seasons   = 0b00000100,
  Shows     = 0b00001000,
  Libraries = 0b00010000
}
