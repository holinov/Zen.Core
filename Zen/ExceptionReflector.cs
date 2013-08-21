using System;
using System.Collections;
using System.Text;

namespace Zen
{
    /// <summary>
    /// Класс позволяющий построить текстовое отражение Exception
    /// </summary>
    public sealed class ExceptionReflector
    {        
        /// <summary>
        /// Сконструировать отразитель ошибок
        /// </summary>
        /// <param name="exception">Ошибка</param>
        public ExceptionReflector(Exception exception)
        {
            StringBuilder sb=new StringBuilder();
            ReflectException(exception, 0, ref sb);
            _reflectedText = sb.ToString();
        }

        private string _reflectedText;
        private const string paddingString = "     ";

        /// <summary>
        /// Отражение ошибки
        /// </summary>
        public string ReflectedText
        {
            get { return _reflectedText; }
        }

        private static void ReflectException(Exception ex,int lvl, ref StringBuilder stringBuilder)
        {
            string pad="";
            for(int i=0;i<lvl;i++)
                pad += paddingString;
            stringBuilder.AppendLine("Exception type: " + ex.GetType().Name);
            //stringBuilder.AppendLine(pad + ex.Message);)
            foreach(var field in ex.GetType().GetProperties())
            {
                if (field.Name != "InnerException")
                {
                    var val = field.GetValue(ex, null);
                    if (val != null)
                    {
                        string fVal = val.ToString();
                        if (!string.IsNullOrEmpty(fVal) && fVal == " ") 
                        {
                            var ienum = val as IEnumerable;
                            Type valType = val.GetType();

                            if (ienum != null 
                                && !valType.IsPrimitive 
                                && valType != typeof(string))
                            {
                                stringBuilder.AppendLine(pad +"["+ field.Name + "] = {");
                                foreach (var variable in ienum)
                                {
                                    stringBuilder.AppendLine(pad + paddingString + "{1}" + variable);
                                }
                                stringBuilder.AppendLine(pad + "}");
                            }
                            else
                            {
                                stringBuilder.AppendFormat("{2}[{0}] = \"{1}\"", field.Name, fVal, pad);
                                stringBuilder.AppendLine();                                
                            }
                        }
                        else
                        {
                            stringBuilder.AppendFormat("{1}[{0}] = \"{2}\"", field.Name, pad, val);

                        }
                    }
                    else
                    {
                        stringBuilder.AppendFormat("{1}[{0}] = \"{2}\"", field.Name, pad, val);
                    }
                    stringBuilder.AppendLine();
                }
            }            
            if (ex.InnerException != null)
            {
                stringBuilder.AppendLine(pad + "InnerException" + "= {");
                ReflectException(ex.InnerException, lvl + 1, ref stringBuilder);
                stringBuilder.AppendLine(pad + "}");
            }
        }
    }
}
