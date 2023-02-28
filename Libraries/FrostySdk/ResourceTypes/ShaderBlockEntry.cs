namespace FrostySdk.Resources
{
    public class ShaderBlockEntry : ShaderStaticParamDbBlock
    {
        public MeshParamDbBlock GetMeshParams(int sectionIndex)
        {
            ShaderStaticParamDbBlock section = GetSection(sectionIndex);
            if (section == null)
            {
                return null;
            }
            return section.Resources[0] as MeshParamDbBlock;
        }

        public ShaderPersistentParamDbBlock GetTextureParams(int sectionIndex)
        {
            ShaderStaticParamDbBlock section = GetSection(sectionIndex);
            if (section == null)
            {
                return null;
            }
            return (section.Resources[1] as ShaderStaticParamDbBlock).Resources[1] as ShaderPersistentParamDbBlock;
        }

        public ShaderPersistentParamDbBlock GetParams(int sectionIndex)
        {
            ShaderStaticParamDbBlock section = GetSection(sectionIndex);
            if (section == null)
            {
                return null;
            }
            return (section.Resources[1] as ShaderStaticParamDbBlock).Resources[0] as ShaderPersistentParamDbBlock;
        }

        private ShaderStaticParamDbBlock GetSection(int sectionIndex)
        {
            if (sectionIndex >= Resources.Count)
            {
                return null;
            }
            return Resources[sectionIndex] as ShaderStaticParamDbBlock;
        }
    }
}
