using System;
using System.Reflection;

namespace Betlln.Data.Integration.Core
{
    internal class StaticBinder : Binder
    {
        public StaticBinder(Type staticType)
            : base(null, staticType.Name, staticType.GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            if (!IsStatic(staticType))
            {
                throw new ArgumentException();
            }
        }

        //thanks to http://dotneteers.net/blogs/divedeeper/archive/2008/08/04/QueryingStaticClasses.aspx
        private bool IsStatic(Type type)
        {
            return type.IsAbstract && type.IsSealed;
        }
    }
}