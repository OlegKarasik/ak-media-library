using System.Text.Json.Serialization;

namespace MediaLibrary.Business.Properties;

[JsonDerivedType(typeof(LibraryProps), "library")]
[JsonDerivedType(typeof(ShowProps), "show")]
[JsonDerivedType(typeof(SeasonProps), "season")]
[JsonDerivedType(typeof(EpisodeProps), "episode")]
public class PropsJson
{
}
