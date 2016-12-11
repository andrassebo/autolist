using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace AutoList.Base.Extensions
{
   public static class TypeExtensions
   {
      /// <summary>
      /// Retrieves every property name and a property selector func filtered by the predicate.
      /// </summary>
      /// <typeparam name="TProperty">Item type to inspect</typeparam>
      /// <param name="propertyPredicate">Property selector</param>
      /// <returns>A dictionary with the property name and the property selector func in pair</returns>
      public static Dictionary<string, Expression<Func<object, object>>> GetPropertySelectorsWithName<TProperty>(Func<PropertyInfo, bool> propertyPredicate)
      {
         var result = new Dictionary<string, Expression<Func<object, object>>>();

         foreach (var p in GetProperties<TProperty>(propertyPredicate))
         {
            var exp = GetPropertySelector<TProperty>(p);
            result.Add(p.Name, exp);
         }

         return result;
      }

      public static Dictionary<string, Expression<Func<object, object>>> GetPropertySelectorsWithName<TProperty>()
      {
         return GetPropertySelectorsWithName<TProperty>(p => true);
      }
     
      private static IEnumerable<PropertyInfo> GetProperties<TProperty>(Func<PropertyInfo, bool> propertyPredicate)
      {
         var properties = typeof(TProperty).GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
         var filteredProperties = properties.Where(propertyPredicate);

         foreach (var p in filteredProperties)
         {
            yield return p;
         }
      }

      private static Expression<Func<object, object>> GetPropertySelector<TProperty>(PropertyInfo p)
      {
         var parameter = Expression.Parameter(typeof(object), "item");
         var cast = Expression.TypeAs(parameter, typeof(TProperty));
         var getterBody = Expression.Property(cast, p);
         var bodyAsObject = Expression.TypeAs(getterBody, typeof(object));

         return Expression.Lambda<Func<object, object>>(bodyAsObject, parameter);
      }

      public static IEnumerable<MethodInfo> GetFilterMethods<T>()
      {
         return GetFilterMethods(typeof(T));
      }

      public static IEnumerable<MethodInfo> GetFilterMethods(Type type)
      {
         var methods = type.GetMethods().Where(m =>
            m.ContainsGenericParameters == false &&
            m.ReturnType == typeof(bool) &&
            m.IsPublic && m.IsAbstract == false
            );

         foreach (var m in methods)
         {
            var parameters = m.GetParameters();

            if (parameters.Count() == 1 && parameters[0].ParameterType == type)
            {
               yield return m;
            }
         }
      }
   }
}
