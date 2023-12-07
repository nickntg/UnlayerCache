using System.Collections.Generic;

namespace UnlayerCache.API.Models
{
	public class UnlayerMergeTags
	{
		public Dictionary<string, string> mergeTags { get; set; }
		public List<Dictionary<string, string>> repeatMergeTags { get; set; }
	}
}
