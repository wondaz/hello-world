using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Frame.Utils.ExcelLibrary.BinaryDrawingFormat
{
	public partial class MsofbtColorScheme : EscherRecord
	{
		public MsofbtColorScheme(EscherRecord record) : base(record) { }

		public MsofbtColorScheme()
		{
			this.Type = EscherRecordType.MsofbtColorScheme;
		}

	}
}
