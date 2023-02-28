using System.ComponentModel;

namespace FrostbiteSdk.Export
{
    public class MeshExportSettings
    {
        [Description("The FBX file version to export to.")]
        public MeshExportVersion Version
        {
            get;
            set;
        }

        [Description("The FBX unit scale.")]
        public MeshExportScale Scale
        {
            get;
            set;
        }

        [Description("Whether to flatten all FBX nodes into the root node of the hierarchy.")]
        public bool FlattenHierarchy
        {
            get;
            set;
        }
    }
}
