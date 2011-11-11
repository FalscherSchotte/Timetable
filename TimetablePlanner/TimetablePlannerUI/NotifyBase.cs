using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace TimetablePlannerUI
{
    public class NotifyBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePropertyChanged(Expression<Func<object>> locator)
        {
            LambdaExpression lambdaExp = (LambdaExpression)locator;
            if (lambdaExp.Body is MemberExpression)
                RaisePropertyChanged(((MemberExpression)lambdaExp.Body).Member.Name);
            else if (lambdaExp.Body is UnaryExpression)
                RaisePropertyChanged(((MemberExpression)((UnaryExpression)lambdaExp.Body).Operand).Member.Name);
            else
                throw new Exception("Undefined expression type");
        }
    }
}
