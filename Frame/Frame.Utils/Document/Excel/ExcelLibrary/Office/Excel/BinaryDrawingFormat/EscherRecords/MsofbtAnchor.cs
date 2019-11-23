using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Frame.Utils.ExcelLibrary.BinaryDrawingFormat
{
	public partial class MsofbtAnchor : EscherRecord
	{
		public MsofbtAnchor(EscherRecord record) : base(record) { }

		public MsofbtAnchor()
		{
			this.Type = EscherRecordType.MsofbtAnchor;
		}

	}
}
