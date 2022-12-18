using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostySdk.FrostySdk.Managers
{
    public interface IAssetEntry
    {
		public string Filename { get; }

		public string Path { get; }

		public string Name { get; set; }

		public string DisplayName { get; }

        public bool IsModified { get; }

	}
}
