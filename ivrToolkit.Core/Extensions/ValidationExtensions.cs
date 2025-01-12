using System;

namespace ivrToolkit.Core.Extensions;

public static class ValidationExtensions
{
    public static T ThrowIfNull<T>(this T parameter, string parameterName) where T : class
    {
        if (parameter == null)
        {
            throw new ArgumentNullException(parameterName);
        }

        return parameter;
    }

    public static string ThrowIfNullOrEmpty(this string parameter, string parameterName)
    {
        if (string.IsNullOrEmpty(parameter))
        {
            if (parameter == null) throw new ArgumentNullException();
            throw new ArgumentException(parameterName);
        }

        return parameter;
    }

    public static string ThrowIfNullOrWhiteSpace(this string parameter, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(parameter))
        {
            if (parameter == null) throw new ArgumentNullException();
            throw new ArgumentException(parameterName);
        }

        return parameter;
    }

    public static T ThrowIfLessThanOrEqualTo<T>(this T value, T index, string parameterName) where T : IComparable<T>
    {
        if (value.CompareTo(index) <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"Value must be greater than {index}");
        }

        return value;
    }

}