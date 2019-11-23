using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frame.Utils.EPPlus.DataValidation.Formulas.Contracts;

namespace Frame.Utils.EPPlus.DataValidation.Contracts
{
    public interface IExcelDataValidationInt : IExcelDataValidationWithFormula2<IExcelDataValidationFormulaInt>, IExcelDataValidationWithOperator
    {
    }
}
