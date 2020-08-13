using FrostySdk;
using FrostySdk.IO;
using FrostySdk.Managers;

namespace FrostySdk
{
	public interface ICustomActionHandler
	{
		void SaveToMod(FrostyModWriter writer, AssetEntry entry);

		void SaveToMod(FrostyModWriter writer);

		bool SaveToProject(NativeWriter writer);

		void LoadFromProject(DbObject project);

		void LoadFromProject(uint version, NativeReader reader, string type);
	}
}
