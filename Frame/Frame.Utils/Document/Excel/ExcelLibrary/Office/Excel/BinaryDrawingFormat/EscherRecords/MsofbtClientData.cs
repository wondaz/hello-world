﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Frame.Utils.ExcelLibrary.BinaryDrawingFormat
{
	public partial class MsofbtClientData : EscherRecord
	{
		public MsofbtClientData(EscherRecord record) : base(record) { }

		public MsofbtClientData()
		{
			this.Type = EscherRecordType.MsofbtClientData;
		}

	}
}
