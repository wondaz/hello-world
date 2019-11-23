using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Frame.Utils.ExcelLibrary.BinaryFileFormat
{
	public partial class End : SubRecord
	{
		public End(SubRecord record) : base(record) { }

		public End()
		{
			this.Type = SubRecordType.End;
		}

	}
}
