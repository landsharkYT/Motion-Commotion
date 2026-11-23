using System;
using System.Globalization;

namespace MotionCommotion.Core
{
    public static class NumericRules
    {
        public const double MaximumLiteral = 1000000;
        public const double MaximumResult = 1e18;
        public const int MaximumCharacters = 12;
        public static bool IsDraft(string text)
        {
            if (text == null || text.Length > MaximumCharacters) return false;
            bool point=false; int decimals=0;
            for (int i=0;i<text.Length;i++)
            {
                char c=text[i];
                if(c=='-' && i==0) continue;
                if(c=='.' && !point) { point=true; continue; }
                if(c<'0' || c>'9' || (point && ++decimals>3)) return false;
            }
            return true;
        }
        public static double Parse(string text,string source)
        {
            if(!IsDraft(text) || !double.TryParse(text,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out double value))
                throw new FormulaError(source,"Enter a decimal number, up to 3 decimal places and 12 characters.");
            if(Math.Abs(value)>MaximumLiteral) throw new FormulaError(source,"Input magnitude cannot exceed 1000000.");
            return value;
        }
        public static void Check(double value,InputDefinition slot,string source)
        {
            double minimum=slot.Dimension==Dimension.Mass?PhysicsLimits.MinimumMass:slot.Positive?.001:slot.Nonnegative || slot.Dimension==Dimension.Time || slot.Dimension==Dimension.Force?0:-MaximumResult;
            double maximum=slot.Dimension==Dimension.Mass?PhysicsLimits.MaximumMass:slot.Dimension==Dimension.Time?PhysicsLimits.MaximumProgramSeconds:MaximumResult;
            if(value<minimum || value>maximum || !FormulaEvaluator.Finite(value))
                throw new FormulaError(source,$"Allowed range: {minimum.ToString("0.###",CultureInfo.InvariantCulture)} to {maximum.ToString("0.###",CultureInfo.InvariantCulture)} {FormulaCatalog.Unit(slot.Dimension)}.");
        }
        public static string Normalize(double value)=>value==0?"0":value.ToString("0.###",CultureInfo.InvariantCulture);
    }
}
