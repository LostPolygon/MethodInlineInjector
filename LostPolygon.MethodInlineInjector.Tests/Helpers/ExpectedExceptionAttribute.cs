using System;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace NUnit.Framework {
    /// <summary>
    ///     A simple ExpectedExceptionAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ExpectedExceptionAttribute : NUnitAttribute, IWrapTestMethod {
        private readonly Type _expectedExceptionType;

        public ExpectedExceptionAttribute(Type type) {
            _expectedExceptionType = type;
        }

        public TestCommand Wrap(TestCommand command) {
            return new ExpectedExceptionCommand(command, _expectedExceptionType);
        }

        private class ExpectedExceptionCommand : DelegatingTestCommand {
            private readonly Type _expectedType;

            public ExpectedExceptionCommand(TestCommand innerCommand, Type expectedType)
                : base(innerCommand) {
                _expectedType = expectedType;
            }

            public override TestResult Execute(TestExecutionContext context) {
                Type caughtType = null;
                Exception caughtEx = null;

                try {
                    innerCommand.Execute(context);
                } catch (Exception ex) {
                    if (ex is NUnitException) {
                        ex = ex.InnerException;
                    }
                    caughtEx = ex;
                    caughtType = caughtEx.GetType();
                }

                if (caughtType == _expectedType) {
                    context.CurrentResult.SetResult(ResultState.Success, $"Exception message: {Environment.NewLine}{caughtEx}");
                } else if (caughtType != null) {
                    context.CurrentResult.SetResult(ResultState.Failure,
                        $"Expected {_expectedType.Name} but got {caughtType.Name}, exception message: {Environment.NewLine}{caughtEx}");
                } else {
                    context.CurrentResult.SetResult(ResultState.Failure, $"Expected {_expectedType.Name} but no exception was thrown");
                }

                return context.CurrentResult;
            }
        }
    }
}