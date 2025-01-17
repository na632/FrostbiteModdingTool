﻿using System.Globalization;
using System.Windows.Controls;

namespace FMT.Pages.Common.EBX.Validation
{
    internal class ValidationRuleFloat : ValidationRule
    {
        public ValidationRuleFloat()
        {
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return float.TryParse(value.ToString(), out _) ? ValidationResult.ValidResult : new ValidationResult(false, "Input is not a valid Float");
        }
    }
}
