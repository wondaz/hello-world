﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frame.Utils.EPPlus.DataValidation.Formulas.Contracts;

namespace Frame.Utils.EPPlus.DataValidation.Contracts
{
    /// <summary>
    /// Validation interface for datetime validations
    /// </summary>
    public interface IExcelDataValidationDateTime : IExcelDataValidationWithFormula2<IExcelDataValidationFormulaDateTime>, IExcelDataValidationWithOperator
    {
    }
}
