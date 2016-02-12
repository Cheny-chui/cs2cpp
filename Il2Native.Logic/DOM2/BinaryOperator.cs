﻿namespace Il2Native.Logic.DOM2
{
    using System;
    using System.Diagnostics;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    public class BinaryOperator : Expression
    {
        public override Kinds Kind
        {
            get { return Kinds.BinaryOperator; }
        }

        internal BinaryOperatorKind OperatorKind { get; set; }

        public Expression Left { get; set; }

        public Expression Right { get; set; }

        internal void Parse(BoundBinaryOperator boundBinaryOperator)
        {
            base.Parse(boundBinaryOperator);
            this.OperatorKind = boundBinaryOperator.OperatorKind;

            // special case for PointerSubtraction 
            if (this.OperatorKind == BinaryOperatorKind.Division)
            {
                var boundBinaryOperator2 = boundBinaryOperator.Left as BoundBinaryOperator;
                if (boundBinaryOperator2 != null && boundBinaryOperator2.OperatorKind == BinaryOperatorKind.PointerSubtraction)
                {
                    this.Parse(boundBinaryOperator2);
                    return;
                }
            }

            this.Left = Deserialize(boundBinaryOperator.Left) as Expression;
            Debug.Assert(this.Left != null);
            this.Right = Deserialize(boundBinaryOperator.Right) as Expression;
            Debug.Assert(this.Right != null);
        }

        internal override void Visit(Action<Base> visitor)
        {
            base.Visit(visitor);
            this.Left.Visit(visitor);
            this.Right.Visit(visitor);
        }

        internal override void WriteTo(CCodeWriterBase c)
        {
            bool changedLeft;
            this.Left = AdjustEnumType(this.Left, out changedLeft);
            bool changedRight;
            this.Right = AdjustEnumType(this.Right, out changedRight);

            var castOfResult = (changedRight || changedLeft) && Type.TypeKind == TypeKind.Enum;
            if (castOfResult)
            {
                c.TextSpan("((");
                c.WriteType(Type);
                c.TextSpan(")(");
            }

            c.WriteExpressionInParenthesesIfNeeded(this.Left);
            c.WhiteSpace();
            this.WriteOperator(c);
            c.WhiteSpace();
            c.WriteExpressionInParenthesesIfNeeded(this.Right);

            if (castOfResult)
            {
                c.TextSpan("))");
            }
        }

        private void WriteOperator(CCodeWriterBase c)
        {
            switch (this.OperatorKind & (BinaryOperatorKind.OpMask | BinaryOperatorKind.Logical))
            {
                case BinaryOperatorKind.Multiplication:
                    c.TextSpan("*");
                    break;

                case BinaryOperatorKind.Addition:
                    c.TextSpan("+");
                    break;

                case BinaryOperatorKind.Subtraction:
                    c.TextSpan("-");
                    break;

                case BinaryOperatorKind.Division:
                    c.TextSpan("/");
                    break;

                case BinaryOperatorKind.Remainder:
                    c.TextSpan("%");
                    break;

                case BinaryOperatorKind.LeftShift:
                    c.TextSpan("<<");
                    break;

                case BinaryOperatorKind.RightShift:
                    c.TextSpan(">>");
                    break;

                case BinaryOperatorKind.Equal:
                    c.TextSpan("==");
                    break;

                case BinaryOperatorKind.NotEqual:
                    c.TextSpan("!=");
                    break;

                case BinaryOperatorKind.GreaterThan:
                    c.TextSpan(">");
                    break;

                case BinaryOperatorKind.LessThan:
                    c.TextSpan("<");
                    break;

                case BinaryOperatorKind.GreaterThanOrEqual:
                    c.TextSpan(">=");
                    break;

                case BinaryOperatorKind.LessThanOrEqual:
                    c.TextSpan("<=");
                    break;

                case BinaryOperatorKind.And:
                    c.TextSpan("&");
                    break;

                case BinaryOperatorKind.Xor:
                    c.TextSpan("^");
                    break;

                case BinaryOperatorKind.Or:
                    c.TextSpan("|");
                    break;

                case BinaryOperatorKind.LogicalAnd:
                    c.TextSpan("&&");
                    break;

                case BinaryOperatorKind.LogicalOr:
                    c.TextSpan("||");
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}
