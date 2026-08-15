namespace Test.Shared
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Thrown when a Touchstone test assertion fails. Touchstone treats any exception thrown from a
    /// test body as a failure, capturing the full message and stack trace.
    /// </summary>
    public sealed class TestAssertionException : Exception
    {
        /// <summary>Create a new assertion failure.</summary>
        /// <param name="message">Failure detail.</param>
        public TestAssertionException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Minimal, dependency-free assertion helpers used by the shared test descriptors so the same
    /// assertions run identically under the CLI runner, xUnit, and NUnit.
    /// </summary>
    public static class TestAssert
    {
        /// <summary>Assert that a condition holds.</summary>
        public static void True(bool condition, string message)
        {
            if (!condition) throw new TestAssertionException("Expected condition to be true: " + message);
        }

        /// <summary>Assert that a condition is false.</summary>
        public static void False(bool condition, string message)
        {
            if (condition) throw new TestAssertionException("Expected condition to be false: " + message);
        }

        /// <summary>Assert that a reference is not null.</summary>
        public static void NotNull(object? value, string message)
        {
            if (value is null) throw new TestAssertionException("Expected non-null value: " + message);
        }

        /// <summary>Assert equality using the default equality comparer.</summary>
        public static void Equal<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new TestAssertionException(
                    "Expected [" + Fmt(expected) + "] but got [" + Fmt(actual) + "]: " + message);
            }
        }

        /// <summary>
        /// Assert that <paramref name="action"/> throws an exception assignable to
        /// <typeparamref name="TException"/> and return the caught exception for further inspection.
        /// </summary>
        public static TException Throws<TException>(Action action, string message)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException ex)
            {
                return ex;
            }
            catch (Exception other)
            {
                throw new TestAssertionException(
                    "Expected " + typeof(TException).Name + " but got " + other.GetType().Name +
                    " (" + other.Message + "): " + message);
            }

            throw new TestAssertionException(
                "Expected " + typeof(TException).Name + " but no exception was thrown: " + message);
        }

        /// <summary>Assert that <paramref name="action"/> does not throw any exception.</summary>
        public static void DoesNotThrow(Action action, string message)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                throw new TestAssertionException(
                    "Expected no exception but got " + ex.GetType().Name + " (" + ex.Message + "): " + message);
            }
        }

        private static string Fmt(object? value)
        {
            return value is null ? "null" : value.ToString() ?? "null";
        }
    }
}
