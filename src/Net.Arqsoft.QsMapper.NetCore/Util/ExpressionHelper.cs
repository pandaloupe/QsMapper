namespace Net.Arqsoft.QsMapper.Util;

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

public class ExpressionHelper {
    public static string GetPropertyName<T>(Expression<Func<T, object>> propertyExpression) {
        var body = propertyExpression.Body as MemberExpression;

        if (body == null) {
            var ubody = (UnaryExpression)propertyExpression.Body;
            body = (MemberExpression)ubody.Operand;
        }

        return body.Member.Name;
    }

    public static string GetPropertyName<T, T1>(Expression<Func<T, T1>> propertyExpression) {
        return ((MemberExpression)propertyExpression.Body).Member.Name;
    }

    public static string GetPropertyName<T>(Expression<Func<T, int>> propertyExpression) {
        return ((MemberExpression)propertyExpression.Body).Member.Name;
    }

    public static string GetPropertyName<T>(Expression<Func<T, int?>> propertyExpression) {
        return ((MemberExpression)propertyExpression.Body).Member.Name;
    }
    public static string GetPropertyName<T>(Expression<Func<T, string>> propertyExpression) {
        return ((MemberExpression)propertyExpression.Body).Member.Name;
    }

    public static string GetPropertyName<T, T1>(Expression<Func<T, IEnumerable<T1>>> propertyExpression) {
        return ((MemberExpression)propertyExpression.Body).Member.Name;
    }
}