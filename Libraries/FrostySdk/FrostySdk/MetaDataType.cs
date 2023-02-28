namespace FrostySdk
{
    public struct MetaDataType
    {
        private DbObject meta;

        public string DisplayName => meta.GetValue("displayName", "");

        public string Description => meta.GetValue("description", "");

        public string Category => meta.GetValue("category", "");

        public string Editor => meta.GetValue("editor", "");

        public string ValueConverter => meta.GetValue("valueConverter", "");

        public bool IsAbstract => meta.GetValue("abstract", defaultValue: false);

        public bool IsTransient => meta.GetValue("transient", defaultValue: false);

        public bool IsReadOnly => meta.GetValue("readOnly", defaultValue: false);

        public bool IsReference => meta.GetValue("reference", defaultValue: false);

        public bool IsInline => meta.GetValue("inline", defaultValue: false);

        public bool IsProperty => false;

        public MetaDataType(DbObject inMeta)
        {
            meta = inMeta;
        }
    }
}
