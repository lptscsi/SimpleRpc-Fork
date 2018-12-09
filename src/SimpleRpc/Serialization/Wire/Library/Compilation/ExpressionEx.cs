// -----------------------------------------------------------------------
//   <copyright file="ExpressionEx.cs" company="Asynkron HB">
//       Copyright (C) 2015-2017 Asynkron HB All rights reserved
//   </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq.Expressions;
using System.Reflection;
using SimpleRpc.Serialization.Wire.Library.Extensions;

namespace SimpleRpc.Serialization.Wire.Library.Compilation
{
    public static class ExpressionEx
    {
        public static ConstantExpression ToConstant(this object self)
        {
            return Expression.Constant(self);
        }

        public static Expression GetNewExpression(Type type)
        {
            if (type.GetTypeInfo().IsValueType)
            {
                var x = Expression.Constant(Activator.CreateInstance(type));
                var convert = Expression.Convert(x, typeof(object));
                return convert;
            }

            var emptyObjectMethod = typeof(TypeEx).GetTypeInfo().GetMethod(nameof(TypeEx.GetEmptyObject));
            var emptyObject = Expression.Call(null, emptyObjectMethod, type.ToConstant());

            return emptyObject;
        }
    }
}