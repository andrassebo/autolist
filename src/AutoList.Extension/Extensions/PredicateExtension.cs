namespace AutoList.Base.Extensions
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Linq.Expressions;
   using System.Reflection;
   using System.Text;

   public class PredicateExtension
   {
      public static Predicate<object> BuildFilterPredicate<TItem>(Expression<Func<object, object>> propertySelector, object value, MethodInfo methodInfo, bool matchCase)
      {                  
         Expression operand = (propertySelector.Body as UnaryExpression).Operand;
         var constant = Expression.Constant(value, operand.Type);
         Expression operandWithMethod = Expression.Call(operand, methodInfo, constant);

         var result = Expression.Lambda<Predicate<object>>(operandWithMethod, propertySelector.Parameters[0]);

         return result.Compile();
      }
   }
}
