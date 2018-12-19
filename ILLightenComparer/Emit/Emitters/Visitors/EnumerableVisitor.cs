﻿using System.Reflection.Emit;
using ILLightenComparer.Emit.Emitters.Comparisons;
using ILLightenComparer.Emit.Emitters.Variables;
using ILLightenComparer.Emit.Extensions;
using ILLightenComparer.Emit.Reflection;

namespace ILLightenComparer.Emit.Emitters.Visitors
{
    internal sealed class EnumerableVisitor
    {
        private const int LocalX = Arg.X; // 1
        private const int LocalY = Arg.Y; // 2
        private const int LocalDoneX = 3;
        private const int LocalDoneY = 4;

        private readonly CompareVisitor _compareVisitor;
        private readonly Converter _converter;
        private readonly VariableLoader _loader;
        private readonly StackVisitor _stackVisitor;

        public EnumerableVisitor(
            StackVisitor stackVisitor,
            CompareVisitor compareVisitor,
            VariableLoader loader,
            Converter converter)
        {
            _compareVisitor = compareVisitor;
            _loader = loader;
            _converter = converter;
            _stackVisitor = stackVisitor;
        }

        public ILEmitter Visit(EnumerableComparison comparison, ILEmitter il)
        {
            var variable = comparison.Variable;
            il.DefineLabel(out var gotoNextMember)
              .DefineLabel(out var startLoop)
              .DefineLabel(out var returnResult)
              .DeclareLocal(typeof(int), 0, out var result);

            variable.Load(_loader, il, Arg.X).Store(variable.VariableType, LocalX, out var xEnumerable);
            variable.Load(_loader, il, Arg.Y).Store(variable.VariableType, LocalY, out var yEnumerable);

            il.EmitCheckReferenceComparison(xEnumerable, yEnumerable, gotoNextMember);

            il.LoadLocal(xEnumerable)
              .Call(comparison.GetEnumeratorMethod)
              .Store(comparison.EnumeratorType, LocalX, out var xEnumerator)
              .LoadLocal(yEnumerable)
              .Call(comparison.GetEnumeratorMethod)
              .Store(comparison.EnumeratorType, LocalY, out var yEnumerator);

            Loop(il, xEnumerator, yEnumerator, result, startLoop, returnResult, gotoNextMember, variable);

            il.MarkLabel(returnResult)
              .LoadLocal(result)
              .Return();

            return il.MarkLabel(gotoNextMember);
        }

        private void Loop(
            ILEmitter il,
            LocalBuilder xEnumerator,
            LocalBuilder yEnumerator,
            LocalBuilder result,
            Label startLoop,
            Label returnResult,
            Label gotoNextMember,
            IVariable variable)
        {
            //il.BeginExceptionBlock(); // todo: think how to use it, the problem now with inner `return` statements, it has to be `leave` instruction
            il.MarkLabel(startLoop);

            var (xDone, yDone) = EmitMoveNext(il, xEnumerator, yEnumerator);
            EmitIfLoopIsDone(il, xDone, yDone, result, returnResult, gotoNextMember);

            var itemComparison = _converter.CreateEnumerableItemComparison(
                variable.OwnerType,
                xEnumerator,
                yEnumerator);

            itemComparison.LoadVariables(_stackVisitor, il, gotoNextMember);
            itemComparison.Accept(_compareVisitor, il)
                          .Store(result)
                          .LoadLocal(result)
                          .Branch(OpCodes.Brfalse, startLoop)
                          .Branch(OpCodes.Leave_S, returnResult);

            //il.BeginFinallyBlock();
            EmitDisposeEnumerators(il, xEnumerator, yEnumerator);

            //il.EndExceptionBlock();
        }

        private static void EmitIfLoopIsDone(
            ILEmitter il,
            LocalBuilder xDone,
            LocalBuilder yDone,
            LocalBuilder result,
            Label returnResult,
            Label gotoNextMember)
        {
            il.LoadLocal(xDone)
              .Branch(OpCodes.Brfalse_S, out var checkY)
              .LoadLocal(yDone)
              .Branch(OpCodes.Brfalse_S, out var returnM1)
              .Branch(OpCodes.Leave, gotoNextMember)
              .MarkLabel(returnM1)
              .LoadConstant(-1)
              .Store(result)
              .Branch(OpCodes.Leave, returnResult)
              .MarkLabel(checkY)
              .LoadLocal(yDone)
              .Branch(OpCodes.Brfalse_S, out var compare)
              .LoadConstant(1)
              .Store(result)
              .Branch(OpCodes.Leave, returnResult)
              .MarkLabel(compare);
        }

        private static (LocalBuilder xDone, LocalBuilder yDone) EmitMoveNext(
            ILEmitter il,
            LocalBuilder xEnumerator,
            LocalBuilder yEnumerator)
        {
            il.LoadLocal(xEnumerator)
              .Call(Method.MoveNext)
              .LoadConstant(0)
              .Emit(OpCodes.Ceq)
              .Store(typeof(int), LocalDoneX, out var xDone)
              .LoadLocal(yEnumerator)
              .Call(Method.MoveNext)
              .LoadConstant(0)
              .Emit(OpCodes.Ceq)
              .Store(typeof(int), LocalDoneY, out var yDone);

            return (xDone, yDone);
        }

        private static void EmitDisposeEnumerators(
            ILEmitter il,
            LocalBuilder xEnumerator,
            LocalBuilder yEnumerator)
        {
            il.LoadLocal(xEnumerator)
              .Branch(OpCodes.Brfalse_S, out var check)
              .LoadLocal(xEnumerator)
              .Call(Method.Dispose)
              .MarkLabel(check)
              .LoadLocal(yEnumerator)
              .Branch(OpCodes.Brfalse_S, out var next)
              .LoadLocal(yEnumerator)
              .Call(Method.Dispose)
              .MarkLabel(next);
        }
    }
}
